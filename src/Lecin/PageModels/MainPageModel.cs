using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lecin.Models;
using SupabaseShared.Models;

namespace Lecin.PageModels;

public partial class MainPageModel : ObservableObject
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;

    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] bool _isBusy;

    [ObservableProperty] bool _isRefreshing;
    
    [ObservableProperty] ObservableCollection<Course> _courses = [];

    public ICommand RefreshCommand { get; set; }
    
    private readonly Supabase.Client _client;

    /// <inheritdoc/>
    public MainPageModel(ModalErrorHandler errorHandler, Supabase.Client client)
    {
        _client = client;
        RefreshCommand = new AsyncRelayCommand(Refresh);
        _errorHandler = errorHandler;
    }

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
    private void NavigatedTo() =>
        _isNavigatedTo = true;

    [RelayCommand]
    private void NavigatedFrom() =>
        _isNavigatedTo = false;
}