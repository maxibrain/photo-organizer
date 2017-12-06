using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    public interface IFolderItem
    {
        string Name { get; }
        Task CopyAsync(string destPath, IProgress<Photo> progress, CancellationToken cancellationToken);
        void AdjustTime(TimeSpan span);
    }
}