using Microsoft.Data.Sqlite;
using raumPlayer.Interfaces;
using raumPlayer.Helpers;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using Upnp;
using Prism.Unity.Windows;
using Microsoft.Practices.Unity;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using System.Text.RegularExpressions;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.UI;
using raumPlayer.Models;
using Windows.UI.Core;
using Windows.Storage.FileProperties;

namespace raumPlayer.Services
{
    public class CachingService : ICachingService
    {
        private const string FILENAME_CACHE_DB = "raumPlayer_Cache.db";
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;

        public CachingService(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "CREATE TABLE IF NOT EXISTS CACHE000 (ID NVARCHAR(1024) NOT NULL, FILENAME NVARCHAR(1024) NULL, AVERAGECOLOR NVARCHAR(32) NULL, DAT_USED DATETIME NULL, SIZE_MB REAL, PRIMARY KEY('ID'));"
                    };

                    await command.ExecuteReaderAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<CacheData> GetElementAsync(string iD)
        {
            CacheData cachedElement = new CacheData();

            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "SELECT ID, FILENAME, AVERAGECOLOR from CACHE000 where ID=@ID;"
                    };
                    command.Parameters.AddWithValue("@ID", iD);

                    SqliteDataReader query = await command.ExecuteReaderAsync();

                    if (!query.HasRows)
                    {
                        return null;
                    }

                    while (await query.ReadAsync())
                    {
                        cachedElement.ID = await query.GetFieldValueAsync<string>(0);
                        cachedElement.FileName = await query.GetFieldValueAsync<string>(1);
                        cachedElement.Color = await query.GetFieldValueAsync<string>(2);
                    }

                    command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "UPDATE CACHE000 set DAT_USED = Datetime('Now') where ID=@ID;"
                    };
                    command.Parameters.AddWithValue("@ID", iD);

                    query = await command.ExecuteReaderAsync();

                    return cachedElement;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<CacheData> AddElementAsync(ElementBase element)
        {
            string fileName = $"{element.AlbumArtUri.ComputeMD5()}.png";
            string color = "#7F7F7F7F";
            float size = 0;

            CacheData cachedElement = new CacheData() { ID = element.Id };

            //Save each element into database
            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        // Use parameterized query to prevent SQL injection attacks
                        CommandText = "INSERT OR REPLACE INTO CACHE000(ID, FILENAME, AVERAGECOLOR, DAT_USED, SIZE_MB) VALUES (@ID, @FileName, @Color, @Date, @Size);"
                    };
                    command.Parameters.AddWithValue("@ID", element.Id);
                    command.Parameters.AddWithValue("@FileName", string.Empty);
                    command.Parameters.AddWithValue("@Color", color);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.Parameters.AddWithValue("@Size", 0);

                    await command.ExecuteReaderAsync();

                    db.Close();
                }
            }
            catch (Exception)
            {
                return cachedElement;
            }

            if (element.AlbumArtUri == "ms-appx:///Assets/disc_gray.png")
            {
                return cachedElement;
            }

            //Download Image from UPNP-Device
            try
            {
                StorageFolder cacheFolder = null;
                StorageFile cachedFile = null;
                cacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("cachedImages", CreationCollisionOption.OpenIfExists);

                var httpClient = new HttpClient();
                var buffer = await httpClient.GetBufferAsync(new Uri(element.AlbumArtUri));

                var existingItem = await cacheFolder.TryGetItemAsync(fileName);
                if (existingItem == null)
                {
                    cachedFile = await cacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBufferAsync(cachedFile, buffer);
                    BasicProperties basicProperties = await cachedFile.GetBasicPropertiesAsync();
                    size = basicProperties.Size / 1024f / 1024f;
                }
                else
                {
                    BasicProperties basicProperties = await existingItem.GetBasicPropertiesAsync();
                    size = basicProperties.Size / 1024f / 1024f;
                }

                //Get reference of cachedFile
                cachedFile = await cacheFolder.GetFileAsync(fileName);
                using (var stream = (await Windows.Storage.FileIO.ReadBufferAsync(cachedFile)).AsStream().AsRandomAccessStream())
                {
                    //Create a decoder for the image
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    

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

                    color = Color.FromArgb((byte)127, (byte)avgR, (byte)avgG, (byte)avgB).ToString();
                }
            }
            catch (Exception)
            {
                return cachedElement;
            }

            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        // Use parameterized query to prevent SQL injection attacks
                        CommandText = "INSERT OR REPLACE INTO CACHE000(ID, FILENAME, AVERAGECOLOR, DAT_USED, SIZE_MB) VALUES (@ID, @FileName, @Color, @Date, @Size);"
                    };
                    command.Parameters.AddWithValue("@ID", element.Id);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Color", color);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.Parameters.AddWithValue("@Size", size);

                    await command.ExecuteReaderAsync();

                    db.Close();
                }
            }
            catch (Exception)
            {
                return cachedElement;
            }

            cachedElement.FileName = fileName;
            cachedElement.Color = color;
            return cachedElement;
        }

        public async Task<bool> ClearCacheAsync()
        {
            try
            {
                StorageFolder cacheFolder = null;
                cacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("cachedImages", CreationCollisionOption.OpenIfExists);
                await cacheFolder.DeleteAsync();

                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "DELETE FROM CACHE000;"
                    };

                    await command.ExecuteReaderAsync();

                    db.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<float> GetCountOfCachedFilesAsync()
        {
            float returnValue = 0;
            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "SELECT COUNT(FILENAME) as COUNT from CACHE000 where SIZE_MB > 0;"
                    };

                    SqliteDataReader query = await command.ExecuteReaderAsync();

                    if (!query.HasRows)
                    {
                        return 0;
                    }

                    while (await query.ReadAsync())
                    {
                        returnValue = await query.GetFieldValueAsync<float>(0);
                    }
                    return returnValue;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<float> GetSizeOfCachedFilesAsync()
        {
            float returnValue = 0;
            try
            {
                using (SqliteConnection db = new SqliteConnection(string.Format("Filename={0}", Path.Combine(ApplicationData.Current.LocalFolder.Path, FILENAME_CACHE_DB))))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "SELECT SUM(SIZE_MB) as SIZE from CACHE000;"
                    };

                    SqliteDataReader query = await command.ExecuteReaderAsync();

                    if (!query.HasRows)
                    {
                        return 0;
                    }

                    while (await query.ReadAsync())
                    {
                        returnValue = await query.GetFieldValueAsync<float>(0);
                    }
                    return returnValue;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
