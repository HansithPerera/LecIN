using System.Collections.ObjectModel;
using System.Windows.Input;
using Backend.Api;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    
    private readonly OpenApiClient _client;

    /// <inheritdoc/>
    public MainPageModel(ModalErrorHandler errorHandler, OpenApiClient client)
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
            var courses = await _client.GetAllCoursesAsync();
            Courses = new ObservableCollection<Course>(courses);
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