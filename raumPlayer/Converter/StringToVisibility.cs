using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace raumPlayer.Converter
{
    public class StringToVisibility : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                // Collapse if string is NullOrEmpty
                return Visibility.Collapsed;
            }
            else { return Visibility.Visible; }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
