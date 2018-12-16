using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.Interfaces
{
    public interface INetWorkSocketListener
    {
        string Hostname { get; }

        Task StartListening(int port);
        void StopListening();
    }
}
