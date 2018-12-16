using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using System.Linq;

namespace raumPlayer.ViewModels
{
    public class MyMusicViewModel : BrowseViewModel
    {
        public MyMusicViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance) : base(eventAggregatorInstance, messagingServiceInstance, shellViewModelInstance)
        {
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Artist".GetLocalized()), new ParameterOverride("symbol", "\uED53"), new ParameterOverride("startId", "0/My Music/Artists"), new ParameterOverride("searchId", "0/My Music/Search/TrackArtists")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Album".GetLocalized()), new ParameterOverride("symbol", "\uE958"), new ParameterOverride("startId", "0/My Music/Albums"), new ParameterOverride("searchId", "0/My Music/Search/Albums")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Genre".GetLocalized()), new ParameterOverride("symbol", "\uE74C"), new ParameterOverride("startId", "0/My Music/Genres"), new ParameterOverride("searchId", "")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Composer".GetLocalized()), new ParameterOverride("symbol", "\uE13D"), new ParameterOverride("startId", "0/My Music/Composers"), new ParameterOverride("searchId", "0/My Music/Search/Composers")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Folder".GetLocalized()), new ParameterOverride("symbol", "\uED43"), new ParameterOverride("startId", "0/My Music/ByFolder"), new ParameterOverride("searchId", "")
            }));

            InitializeCommand.Execute(null);
        }
    }
}
