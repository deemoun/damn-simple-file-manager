using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace DamnSimpleFileManager
{
    /// <summary>
    /// Converts FileSystemInfo to a string representing either a folder marker or file extension.
    /// </summary>
    public class FileTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo)
            {
                return "🗂"; // folder icon with improved visibility
            }
            if (value is FileInfo fi)
            {
                return string.IsNullOrEmpty(fi.Extension) ? Localization.Get("FileType_Default") : fi.Extension.TrimStart('.').ToUpperInvariant();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
