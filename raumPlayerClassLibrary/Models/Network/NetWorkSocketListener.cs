using Prism.Events;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.PrismEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace raumPlayer.Models
{
    public class NetWorkSocketListener : INetWorkSocketListener
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;

        private readonly HostName localHostName;
        private StreamSocketListener streamSocketListener;
        private int port;

        public string Hostname
        {
            get
            {
                if (localHostName != null && port != 0) { return $"http://{localHostName.CanonicalName}:{port}"; }
                else { return string.Empty; }
            }
        }

        public NetWorkSocketListener(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;

            localHostName = findHostNameIpv4();
        }

        /// <summary>
        /// Starts listening to Port
        /// First initialize new instance of StreamSocketListener and hook up event
        /// </summary>
        /// <returns></returns>
        public async Task StartListening(int port)
        {
            this.port = port;

            if (streamSocketListener != null)
            {
                streamSocketListener.ConnectionReceived -= onConnectionReceived;
                streamSocketListener.Dispose();
            }

            streamSocketListener = new StreamSocketListener(); 
            streamSocketListener.ConnectionReceived += onConnectionReceived;

            // If necessary, tweak the listener's control options before carrying out the bind operation.
            // These options will be automatically applied to the connected StreamSockets resulting from
            // incoming connections (i.e., those passed as arguments to the ConnectionReceived event handler).
            // Refer to the StreamSocketListenerControl class' MSDN documentation for the full list of control options.
            //socketListener.Control.KeepAlive = false;
            //socketListener.Control.QualityOfService = SocketQualityOfService.LowLatency;
            //socketListener.Control.NoDelay = false;
            try
            {
                await streamSocketListener.BindEndpointAsync(null, port.ToString());
            }
            catch (Exception exception)
            {
                throw new Exception();
                await messagingService.ShowErrorDialogAsync(exception);
            }
        }

        /// <summary>
        /// Stops listening to Port
        /// </summary>
        public void StopListening()
        {
            if (streamSocketListener != null)
            {
                streamSocketListener.ConnectionReceived -= onConnectionReceived;
                streamSocketListener.Dispose();
                streamSocketListener = null;
            }
        }

        private async void onConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            //await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            //{
                try
                {
                    Dictionary<string, string> resultHeaders = new Dictionary<string, string>();

                    // Add "\r\n" to remove EndOfLine
                    string[] stringSeparators = new string[] { ": ", "\r\n" };
                    string[] linesplit;

                    using (DataReader dataReader = new DataReader(args.Socket.InputStream))
                    {
                        string line = string.Empty;
                        byte[] readByte = new byte[1];

                        // Read the HTTP-Headers
                        do
                        {
                            uint l = await dataReader.LoadAsync(1);
                            if (l == 0) { break; }
                            dataReader.ReadBytes(readByte);
                            line += Encoding.ASCII.GetString(readByte);

                            if (line.Length > 2 && line.Substring(line.Length - 2) == "\r\n")
                            {
                                linesplit = line.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                                line = string.Empty;
                                if ((linesplit?.Count() ?? 0) == 2) { resultHeaders[linesplit[0]] = linesplit[1]; }
                            }
                            // Blankline == Headers finished; Content starts
                            else if (line.Length == 2 && line == "\r\n") { break; }
                        } while (true);

                        if (resultHeaders.TryGetValue("Content-Length", out string value))
                        {
                            if (UInt32.TryParse(value, out uint length))
                            {
                                uint resultLength = 0;
                                string data = string.Empty;

                                resultLength = await dataReader.LoadAsync(length);
                                if (resultLength != 0)
                                {
                                    byte[] byteBuffer = new byte[resultLength];
                                    dataReader.ReadBytes(byteBuffer);
                                    data += Encoding.UTF8.GetString(byteBuffer);

                                    RaumFeldEventPropertySet propset = data.ToString().Deserialize<RaumFeldEventPropertySet>();

                                    if (propset != null)
                                    {
                                        propset.EventSID = resultHeaders["SID"];
                                        eventAggregator.GetEvent<RaumFeldEventPropertySetReceivedEvent>().Publish(propset);
                                    }
                                }
                            }
                        }

                        dataReader.DetachStream();
                    }

                    using (DataWriter dataWriter = new DataWriter(args.Socket.OutputStream))
                    {
                        dataWriter.WriteBytes(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n"));
                        await dataWriter.FlushAsync();
                        dataWriter.DetachStream();
                    }

                    args.Socket.InputStream.Dispose();
                    args.Socket.OutputStream.Dispose();
                    args.Socket.Dispose();
                }
                catch (Exception exception)
                {
                    var errorStatus = SocketError.GetStatus(exception.HResult);

                    throw;
                }
            //});
        }

        private HostName findHostNameIpv4()
        {
            ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (internetConnectionProfile != null && internetConnectionProfile.NetworkAdapter != null)
            {
                HostName findLocalHostname = NetworkInformation.GetHostNames().SingleOrDefault(hn => hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                                                                                                     && hn.IPInformation.NetworkAdapter.NetworkAdapterId == internetConnectionProfile.NetworkAdapter.NetworkAdapterId
                                                                                                     && hn.Type == HostNameType.Ipv4);

                return findLocalHostname ?? default(HostName);
            }

            // Return Null or Default if no HostName could be found
            return default(HostName);
        }
    }
}
