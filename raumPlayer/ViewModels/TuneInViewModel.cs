using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using System.Linq;

namespace raumPlayer.ViewModels
{
    public class TuneInViewModel : BrowseViewModel
    {
        public TuneInViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance) : base(eventAggregatorInstance, messagingServiceInstance, shellViewModelInstance)
        {
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "Favorites".GetLocalized()), new ParameterOverride("symbol", "\uEB51"), new ParameterOverride("startId", "0/RadioTime/Favorites"), new ParameterOverride("searchId", "0/RadioTime/Search")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "RadioLocal".GetLocalized()), new ParameterOverride("symbol", "\uE80F"), new ParameterOverride("startId", "0/RadioTime/LocalRadio"), new ParameterOverride("searchId", "0/RadioTime/Search")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "RadioMusic".GetLocalized()), new ParameterOverride("symbol", "\uE189"), new ParameterOverride("startId", "0/RadioTime/CategoryMusic"), new ParameterOverride("searchId", "0/RadioTime/Search")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "RadioTalk".GetLocalized()), new ParameterOverride("symbol", "\uE125"), new ParameterOverride("startId", "0/RadioTime/CategoryTalk"), new ParameterOverride("searchId", "0/RadioTime/Search")
            }));
            PivotItems.Add(PrismUnityApplication.Current.Container.Resolve<PivotItemViewModel>(new ResolverOverride[]
            {
                new ParameterOverride("label", "RadioSport".GetLocalized()), new ParameterOverride("symbol", "\uE805"), new ParameterOverride("startId", "0/RadioTime/CategorySports"), new ParameterOverride("searchId", "0/RadioTime/Search")
            }));

            InitializeCommand.Execute(null);
        }
    }
}
