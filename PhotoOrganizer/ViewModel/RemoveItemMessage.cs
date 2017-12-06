namespace PhotoOrganizer.ViewModel
{
    internal class RemoveItemMessage
    {
        public FolderItemViewModel Item { get; }

        public RemoveItemMessage(FolderItemViewModel item)
        {
            Item = item;
        }
    }
}