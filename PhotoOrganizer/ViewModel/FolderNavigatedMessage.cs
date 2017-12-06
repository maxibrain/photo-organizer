namespace PhotoOrganizer.ViewModel
{
    public class FolderNavigatedMessage
    {
        public FolderItemViewModel Item { get; }

        public FolderNavigatedMessage(FolderItemViewModel item)
        {
            Item = item;
        }
    }
}