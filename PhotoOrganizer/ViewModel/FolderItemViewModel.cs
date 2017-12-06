using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace PhotoOrganizer.ViewModel
{
    public class FolderItemViewModel : ViewModelBase
    {
        private readonly IFolderItem _item;

        public string Name => _item.Name;

        private Folder Folder => IsFolder ? (Folder) _item : throw new InvalidOperationException("Not a folder");
        private Photo Photo => IsPhoto ? (Photo)_item : throw new InvalidOperationException("Not a photo");

        public FolderItemViewModel(IFolderItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _item = item;

            OpenCommand = new RelayCommand(Open);
            GroupCommand = new RelayCommand<ArrayList>(GroupItems, CanGroupItems);
            UpCommand = new RelayCommand(Up, CanUp);
            RemoveCommand = new RelayCommand(Remove);

            if (IsPhoto)
            {
                var filePath = Photo.OriginalFileName;
                OriginalName = Path.GetFileName(filePath);
            }
            else if (IsFolder)
            {
                MessengerInstance.Register<RemoveItemMessage>(this, OnItemRemoving);
            }
        }

        private void OnItemRemoving(RemoveItemMessage message)
        {
            Folder.RemoveItem(message.Item._item);
            Items.Remove(message.Item);
        }

        private void Remove()
        {
            if (IsFolder)
            {
                MessengerInstance.Unregister<RemoveItemMessage>(this);
            }
            MessengerInstance.Send(new RemoveItemMessage(this));
        }

        private bool CanUp()
        {
            return !(_item is RootFolder);
        }

        private void Up()
        {
            MessengerInstance.Send(new NavigatedUpMessage());
        }

        private bool CanGroupItems(ArrayList items)
        {
            return items != null && items.Count > 1;
        }

        private void GroupItems(ArrayList items)
        {
            var castedItems = items.OfType<FolderItemViewModel>().ToArray();
            var fromDate = castedItems.Min(x => x.FromDate);
            var toDate = castedItems.Max(x => x.ToDate);

            var newFolder = Folder.AddFolder("", fromDate, toDate);

            foreach (var item in castedItems)
            {
                item._item.Move(Folder, newFolder);
                Items.Remove(item);
            }

            var newFolderVm = new FolderItemViewModel(newFolder);
            Items.Add(newFolderVm);
        }

        private void Open()
        {
            if (IsPhoto)
            {
                Process.Start(Photo.OriginalFileName);
            }
            else
            {
                UpdateItems();
                MessengerInstance.Send(new FolderNavigatedMessage(this));
            }
        }

        private void UpdateItems()
        {
            Items.Clear();
            foreach (var item in Folder.Items)
            {
                Items.Add(new FolderItemViewModel(item));
            }
        }

        public ObservableCollection<FolderItemViewModel> Items { get; } = new ObservableCollection<FolderItemViewModel>();

        public bool IsFolder => _item is Folder;
        public bool IsPhoto => _item is Photo;

        public ICommand OpenCommand { get; }
        public ICommand GroupCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand RemoveCommand { get; }

        public string OriginalName { get; }

        public DateTimeOffset FromDate => IsFolder ? Folder.FromDate : Photo.DateTaken.GetValueOrDefault(DateTimeOffset.MinValue);
        public DateTimeOffset ToDate => IsFolder ? Folder.ToDate : Photo.DateTaken.GetValueOrDefault(DateTimeOffset.MinValue);
        public string OriginalPath => Photo.OriginalFileName;

        public async Task AddPhotoAsync(string filePath)
        {
            var photo = await Task.Run(() => Folder.AddPhoto(filePath));
            Items.Add(new FolderItemViewModel(photo));
        }

        public void Rename(FolderItemViewModel item, string newName)
        {
            Folder.RenameItem(item._item, newName);
            item.RaisePropertyChanged(() => Name);
        }

        public async Task AddPhotosAsync(string[] filePaths, IProgress<ProgressValueAndTitle> progress,
            CancellationToken cancellationToken)
        {
            var exceptions = new List<FileLoadException>();
            foreach (var filePath in filePaths)
            {
                progress.Report(new ProgressValueAndTitle(
                    100.0 * Array.IndexOf(filePaths, filePath) / filePaths.Length,
                    $"Adding {Path.GetFileName(filePath)}..."));

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await AddPhotoAsync(filePath);
                }
                catch (Exception exception)
                {
                    exceptions.Add(new FileLoadException("Failed to load", filePath, exception));
                }
            }
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public Task CopyPhotosAsync(string destPath, IProgress<ProgressValueAndTitle> progress, CancellationToken cancellationToken)
        {
            var photos = Folder.AllPhotos.ToArray();
            var fileProgress = new Progress<Photo>(photo =>
            {
                progress.Report(new ProgressValueAndTitle(
                    100.0 * Array.IndexOf(photos, photo) / photos.Length,
                    $"Copying {Path.GetFileName(photo.OriginalFileName)} to {Path.GetFileName(photo.NewFileName)}..."));
            });
            return Folder.CopyAsync(destPath, fileProgress, cancellationToken);
        }
    }
}