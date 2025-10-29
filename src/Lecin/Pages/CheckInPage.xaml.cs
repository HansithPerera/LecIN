using Supabase.Postgrest;
using SupabaseShared.Models;
using Client = Supabase.Client;
using Lecin.Services;

namespace Lecin.Pages;

public partial class CheckInPage : ContentPage
{
    private bool _hasPhoto = false;
    private readonly Client _client;
    private readonly AuthService _auth;

    private static readonly string[] Quotes = new[]
    {
        "Every day is a chance to learn.",
        "Small steps lead to big results.",
        "Consistency beats intensity.",
        "Show up, grow up.",
        "Keep going, you’re doing great!"
    };

    public CheckInPage(Client client, AuthService auth)
    {
        _client = client;
        _auth = auth;
        InitializeComponent();
        UpdateTime();
    }

    // Parameterless constructor to support Shell route activation without DI
    public CheckInPage() : this(
        (Client)(Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(Client))
                  ?? throw new InvalidOperationException("Supabase Client not available")),
        (AuthService)(Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(AuthService))
                       ?? throw new InvalidOperationException("AuthService not available")))
    {
    }

    private void UpdateTime()
    {
        TimeLabel.Text = $"Time: {DateTime.Now:HH:mm:ss}";

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            MainThread.BeginInvokeOnMainThread(() => UpdateTime());
        });
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                PhotoLabel.Text = "Photo Captured!";
                CaptureButton.Text = "Recapture";
                _hasPhoto = true;
                ConfirmButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Camera error: {ex.Message}", "OK");
        }
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_hasPhoto)
            {
                await DisplayAlert("Notice", "Please capture a photo before confirming.", "OK");
                return;
            }

            if (_auth.CurrentUserId is null)
            {
                await DisplayAlert("Error", "You must be signed in as a student to check in.", "OK");
                return;
            }

            var userId = _auth.CurrentUserId.Value;
            var now = DateTimeOffset.Now;
            var startUpper = now.AddMinutes(15);
            var endLower = now.AddMinutes(-15);

            // Find a class currently in the check-in window that the student is enrolled in
            var classResult = await _client
                .From<Class>()
                .Select("*")
                .Where(c => c.StartTime <= startUpper)
                .Where(c => c.EndTime >= endLower)
                .Order(nameof(Class.StartTime), Constants.Ordering.Ascending)
                .Limit(1)
                .Get();

            var currentClass = classResult.Models.FirstOrDefault();
            if (currentClass == null)
            {
                await DisplayAlert("Not Available", "No class is in the check-in window right now.", "OK");
                return;
            }

            // Pre-check: already checked in for this class?
            var existing = await _client
                .From<Attendance>()
                .Select("*")
                .Where(a => a.StudentId == userId)
                .Where(a => a.ClassId == currentClass.Id)
                .Single();

            if (existing != null)
            {
                await DisplayAlert("Already Checked In", "You have already checked into this class.", "OK");
                return;
            }

            var attendance = new Attendance
            {
                StudentId = userId,
                ClassId = currentClass.Id,
                Reason = "self-checkin",
                Timestamp = DateTimeOffset.Now
            };

            // Insert attendance (RLS will validate eligibility and prevent duplicates)
            await _client.From<Attendance>().Insert(attendance);

            // Fetch student name for greeting
            var student = await _client.From<SupabaseShared.Models.Student>()
                .Where(s => s.Id == userId)
                .Single();

            var name = student?.FirstName ?? "Student";
            var rand = new Random();
            var quote = Quotes[rand.Next(Quotes.Length)];

            await DisplayAlert("Success", $"Welcome, {name}!\n{quote}", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            var msg = ex.Message ?? string.Empty;
            if (msg.Contains("23505", StringComparison.OrdinalIgnoreCase) || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Already Checked In", "You have already checked into this class.", "OK");
                return;
            }
            if (msg.Contains("row-level security", StringComparison.OrdinalIgnoreCase) || msg.Contains("42501"))
            {
                await DisplayAlert("Not Allowed", "Check-in is not allowed right now. Make sure you are enrolled and within the check-in window.", "OK");
                return;
            }
            await DisplayAlert("Error", $"Check-in failed: {ex.Message}", "OK");
        }
    }
}
