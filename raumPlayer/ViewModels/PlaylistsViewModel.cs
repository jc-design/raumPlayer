using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using System.Linq;

namespace raumPlayer.ViewModels
{
    public class PlaylistsViewModel : BrowseViewModel
    {
        public PlaylistsViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance) : base(eventAggregatorInstance, messagingServiceInstance, shellViewModelInstance)
        {
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "MyPlaylists".GetLocalized()), new ParameterOverride("symbol", "\uE90B"), new ParameterOverride("startId", "0/Playlists/MyPlaylists"), new ParameterOverride("searchId", "")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Shuffles".GetLocalized()), new ParameterOverride("symbol", "\uE14B"), new ParameterOverride("startId", "0/Playlists/Shuffles"), new ParameterOverride("searchId", "")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Imported".GetLocalized()), new ParameterOverride("symbol", "\uE118"), new ParameterOverride("startId", "0/Playlists/Imported"), new ParameterOverride("searchId", "")
            }));

            InitializeCommand.Execute(null);
        }
    }
}
