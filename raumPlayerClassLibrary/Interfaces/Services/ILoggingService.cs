using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.Interfaces
{
    public interface ILoggingService
    {
        Task Initialize();
        Task Log(Exception Exception);
        Task Log(string Message);
    }
}
