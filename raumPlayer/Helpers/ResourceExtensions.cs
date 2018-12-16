using System;
using System.Runtime.InteropServices;

using Windows.ApplicationModel.Resources;

namespace raumPlayer.Helpers
{
    internal static class ResourceExtensions
    {
        private static ResourceLoader resourceLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            try
            {
                string retVal = resourceLoader.GetString(resourceKey);

                if (string.IsNullOrEmpty(retVal)) { return resourceKey; }
                else { return retVal; }
            }
            catch (Exception)
            {
                return resourceKey;
            }
            
        }
    }
}
