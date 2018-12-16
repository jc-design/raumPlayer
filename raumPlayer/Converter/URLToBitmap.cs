using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace raumPlayer.Converter
{
    public class UrlToBitmap : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string url;
            if (string.IsNullOrEmpty(value as string))
            {
                // Alternative UrlBitmap if string is NullOrEmpty
                url = "ms-appx:///Assets/Square150x150Logo.png";
            }
            else { url = value as string; }

            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
