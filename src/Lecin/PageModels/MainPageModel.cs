using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.PageModels;

public partial class MainPageModel : ObservableObject
{
    private readonly Client _client;

    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] private ObservableCollection<Course> _courses = [];
    private bool _dataLoaded;

    [ObservableProperty] private bool _isBusy;
    private bool _isNavigatedTo;

    [ObservableProperty] private bool _isRefreshing;

    /// <inheritdoc />
    public MainPageModel(ModalErrorHandler errorHandler, Client client)
    {
        _client = client;
        RefreshCommand = new AsyncRelayCommand(Refresh);
        _errorHandler = errorHandler;
    }

    public ICommand RefreshCommand { get; set; }

    private async Task Refresh()
    {
        IsRefreshing = true;
        await LoadDataAsync();
        IsRefreshing = false;
    }

    public async Task LoadDataAsync()
    {
        if (_dataLoaded && _isNavigatedTo) return;

        IsBusy = true;
        try
        {
            var courses = await _client.From<Course>()
                .Select("*")
                .Get();

            Courses = new ObservableCollection<Course>(courses.Models);
            _dataLoaded = true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NavigatedTo()
    {
        _isNavigatedTo = true;
    }

    [RelayCommand]
    private void NavigatedFrom()
    {
        _isNavigatedTo = false;
    }
}