using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DamnSimpleFileManager
{
    /// <summary>
    /// Provides foreground brush based on the file system entry type.
    /// </summary>
    public class FileForegroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo || value is ParentDirectoryInfo)
            {
                return Brushes.LightSkyBlue;
            }
            return SystemColors.ControlTextBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
