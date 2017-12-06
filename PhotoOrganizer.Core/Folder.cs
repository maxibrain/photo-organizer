using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    public class Folder : IFolderItem, IRenameable
    {
        public string Name { get; private set; }
        public DateTimeOffset FromDate { get; }
        public DateTimeOffset ToDate { get; }

        private readonly List<IFolderItem> _items = new List<IFolderItem>();
        public IEnumerable<IFolderItem> Items => _items.ToArray();

        public Folder(string name, DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            Name = name;
            FromDate = fromDate;
            ToDate = toDate;
        }

        public Photo AddPhoto(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var photo = new Photo(fileInfo);
            photo.NewFileName = PickName(photo.NewFileName);
            _items.Add(photo);
            return photo;
        }

        public Folder AddFolder(string name, DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            if (fromDate.Year != toDate.Year) throw new ArgumentException("Cannot create a folder for several years");
            var nameBuilder = new StringBuilder(fromDate.Year.ToString());
            if (fromDate.Month == toDate.Month)
            {
                nameBuilder = nameBuilder.Append($"-{fromDate.Month:D2}");
            }
            if (fromDate.Day == toDate.Day)
            {
                nameBuilder = nameBuilder.Append($"-{fromDate.Day:D2}");
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                nameBuilder = nameBuilder.Append($" {name}");
            }
            var newName = PickName(nameBuilder.ToString());
            var folder = new Folder(newName, fromDate, toDate);
            _items.Add(folder);
            return folder;
        }

        private string PickName(string intialName)
        {
            return FileUtils.PickNewFileName(intialName, _items.Select(x => x.Name).ToArray(), i => $"_{i}");
        }

        public void AddItem(IFolderItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _items.Add(item);
        }

        public void RemoveItem(IFolderItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _items.Remove(item);
        }

        public async Task CopyAsync(string destPath, IProgress<Photo> progress, CancellationToken cancellationToken)
        {
            var exceptions = new List<FileLoadException>();
            foreach (var item in Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item is Folder folder)
                {
                    var destFolder = new DirectoryInfo(Path.Combine(destPath, folder.Name));
                    if (!destFolder.Exists)
                    {
                        destFolder.Create();
                    }
                    try
                    {
                        await folder.CopyAsync(destFolder.FullName, progress, cancellationToken);
                    }
                    catch (AggregateException e)
                    {
                        exceptions.AddRange(e.InnerExceptions.OfType<FileLoadException>());
                    }
                }
                else if (item is Photo photo)
                {
                    try
                    {
                        await photo.CopyAsync(destPath, progress, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(new FileLoadException("Failed to copy", photo.OriginalFileName, exception));
                    }
                }
            }
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void AdjustTime(TimeSpan span)
        {
            foreach (var item in Items)
            {
                item.AdjustTime(span);
                if (item is Photo photo)
                {
                    photo.NewFileName = PickName(photo.NewFileName);
                }
            }
        }

        public void RenameItem(IFolderItem item, string newName)
        {
            if (Items.Any(x => string.Equals(x.Name, newName)))
            {
                throw new ArgumentException($"Item with name {newName} already exist", nameof(newName));
            }
            ((IRenameable)item).Rename(newName);
        }

        void IRenameable.Rename(string newName)
        {
            Name = newName;
        }

        public IEnumerable<Photo> AllPhotos
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item is Photo photo) yield return photo;
                    if (item is Folder folder)
                    {
                        foreach (var subPhoto in folder.AllPhotos)
                        {
                            yield return subPhoto;
                        }
                    }
                }
            }
        }
    }
}