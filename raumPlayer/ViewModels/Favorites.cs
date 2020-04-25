using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using System.Linq;

namespace raumPlayer.ViewModels
{
    public class FavoritesViewModel : BrowseViewModel
    {
        public FavoritesViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance) : base(eventAggregatorInstance, messagingServiceInstance, shellViewModelInstance)
        {
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "MyFavorites".GetLocalized()), new ParameterOverride("symbol", "\uEB51"), new ParameterOverride("startId", "0/Favorites/MyFavorites"), new ParameterOverride("searchId", "")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "RecentlyPlayed".GetLocalized()), new ParameterOverride("symbol", "\uE823"), new ParameterOverride("startId", "0/Favorites/RecentlyPlayed"), new ParameterOverride("searchId", "")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "MostPlayed".GetLocalized()), new ParameterOverride("symbol", "\uE8D6"), new ParameterOverride("startId", "0/Favorites/MostPlayed"), new ParameterOverride("searchId", "")
            }));

            InitializeCommand.Execute(null);
        }
    }
}
