using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.PrismEvents;
using System.Windows.Input;
using Upnp;
using Windows.UI.Xaml.Controls;

namespace raumPlayer.ViewModels
{
    public class RoomViewModel : ViewModelBase, IRoomViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;

        private readonly ZoneViewModel zoneViewModel;
        private readonly MediaRenderer roomRenderer;

        public string Udn { get; }
        public string Name { get; }

        private string powerState = string.Empty;
        public string PowerState
        {
            get { return powerState; }
            set
            {
                SetProperty(ref powerState, value, () =>
                {
                    switch (value)
                    {
                        case "ACTIVE":
                            IsActive = true;
                            break;
                        case "AUTOMATIC_STANDBY":
                        case "MANUAL_STANDBY":
                            IsActive = false;
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        private double roomVolume;
        public double RoomVolume
        {
            get { return roomVolume; }
            set { SetProperty(ref roomVolume, value); }
        }

        private bool? roomMute;
        public bool? RoomMute
        {
            get { return roomMute; }
            set { SetProperty(ref roomMute, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                SetProperty(ref isActive, value, () =>
                {
                    SetRoomStateCommand.Execute(value);
                });
            }
        }

        private ICommand getRoomVolumeCommand;
        public ICommand GetRoomVolumeCommand
        {
            get
            {
                if (getRoomVolumeCommand == null)
                {
                    getRoomVolumeCommand = new DelegateCommand<object>(async (param) =>
                    {
                        RoomVolume = await zoneViewModel.GetRoomVolume(Udn);
                    });
                }
                return getRoomVolumeCommand;
            }
        }

        private ICommand getRoomMuteCommand;
        public ICommand GetRoomMuteCommand
        {
            get
            {
                if (getRoomMuteCommand == null)
                {
                    getRoomMuteCommand = new DelegateCommand<object>(async (param) =>
                    {
                        RoomMute = await zoneViewModel.GetRoomMute(Udn);
                    });
                }
                return getRoomMuteCommand;
            }
        }

        private ICommand setRoomVolumeCommand;
        public ICommand SetRoomVolumeCommand
        {
            get
            {
                if (setRoomVolumeCommand == null)
                {
                    setRoomVolumeCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (param is Slider slider)
                        {
                            await zoneViewModel.SetRoomVolume(Udn, slider.Value);
                        }
                    });
                }
                return setRoomVolumeCommand;
            }
        }

        private ICommand setRoomMuteCommand;
        public ICommand SetRoomMuteCommand
        {
            get
            {
                if (setRoomMuteCommand == null)
                {
                    setRoomMuteCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (param is CheckBox checkbox)
                        {
                            await zoneViewModel.SetRoomMute(Udn, (checkbox?.IsChecked ?? false));
                        }
                    });
                }
                return setRoomMuteCommand;
            }
        }

        private ICommand setRoomStateCommand;
        public ICommand SetRoomStateCommand
        {
            get
            {
                if (setRoomStateCommand == null)
                {
                    setRoomStateCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (param is bool state)
                        {
                            await zoneViewModel.SetRommState(Udn, state);
                        }
                    });
                }
                return setRoomStateCommand;
            }
        }

        public RoomViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, ZoneViewModel zoneViewModelInstance, MediaRenderer roomRendererInstance, string name, string udn, string powerState)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;

            zoneViewModel = zoneViewModelInstance;
            roomRenderer = roomRendererInstance;

            Name = name;
            Udn = udn;
            this.powerState = powerState;

            switch (powerState)
            {
                case "ACTIVE":
                    isActive = true;
                    break;
                case "AUTOMATIC_STANDBY":
                case "MANUAL_STANDBY":
                    isActive = false;
                    break;
                default:
                    break;
            }

            GetRoomVolumeCommand.Execute(null);
            GetRoomMuteCommand.Execute(null);

            eventAggregator.GetEvent<PowerStateChangedEvent>().Subscribe(onPowerStateChanged,
                                     ThreadOption.UIThread, false,
                                     device => device.MediaDevice == roomRenderer);
        }

        private void onPowerStateChanged(RaumFeldEvent args)
        {
            if (args.ChangedValues.TryGetValue("val", out string powerstate))
            {
                switch (powerstate)
                {
                    case "ACTIVE":
                        IsActive = true;
                        break;
                    case "AUTOMATIC_STANDBY":
                    case "MANUAL_STANDBY":
                        IsActive = false;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
