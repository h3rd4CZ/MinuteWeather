
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiNotifications.Services;
using System.Collections.ObjectModel;

namespace MauiNotifications.Model.MVVM
{
    public partial class GeofencePreferenceModel : ObservableObject, IQueryAttributable
    {
        private readonly PreferenceService preferenceService;
        private readonly IMessage message;
        private readonly IAlarmService alarmService;
        Random rand;

        public ObservableCollection<GeofencePreferencePlace> geofences { get; }

        private const int DEFAULT_GEOFENCE_RADIUS = 5000;

        public const double MIN_GEOFENCE_RADIUS = 100;
        public const double MAX_GEOFENCE_RADIUS = 10000;

        [ObservableProperty]
        uint currentPlace;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(
            nameof(AddGeofencePlaceCommand),
            nameof(SavePreferenceCommand),
            nameof(DeleteGeofencePlaceCommand),
            nameof(SyncGeofencePlaceCommand))]
        bool geofencingEnabled;

        public GeofencePreferenceModel(
            PreferenceService preferenceService,
            IMessage message,
            IAlarmService alarmService)
        {
            var geofencePreference = preferenceService.GetGeofencePreference();

            var places = geofencePreference.places.OrderByDescending(p => p.EnterAt);

            geofences = new ObservableCollection<GeofencePreferencePlace>(places);

            CurrentPlace = geofencePreference.currentPlace;

            GeofencingEnabled = geofencePreference.active;

            this.preferenceService = preferenceService;
            this.message = message;
            this.alarmService = alarmService;
            rand = new Random();
        }

        [RelayCommand]
        async void ToggleGeofence(ToggledEventArgs args)
        {
            GeofencingEnabled = args.Value;
                        
            await SavePreference();
        }
                

        [RelayCommand(CanExecute = nameof(GeofencingEnabled))]
        async Task AddGeofencePlace()
        {
            await Shell.Current.GoToAsync("findloc");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("data", out object latLon) && latLon is Position position)
            {
                query.TryGetValue("address", out object address);

                var freeFormAddress = address is string sAddress ? sAddress : string.Empty;

                var geofenceId = (uint)rand.Next(1, int.MaxValue);

                var newPlace = new GeofencePreferencePlace
                {
                    Lat = position.lat,
                    Lon = position.lon,
                    AddressName = freeFormAddress,
                    Radius = DEFAULT_GEOFENCE_RADIUS,
                    Id = geofenceId
                };

                geofences.Add(newPlace);

                alarmService.RegisterGeofence(newPlace);

                SavePreference("Place registered successfully").GetAwaiter().GetResult();       
            }
        }


        [RelayCommand(CanExecute = nameof(GeofencingEnabled))]
        async void SyncGeofencePlace(GeofencePreferencePlace place)
        {
            alarmService.UnregisterGeofence(place);

            alarmService.RegisterGeofence(place); 

            await SavePreference("Place modified successfully");
        }

        [RelayCommand(CanExecute = nameof(GeofencingEnabled))]
        async void DeleteGeofencePlace(GeofencePreferencePlace geofencePlace)
        {
            var challenge = true;

            if (OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                challenge = await Application.Current.MainPage.DisplayAlert("Delete", "Really delete geofence?", "OK", "Cancel");
            }

            if (challenge)
            {
                var geofencePreferencePlaceId = geofencePlace.Id;

                var place = geofences.FirstOrDefault(g => g.Id == geofencePreferencePlaceId);
                                
                geofences.Remove(place);

                if (place.Id == CurrentPlace) CurrentPlace = 0;

                alarmService.UnregisterGeofence(place);

                await SavePreference("Place unregistered successfully");
            }
        }

        [RelayCommand(CanExecute = nameof(GeofencingEnabled))]
        async Task SavePreference(string msg = default)
        {
            try
            {
                var preference = new GeofencePreference(GeofencingEnabled, CurrentPlace, geofences.ToList());

                preferenceService.SaveGeofencePreference(preference);

                if (!string.IsNullOrWhiteSpace(msg)) message.ShowMessage(msg);
            }
            catch(Exception ex)
            {
                await HandleUnexected(ex);
            }
        }

        private async Task HandleUnexected(Exception ex)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
            else
            {
                message.ShowMessage(ex.ToString());
            }
        }
    }
}
