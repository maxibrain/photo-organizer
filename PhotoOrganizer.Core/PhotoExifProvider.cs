using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using photo.exif;

namespace PhotoOrganizer
{
    public class PhotoExifProvider
    {
        public string FilePath { get; }

        public DateTimeOffset? DateTaken
        {
            get
            {
                var value = FindValue("ExifDTOrig");
                if (string.IsNullOrEmpty(value)) return null;
                return DateTimeOffset.Parse(value,
                    new DateTimeFormatInfo() {TimeSeparator = ":", DateSeparator = ":"});
            }
        }

        private readonly Lazy<ExifItem[]> _data;

        public PhotoExifProvider(string filePath)
        {
            FilePath = filePath;
            _data = new Lazy<ExifItem[]>(Parse);
        }

        private ExifItem[] Parse()
        {
            try
            {
                return new Parser().Parse(FilePath).ToArray();
            }
            catch
            {
                return new ExifItem[0];
            }
        }

        private string FindValue(string title)
        {
            return _data.Value
                .Where(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value.ToString())
                .SingleOrDefault();
        }
    }
}
