using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    public class Photo : IFolderItem, IRenameable
    {
        public FileInfo FileInfo { get; }

        public Photo(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            if (!fileInfo.Exists) throw new FileNotFoundException("Photo not found", fileInfo.FullName);
            OriginalFileName = fileInfo.FullName;
            var exif = new PhotoExifProvider(OriginalFileName);
            DateTaken = exif.DateTaken;
            ResetNewFileName();
        }

        public string OriginalFileName { get; }
        public DateTimeOffset? DateTaken { get; private set; }
        public string NewFileName { get; set; }

        public string Name => NewFileName;

        void IRenameable.Rename(string newName)
        {
            NewFileName = newName;
        }

        private void ResetNewFileName()
        {
            NewFileName = DateTaken.HasValue
                ? DateTaken.Value.ToString("yyyyMMdd_HHmmss") + FileInfo.Extension
                : FileInfo.Name;
        }

        public Task CopyAsync(string destPath, IProgress<Photo> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(this);
            return Task.Run(() => File.Copy(OriginalFileName, Path.Combine(destPath, NewFileName), false), cancellationToken);
        }

        public void AdjustTime(TimeSpan span)
        {
            DateTaken = DateTaken?.Add(span);
            ResetNewFileName();
        }
    }
}
