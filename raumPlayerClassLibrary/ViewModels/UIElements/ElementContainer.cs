using Prism.Events;
using Prism.Unity.Windows;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Upnp;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace raumPlayer.ViewModels
{
    public class ElementContainer : ElementBase
    {
        public ElementContainer(IEventAggregator eventAggregatorInstance, DIDLContainer didl) :base(eventAggregatorInstance, didl)
        {
            //ImageArt = new BitmapImage(new Uri(AlbumArtUri, UriKind.Absolute));
            //ImageArt.ImageFailed += OnImageFailed;
            //ImageArt.ImageOpened += OnImageOpened;
        }

        private async  void OnImageOpened(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = sender as BitmapImage;
            StorageFile cachedFile = null;
            try
            {
                cachedFile = await StorageFile.GetFileFromPathAsync(this.AlbumArtUri);
            }
            catch (Exception)
            {
                // File doesn't exists
            }

            try
            {
                if (cachedFile != null)
                {
                    var randomAccessStream = await cachedFile.OpenReadAsync();
                    using (var stream = randomAccessStream)
                    {
                        //Create a decoder for the image
                        var decoder = await BitmapDecoder.CreateAsync(stream);

                        var pixels = await decoder.GetPixelDataAsync(
                                             BitmapPixelFormat.Rgba8,
                                             BitmapAlphaMode.Ignore,
                                             new BitmapTransform { ScaledHeight = 50, ScaledWidth = 50 },
                                             ExifOrientationMode.IgnoreExifOrientation,
                                             ColorManagementMode.DoNotColorManage);

                        //Get the bytes of the 1x1 scaled image
                        var bytes = pixels.DetachPixelData();

                        long[] totals = new long[] { 0, 0, 0 };

                        for (int i = 0; i < bytes.Length; i += 4)
                        {
                            totals[0] += bytes[i + 0];
                            totals[1] += bytes[i + 1];
                            totals[2] += bytes[i + 2];
                        }

                        int avgR = (int)(totals[0] / (bytes.Length / 4));
                        int avgG = (int)(totals[1] / (bytes.Length / 4));
                        int avgB = (int)(totals[2] / (bytes.Length / 4));

                        //ImageArt = bitmap;
                        AverageColorBrushImageArt = new SolidColorBrush(Color.FromArgb((byte)127, (byte)avgR, (byte)avgG, (byte)avgB));
                    }
                }
                //if (bitmap.UriSource == new Uri("ms-appx:///Assets/disc_gray.png", UriKind.Absolute))
                //{
                //    //ImageArt = bitmap;
                //    //AverageColorImageArt = (SolidColorBrush)PrismUnityApplication.Current.Resources["AppBackgroundColorBrush"];
                //    AverageColorBrushImageArt = new SolidColorBrush(Colors.Transparent);
                //}
                else
                {
                    var httpClient = new HttpClient();
                    var buffer = await httpClient.GetBufferAsync(bitmap.UriSource);

                    using (var stream = buffer.AsStream().AsRandomAccessStream())
                    {
                        //Create a decoder for the image
                        var decoder = await BitmapDecoder.CreateAsync(stream);

                        var pixels = await decoder.GetPixelDataAsync(
                                             BitmapPixelFormat.Rgba8,
                                             BitmapAlphaMode.Ignore,
                                             new BitmapTransform { ScaledHeight = 50, ScaledWidth = 50 },
                                             ExifOrientationMode.IgnoreExifOrientation,
                                             ColorManagementMode.DoNotColorManage);

                        //Get the bytes of the 1x1 scaled image
                        var bytes = pixels.DetachPixelData();

                        long[] totals = new long[] { 0, 0, 0 };

                        for (int i = 0; i < bytes.Length; i += 4)
                        {
                            totals[0] += bytes[i + 0];
                            totals[1] += bytes[i + 1];
                            totals[2] += bytes[i + 2];
                        }

                        int avgR = (int)(totals[0] / (bytes.Length / 4));
                        int avgG = (int)(totals[1] / (bytes.Length / 4));
                        int avgB = (int)(totals[2] / (bytes.Length / 4));

                        //ImageArt = bitmap;
                        AverageColorBrushImageArt = new SolidColorBrush(Color.FromArgb((byte)127, (byte)avgR, (byte)avgG, (byte)avgB));
                    }
                }
            }
            catch (Exception)
            {

            }        
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = sender as BitmapImage;
            bitmap.UriSource = new Uri("ms-appx:///Assets/disc_gray.png", UriKind.Absolute);
        }
    }
}
