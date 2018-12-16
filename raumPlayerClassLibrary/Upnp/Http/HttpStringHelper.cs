
namespace Upnp.Helper
{
    static class HttpStringHelper
    {
        public static string StringWithStartSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.Substring(0, 1) == "/") { return url; }
            else { return "/" + url; }
        }

        public static string StringWithEndSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if (url.Substring(url.Length - 1, 1) != "/")
                return "/";

            return "";
        }

        public static string StringWithoutEndSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.Substring(url.Length - 1, 1) != "/") { return url; }
            else { return url.Substring(0,url.Length - 1); }
        }

        public static string CompleteHttpString(string ip, int port, string extention)
        {
            string uri;
            if (ip.Substring(0,7) != "http://") { ip = "http://" + ip; }
            uri = StringWithoutEndSlash(ip) + ":" + port + StringWithStartSlash(extention);
            return uri;
        }
    }
}
