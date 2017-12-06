using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace PhotoOrganizer.ViewModel
{
    public sealed class ProjectViewModel : ViewModelBase
    {
        private readonly Stack<FolderItemViewModel> _navigationStack = new Stack<FolderItemViewModel>(3);

        private FolderItemViewModel _activeFolder;

        public FolderItemViewModel ActiveFolder
        {
            get => _activeFolder;
            private set { _activeFolder = value; RaisePropertyChanged(() => ActiveFolder); }
        }

        public ProjectViewModel(Project project)
        {
            var root = new FolderItemViewModel(project.Folder);
            OnNavigated(new FolderNavigatedMessage(root));
            MessengerInstance.Register<FolderNavigatedMessage>(this, OnNavigated);
            MessengerInstance.Register<NavigatedUpMessage>(this, OnNavigatedUp);
        }

        private void OnNavigatedUp(NavigatedUpMessage message)
        {
            if (_navigationStack.Count <= 1) return;
            var active = _navigationStack.Pop();
            ActiveFolder = _navigationStack.Peek();
        }

        private void OnNavigated(FolderNavigatedMessage message)
        {
            _navigationStack.Push(message.Item);
            ActiveFolder = message.Item;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            MessengerInstance.Unregister<FolderNavigatedMessage>(this);
            MessengerInstance.Unregister<NavigatedUpMessage>(this);
        }
    }
}
