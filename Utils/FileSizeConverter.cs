using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace DamnSimpleFileManager
{
    /// <summary>
    /// Converts <see cref="FileSystemInfo"/> to a formatted size string in MB or GB.
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FileInfo fi)
            {
                double sizeInMb = fi.Length / (1024.0 * 1024.0);
                if (sizeInMb >= 1024)
                {
                    return $"{sizeInMb / 1024:0.##} GB"; 
                }
                return $"{sizeInMb:0.##} MB";
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
