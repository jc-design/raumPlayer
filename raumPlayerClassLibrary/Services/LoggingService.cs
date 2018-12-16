using Prism.Unity.Windows;
using raumPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace raumPlayer.Services
{
    public class LoggingService : ILoggingService
    {
        private const string LOG_CHANNEL_RESROUCE_NAME = "LogChannel";
        private const string LOG_SESSION_RESROUCE_NAME = "LogSession";
        private const string LOG_SFOLDER_RESROUCE_NAME = "LogFilesFolder";
        private const int DAYS_TO_DELETE = 15;

        private readonly LoggingChannel logChannel;
        private readonly LoggingSession logSession;
        private string fileName => DateTime.Now.ToString("yyyyMMdd") + ".etl";

        private readonly SemaphoreSlim semaphoreLock;
        private static StorageFolder logFolder;

        private static bool isLogged = false;
        public static bool IsLooged
        {
            get { return isLogged; }
        }

        public LoggingService()
        {
            logChannel = new LoggingChannel(LOG_CHANNEL_RESROUCE_NAME, new LoggingChannelOptions());
            logSession = new LoggingSession(LOG_SESSION_RESROUCE_NAME);
            logSession.AddLoggingChannel(logChannel);

            semaphoreLock = new SemaphoreSlim(1);

            CoreApplication.UnhandledErrorDetected += OnUnhandledErrorDetected;
            PrismUnityApplication.Current.UnhandledException += OnUnhandledException;

            isLogged = false;
        }

        private async void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await Log(e.Exception);
            PrismUnityApplication.Current.Exit();
        }

        private async void OnUnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            try
            {
                if (sender != null) { logChannel.LogMessage(sender.ToString()); }
                e.UnhandledError.Propagate();
            }
            catch (Exception exception)
            {
                await Log(exception);
            }
        }

        public async Task Initialize()
        {
            logFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LOG_SFOLDER_RESROUCE_NAME, CreationCollisionOption.OpenIfExists);
            try
            {
                var logFiles = await logFolder.GetFilesAsync();

                foreach (var logFile in logFiles)
                {
                    if ((DateTime.Now - logFile.DateCreated).Days > DAYS_TO_DELETE)
                    {
                        await logFile.DeleteAsync();
                    }
                }
            }
            catch { }
        }

        public async Task Log(Exception exception)
        {
            try
            {
                if (logSession != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(string.Format("Error Message: {0}", exception.Message));
                    sb.AppendLine(string.Format("UnhandledError: 0x{0:X})", exception.HResult));
                    sb.AppendLine(string.Format("Stacktrace: {0}", exception?.StackTrace ?? string.Empty));

                    logChannel.LogMessage(sb.ToString(), LoggingLevel.Critical);

                    await semaphoreLock.WaitAsync();
                    try
                    {
                        await logSession.SaveToFileAsync(logFolder, fileName);
                        isLogged = true;
                    }
                    finally
                    {
                        semaphoreLock.Release();
                    }
                }
            }
            catch { }
        }

        public async Task Log(string message)
        {
            try
            {
                if (logSession != null)
                {
                    logChannel.LogMessage(message, LoggingLevel.Information);

                    await semaphoreLock.WaitAsync();
                    try
                    {
                        await logSession.SaveToFileAsync(logFolder, fileName);
                        isLogged = true;
                    }
                    finally
                    {
                        semaphoreLock.Release();
                    }
                }
            }
            catch { }
        }
    }
}
