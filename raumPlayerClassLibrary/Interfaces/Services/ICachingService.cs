using raumPlayer.Models;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upnp;

namespace raumPlayer.Interfaces
{
    public interface ICachingService
    {
        Task<bool> InitializeAsync();
        Task<CacheData> AddElementAsync(ElementBase element);
        Task<CacheData> GetElementAsync(string FileName);
        Task<bool> ClearCacheAsync();

        Task<float> GetCountOfCachedFilesAsync();
        Task<float> GetSizeOfCachedFilesAsync();

    }
}
