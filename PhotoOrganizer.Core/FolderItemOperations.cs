using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    public static class FolderItemOperations
    {
        public static void Move(this IFolderItem item, Folder from, Folder to)
        {
            if (from == to) throw new ArgumentException("Source and destination folders are the same one");
            to.AddItem(item);
            from.RemoveItem(item);
        }
    }
}
