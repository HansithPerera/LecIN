using CommunityToolkit.Mvvm.Input;
using Lecin.Models;

namespace Lecin.PageModels;

public interface IProjectTaskPageModel
{
    IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
    bool IsBusy { get; }
}