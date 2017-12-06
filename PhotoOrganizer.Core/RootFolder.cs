using System;

namespace PhotoOrganizer
{
    public class RootFolder : Folder
    {
        public RootFolder() : base("", DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
        {
        }
    }
}