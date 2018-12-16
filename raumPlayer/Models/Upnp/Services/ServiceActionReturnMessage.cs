using Upnp;

namespace Upnp
{
    public class ServiceActionReturnMessage
    {
        public ActionStatus ActionStatus { get; set; }
        public int ActionErrorCode { get; set; }
        public string ActionMessage { get; set; }
        public object ReturnValue { get; set; }
    }
}
