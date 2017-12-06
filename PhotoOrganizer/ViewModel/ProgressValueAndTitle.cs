namespace PhotoOrganizer.ViewModel
{
    public class ProgressValueAndTitle
    {
        public ProgressValueAndTitle(double value, string title = null)
        {
            Value = value;
            Title = title;
        }

        public double Value { get; }
        public string Title { get; }
    }
}