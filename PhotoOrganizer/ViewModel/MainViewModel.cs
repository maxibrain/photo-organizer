using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace PhotoOrganizer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ProjectViewModel _project = new ProjectViewModel(new Project());
        private ProgressViewModel _currentOperation;
        private MessageViewModel _currentMessage;

        public ProjectViewModel Project
        {
            get => _project;
            private set => Set(() => Project, ref _project, value);
        }

        public ProgressViewModel CurrentOperation
        {
            get { return _currentOperation; }
            private set { Set(() => CurrentOperation, ref _currentOperation, value); }
        }

        public MessageViewModel CurrentMessage
        {
            get { return _currentMessage; }
            private set { Set(() => CurrentMessage, ref _currentMessage, value); }
        }

        public ICommand NewProjectCommand { get; }

        public MainViewModel()
        {
            NewProjectCommand = new RelayCommand(NewProject);
        }

        private void NewProject()
        {
            Project.Cleanup();
            Project = new ProjectViewModel(new Project());
        }

        public async Task AddPhotosAsync(string[] filePaths)
        {
            CurrentOperation = new ProgressViewModel(new Progress<ProgressValueAndTitle>(), new CancellationTokenSource());
            try
            {
                await Project.ActiveFolder.AddPhotosAsync(filePaths, CurrentOperation.Progress,
                    CurrentOperation.CancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                CurrentMessage = new MessageViewModel("Operation cancelled");
            }
            catch (AggregateException e)
            {
                CurrentMessage = new MessageViewModel(
                    $"Failed to add {e.InnerExceptions.Count} photos:" +
                    $"{Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, e.InnerExceptions.OfType<FileLoadException>().Select(x => x.FileName))}");
            }
            finally
            {
                CurrentOperation.Complete();
            }
        }

        public async Task CopyAsync(string path)
        {
            CurrentOperation = new ProgressViewModel(new Progress<ProgressValueAndTitle>(), new CancellationTokenSource());
            try
            {
                await Project.ActiveFolder.CopyPhotosAsync(path, CurrentOperation.Progress,
                    CurrentOperation.CancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                CurrentMessage = new MessageViewModel("Operation cancelled");
            }
            catch (AggregateException e)
            {
                CurrentMessage = new MessageViewModel(
                    $"Failed to copy {e.InnerExceptions.Count} photos:" +
                    $"{Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, e.InnerExceptions.OfType<FileLoadException>().Select(x => x.FileName))}");
            }
            finally
            {
                CurrentOperation.Complete();
            }
        }
    }
}
