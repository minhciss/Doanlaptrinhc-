using CommunityToolkit.Mvvm.Input;
using VinhKhanhapp.Models;

namespace VinhKhanhapp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}