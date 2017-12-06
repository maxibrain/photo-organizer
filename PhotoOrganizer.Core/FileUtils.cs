using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoOrganizer
{
    internal static class FileUtils
    {
        public static string PickNewFileName(string initialFileName, IList<string> existingNames, Func<int, string> postfixFunc)
        {
            var fullName = initialFileName;
            var name = Path.GetFileNameWithoutExtension(fullName);
            var ext = Path.GetExtension(fullName);
            var i = 1;
            while (existingNames.Any(x => string.Equals(x, fullName, StringComparison.OrdinalIgnoreCase)))
            {
                fullName = $"{name}{postfixFunc(i++)}{ext}";
            }
            return fullName;
        }
    }
}
