using System;
using Windows.UI.Xaml.Data;

namespace raumPlayer.Converter
{
    public class IntToString : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int) { return string.Format("#{0:N0}", (int)value); }
            else { return string.Empty; }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
