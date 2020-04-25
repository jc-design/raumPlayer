using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;
using raumPlayer.Services;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace raumPlayer.ViewModels
{
    public class PivotItemViewModel : ViewModelBase, IPivotItemViewModel
    {
        private void onCacheElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsGoBackInCacheEnabled = ((ICollection)sender).Count > 1;
        }

        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly IShellViewModel shellViewModel;
        private readonly IRaumFeldService raumFeldService;

        private readonly string startId;
        private readonly string searchId;

        public PivotItemViewModel ViewModel { get; }

        private string pivotSymbolAsString = string.Empty;
        public string PivotSymbol
        {
            get { return pivotSymbolAsString; }
            set { SetProperty(ref pivotSymbolAsString, value); }
        }

        private string pivotLabel = string.Empty;
        public string PivotLabel
        {
            get { return pivotLabel; }
            set { SetProperty(ref pivotLabel, value); }
        }

        private bool isScanning = false;
        public bool IsScanning
        {
            get { return isScanning; }
            set
            {
                SetProperty(ref isScanning, value, () =>
                {
                    IsScanningVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        private Visibility isScanningVisibility;
        public Visibility IsScanningVisibility
        {
            get { return isScanningVisibility; }
            set { SetProperty(ref isScanningVisibility, value); }
        }

        private Visibility listVisibility = Visibility.Collapsed;
        public Visibility ListVisibility
        {
            get { return listVisibility; }
            set { SetProperty(ref listVisibility, value); }
        }

        private bool isFiltering = false;

        private string filterCriteria;
        public string FilterCriteria
        {
            get { return filterCriteria; }
            set
            {
                SetProperty(ref filterCriteria, value);
            }
        }

        public List<ElementBase> RawElements { get; set; }

        private ObservableCollection<ElementBase> elements;
        public ObservableCollection<ElementBase> Elements
        {
            get { return elements; }
            set { SetProperty(ref elements, value); }
        }

        private ObservableCollection<ElementBase> filteredElements;
        public ObservableCollection<ElementBase> FilteredElements
        {
            get { return filteredElements; }
            set { SetProperty(ref filteredElements, value); }
        }

        private ObservableCollection<ElementBase> cacheElements;
        public ObservableCollection<ElementBase> CacheElements
        {
            get { return cacheElements; }
            set { SetProperty(ref cacheElements, value); }
        }

        private ElementBase lastCacheElement;
        public ElementBase LastCacheElement
        {
            get { return lastCacheElement; }
            set
            {
                SetProperty(ref lastCacheElement, value, () =>
                {
                    if (value == null) { return; }

                    for (int i = CacheElements.IndexOf(value) + 1; i < CacheElements.Count(); )
                    {
                        CacheElements.RemoveAt(i);
                    }
                    RaisePropertyChanged(nameof(IsGoBackInCacheEnabled));
                    if (RefreshElementsCommand is DelegateCommand command)
                    {
                        command.RaiseCanExecuteChanged();
                    }
                    if (RefreshElementsCommand.CanExecute(null)) { RefreshElementsCommand.Execute(null); }                   
                });
            }
        }

        private bool isGoBackInCacheEnabled;
        public bool IsGoBackInCacheEnabled
        {
            get { return isGoBackInCacheEnabled; }
            set { SetProperty(ref isGoBackInCacheEnabled, value); }
        }

        private bool isSearchEnabled;
        public bool IsSearchEnabled
        {
            get { return isSearchEnabled; }
            set { SetProperty(ref isSearchEnabled, value); }
        }

        public PivotItemViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance, IRaumFeldService raumFeldServiceInstance, string label, string symbol, string startId, string searchId)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            shellViewModel = shellViewModelInstance;
            raumFeldService = raumFeldServiceInstance;

            this.startId = startId;
            this.searchId = searchId;

            if (string.IsNullOrEmpty(searchId)) { IsSearchEnabled = false; }
            else { IsSearchEnabled = true; }

            PivotLabel = label;
            PivotSymbol = symbol;

            Elements = new ObservableCollection<ElementBase>();
            CacheElements = new ObservableCollection<ElementBase>();

            ViewModel = this;

            InitializeCommand.Execute(null);
        }

        private ICommand initializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (initializeCommand == null)
                {
                    initializeCommand = new DelegateCommand(async () =>
                    {
                        var startData = await raumFeldService.BrowseMetaData(startId);

                        if (startData != null)
                        {
                            LastCacheElement = startData;
                            CacheElements.Add(startData);
                            CacheElements.CollectionChanged += onCacheElementsCollectionChanged;
                        }
                    });
                }

                return initializeCommand;
            }
        }

        /// <summary>
        /// Reload last element from CacheElements
        /// </summary>
        private ICommand refreshElementsCommand;
        public ICommand RefreshElementsCommand
        {
            get
            {
                if (refreshElementsCommand == null)
                {
                    refreshElementsCommand = new DelegateCommand(async () =>
                    {
                        if ((CacheElements?.Count() ?? 0) == 0) { return; }

                        IsScanning = true;

                        await raumFeldService.BrowseChildren(this, CacheElements.Last().Id);

                        IsScanning = false;
                    }, () => (CacheElements?.Count() ?? 0) > 0);
                }

                return refreshElementsCommand;
            }
        }

        public void GoBackInCache()
        {
            if ((CacheElements?.Count() ?? 0) > 1)
            {
                CacheElements.Remove(CacheElements.Last());
                LastCacheElement = CacheElements.Last();
            }
        }

        /// <summary>
        /// Filter Elements - in memory
        /// </summary>
        private ICommand querySubmittedFilterCommand;
        public ICommand QuerySubmittedFilterCommand
        {
            get
            {
                if (querySubmittedFilterCommand == null)
                {
                    querySubmittedFilterCommand = new DelegateCommand<AutoSuggestBox>((param) =>
                    {
                        if (string.IsNullOrEmpty(param.Text)) { isFiltering = false; }
                        else
                        {
                            isFiltering = true;

                            Elements.Clear();

                            IEnumerable<ElementBase> filteredList = RawElements.Where(e => (e.Title + (e?.Genre ?? "") + (e?.Album ?? "") + (e?.Artist ?? "")).ToUpper().Contains(param.Text.ToUpper())).ToList();

                            foreach (var item in filteredList)
                            {
                                Elements.Add(item);
                            }
                        }
                    });
                }

                return querySubmittedFilterCommand;
            }
        }

        /// <summary>
        /// Do search on Raumfeld device
        /// </summary>
        private ICommand querySubmittedSearchCommand;
        public ICommand QuerySubmittedSearchCommand
        {
            get
            {
                if (querySubmittedSearchCommand == null)
                {
                    querySubmittedSearchCommand = new DelegateCommand<AutoSuggestBox>(async (param) =>
                    {
                        if (string.IsNullOrEmpty(searchId)) { return; }
                        if (string.IsNullOrEmpty(param.Text)) { RefreshElementsCommand.Execute(null); }
                        else
                        {
                            IsScanning = true;

                            await raumFeldService.Search(Elements, searchId, param.Text);

                            IsScanning = false;
                        }
                    });
                }

                return querySubmittedSearchCommand;
            }
        }

        /// <summary>
        /// Filter Elements - in memory
        /// </summary>
        private ICommand itemTappedCommand;
        public ICommand ItemTappedCommand
        {
            get
            {
                if (itemTappedCommand == null)
                {
                    itemTappedCommand = new DelegateCommand<ElementBase>((param) =>
                    {
                        if (param.IsPlayable)
                        {
                            if (NewQueueTappedCommand != null)
                            {
                                if (NewQueueTappedCommand.CanExecute(param))
                                {
                                    NewQueueTappedCommand.Execute(param);
                                }
                            }
                            return;
                        }
                        if (param.IsFolder)
                        {
                            if (BrowseTappedCommand != null)
                            {
                                if (BrowseTappedCommand.CanExecute(param))
                                {
                                    BrowseTappedCommand.Execute(param);
                                }
                            }
                        }
                    });
                }

                return itemTappedCommand;
            }
        }

        /// <summary>
        /// Browse folder
        /// </summary>
        private ICommand browseTappedCommand;
        public ICommand BrowseTappedCommand
        {
            get
            {
                if (browseTappedCommand == null)
                {
                    browseTappedCommand = new DelegateCommand<ElementBase>((param) =>
                    {
                        CacheElements.Add(param);
                        LastCacheElement = param;
                    });
                }

                return browseTappedCommand;
            }
        }

        /// <summary>
        /// Start new queue
        /// </summary>
        private ICommand newQueueTappedCommand;
        public ICommand NewQueueTappedCommand
        {
            get
            {
                if (newQueueTappedCommand == null)
                {
                    newQueueTappedCommand = new DelegateCommand<ElementBase>((param) =>
                    {
                        if (shellViewModel.ActiveZoneViewModel == null) { return; }

                        if (shellViewModel.ActiveZoneViewModel.SetAVTransportUriCommand != null)
                        {
                            if (shellViewModel.ActiveZoneViewModel.SetAVTransportUriCommand.CanExecute(param))
                            {
                                shellViewModel.ActiveZoneViewModel.SetAVTransportUriCommand.Execute(param);
                            }
                        }
                    });
                }

                return newQueueTappedCommand;
            }
        }

        /// <summary>
        /// Event when GridviewItem is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void FolderItemClicked(object sender, EventArgs<ElementBase> e)
        {
            //IsScanning = true;

            //CacheElements.Add(new PathElement() { Title = e.Value.Title, ParentId = e.Value.ParentID, Id = e.Value.Id, AlbumArtUri = e.Value.AlbumArtUri });
            //LastElement = CacheElements.Last();

            //ServiceActionReturnMessage messageContainer = await browseFolders(e.Value.Id, await createNewToken());
            //if (messageContainer.ActionStatus == ActionStatus.Error)
            //{
            //    await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageContainer.ActionErrorCode, messageContainer.ActionMessage), "FolderItemClicked");
            //}

            //IsScanning = false;
        }

        /// <summary>
        /// Event when New button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnNewQueueClicked(object sender, EventArgs<ElementBase> e)
        {
            //// Need to check type of Element
            //// SetAVTransportUri receives different parameters

            //if (ActiveRenderer == null) { return; }

            //Element element = await browseMetaData(e.Value.Id);
            //if (element != null && element.IsPlayable)
            //{
            //    DoEmptyPlayList?.Invoke(this, null);

            //    if (element.IsFolder)
            //    {
            //        ServiceActionReturnMessage messageContainer = await ActiveRenderer.SetAVTransportUri(true, mediaServer.UDN, element.Id, null, 0, element.BrowsedMetaData);
            //        if (messageContainer.ActionStatus == ActionStatus.Error)
            //        {
            //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageContainer.ActionErrorCode, messageContainer.ActionMessage), UIService.GetResource("Error"));
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        ServiceActionReturnMessage messageItem = await ActiveRenderer.SetAVTransportUri(false, mediaServer.UDN, null, element.Id, -1, element.BrowsedMetaData);
            //        if (messageItem.ActionStatus == ActionStatus.Error)
            //        {
            //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageItem.ActionErrorCode, messageItem.ActionMessage), UIService.GetResource("Error"));
            //            return;
            //        }
            //    }

            //    //switch (element.Class)
            //    //{
            //    //    //Object_container_trackContainer,
            //    //    //Object_container_trackContainer_napster,
            //    //    //Object_item_audioItem_audioBroadcast_lastFM,
            //    //    //Object_item_audioItem_audioBroadcast_rhapsody,
            //    //    case "object.container":
            //    //    case "object.container.albumContainer":
            //    //    case "object.container.favoritesContainer":
            //    //    case "object.container.genre.musicGenre":
            //    //    case "object.container.person.musicArtist":
            //    //    case "object.container.person.musicComposer":
            //    //        break;
            //    //    case "object.container.album.musicAlbum":
            //    //    case "object.container.playlistContainer":
            //    //    case "object.container.playlistContainer.shuffle":
            //    //    case "object.container.storageFolder":
            //    //    case "object.container.trackContainer.allTracks":
            //    //    case "object.container.album.musicAlbum.compilation":
            //    //        ServiceActionReturnMessage messageContainer = await ActiveRenderer.SetAVTransportUri(true, mediaServer.UDN, element.Id, null, 0, element.BrowsedMetaData);
            //    //        if (messageContainer.ActionStatus == ActionStatus.Error)
            //    //        {
            //    //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageContainer.ActionErrorCode, messageContainer.ActionMessage), UIService.GetResource("Error"));
            //    //        }
            //    //        break;
            //    //    case "object.item.audioItem.audioBroadcast.radio":
            //    //        ServiceActionReturnMessage messageRadio = await ActiveRenderer.SetAVTransportUri(false, mediaServer.UDN, null, element.Id, -1, element.BrowsedMetaData);
            //    //        if (messageRadio.ActionStatus == ActionStatus.Error)
            //    //        {
            //    //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageRadio.ActionErrorCode, messageRadio.ActionMessage), UIService.GetResource("Error"));
            //    //        }
            //    //        break;
            //    //    case "object.item.audioItem.musicTrack":

            //    //        Element parentelement = await browseMetaData(e.Value.ParentID);
            //    //        if (parentelement != null)
            //    //        {
            //    //            //ServiceActionReturnMessage messageTrack = await ActiveRenderer.SetAVTransportUri(true, mediaServer.UDN, element.ParentID, element.Id, 0, parentelement.BrowsedMetaData);
            //    //            ServiceActionReturnMessage messageTrack = await ActiveRenderer.SetAVTransportUri(false, mediaServer.UDN, null, element.Id, -1, element.BrowsedMetaData);
            //    //            if (messageTrack.ActionStatus == ActionStatus.Error)
            //    //            {
            //    //                await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageTrack.ActionErrorCode, messageTrack.ActionMessage), UIService.GetResource("Error"));
            //    //            }
            //    //        }
            //    //        break;
            //    //    case "object.item.audioItem.audioBroadcast.lineIn":
            //    //        //Play
            //    //        break;
            //    //    default:
            //    //        break;
            //    //}
            //}
        }

        /// <summary>
        /// Event when Add button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnAddQueueClicked(object sender, EventArgs<ElementBase> e)
        {
            //// First check if playlist already exists
            //int position = 0;
            //Element playlist = await browseMetaData(RaumFeldDefinitions.PLAYLISTBASE + "/" + RaumFeldDefinitions.PLAYLIST);

            //if (playlist != null)
            //{
            //    position = playlist.ChildCount;
            //}
            //else
            //{
            //    ServiceActionReturnMessage message = await mediaServer.CreateQueue();
            //    if (message.ActionStatus == ActionStatus.Error)
            //    {
            //        await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnAddQueueClicked");
            //    }
            //}

            //Element addElement = await browseMetaData(e.Value.Id);
            //if (addElement != null && addElement.IsPlayable)
            //{
            //    if (addElement.IsFolder)
            //    {
            //        ServiceActionReturnMessage message = await mediaServer.AddContainerToQueue(addElement.Id, position);
            //        if (message.ActionStatus == ActionStatus.Error)
            //        {
            //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnAddQueueClicked");
            //        }
            //    }
            //    else
            //    {
            //        ServiceActionReturnMessage message = await mediaServer.AddItemToQueue(addElement.Id, position);
            //        if (message.ActionStatus == ActionStatus.Error)
            //        {
            //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnAddQueueClicked");
            //        }
            //    }
            //}
            //else
            //{
            //    await UIService.ShowDialogAsync(string.Format("{0} : {1}", UIService.GetResource("ErrorNoData"), e.Value.Id), "OnAddQueueClicked");
            //}
        }

        public async void OnRemQueueClicked(object sender, EventArgs<ElementBase> e)
        {
            //    ElementGroup group = GroupedElements.Where(g => g.Elements.Contains(e.Value)).FirstOrDefault();

            //    if (group != null)
            //    {
            //        group.Elements.Remove(e.Value);
            //        if (group.Elements.Count() == 0)
            //        {
            //            GroupedElements.Remove(group);
            //        }
            //    }

            //    Elements.Remove(e.Value);
            //    ServiceActionReturnMessage message = await mediaServer.DestroyObject(e.Value.Id);
            //    if (message.ActionStatus == ActionStatus.Error)
            //    {
            //        await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnFavQueueClicked");
            //    }
        }

        /// <summary>
        /// Event when Fav button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnFavQueueClicked(object sender, EventArgs<ElementBase> e)
        {
            //ServiceActionReturnMessage message = await mediaServer.CreateReference(e.Value.Id);
            //if (message.ActionStatus == ActionStatus.Error)
            //{
            //    await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnFavQueueClicked");
            //}
        }

        /// <summary>
        /// Event when FavRem button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnFavRemQueueClicked(object sender, EventArgs<ElementBase> e)
        {
            //ElementGroup group = GroupedElements.Where(g => g.Elements.Contains(e.Value)).FirstOrDefault();

            //if (group != null)
            //{
            //    group.Elements.Remove(e.Value);
            //    if (group.Elements.Count() == 0)
            //    {
            //        GroupedElements.Remove(group);
            //    }
            //}

            //Elements.Remove(e.Value);
            //ServiceActionReturnMessage message = await mediaServer.DestroyObject(e.Value.Id);
            //if (message.ActionStatus == ActionStatus.Error)
            //{
            //    await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "OnFavQueueClicked");
            //}
        }

        #region Private Methods

        ///// <summary>
        ///// Browse folder
        ///// </summary>
        ///// <param name="elementId"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //private async Task<ServiceActionReturnMessage> browseFolders(string elementId, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        int start = 0;
        //        int limit = 10;

        //        bool continueLoop = true;

        //        //SearchedServerContentElements = new ObservableCollection<Element>();
        //        Elements = new ObservableCollection<Element>();
        //        GroupedElements = new ObservableCollection<ElementGroup>();

        //        ServiceActionReturnMessage message;

        //        do
        //        {
        //            message = await mediaServer.Browse(elementId, "BrowseDirectChildren", start, limit);

        //            if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is List<Element> partList)
        //            {
        //                if ((partList?.Count() ?? 0) > 0)
        //                {
        //                    cancellationToken.ThrowIfCancellationRequested();

        //                    foreach (var item in partList)
        //                    {
        //                        Elements.Add(item);

        //                        ElementGroup group = null;
        //                        string groupKey = string.Empty;

        //                        if (item.IsFolder) { groupKey = item.Title.GetGroupKey(); }
        //                        else { groupKey = "#"; }

        //                        group = GroupedElements.Where(g => g.Key == groupKey).FirstOrDefault();
        //                        if (group != null) { group.Elements.Add(item); }
        //                        else { GroupedElements.Add(new ElementGroup(groupKey, new ObservableCollection<Element>() { item })); }
        //                    }

        //                    if (start == 0)
        //                    {
        //                        IsListview = false;
        //                        //if (AutoSuggestBoxSearch != null) { AutoSuggestBoxSearch.Text = String.Empty; }
        //                    }

        //                    start = start + limit;
        //                }
        //                else { continueLoop = false; }
        //            }
        //            else
        //            {
        //                Elements = new ObservableCollection<Element>();
        //                GroupedElements = new ObservableCollection<ElementGroup>();
        //                continueLoop = false;
        //            }
        //        } while (continueLoop);

        //        FillSemanticZoom filler = new FillSemanticZoom();
        //        GroupedElements = new ObservableCollection<ElementGroup>(GroupedElements.Union(filler.Filler.Where(f => GroupedElements.All(e => e.Key != f.Key))).OrderBy(f => f.Key).ToList());

        //        return message;
        //    }
        //    catch (OperationCanceledException canceledException)
        //    {
        //        return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Okay, ActionMessage = canceledException.Message };
        //    }
        //    catch (Exception exception)
        //    {
        //        Logger.GetLogger().SaveMessage(exception, "BrowseFolders");
        //        return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
        //    }
        //}

        ///// <summary>
        ///// Browse ObjectId to get MetaData
        ///// </summary>
        ///// <param name="element">Selected Element</param>
        ///// <returns></returns>
        //private async Task<Element> browseMetaData(string elementId)
        //{
        //    ServiceActionReturnMessage message = await mediaServer.Browse(elementId, "BrowseMetadata", 0, 1);
        //    if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is List<Element> partList)
        //    {
        //        return partList.FirstOrDefault();
        //    }
        //    else
        //    {
        //        await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "BrowseMetdaData");
        //        return null;
        //    }
        //}

        #endregion
    }
}
