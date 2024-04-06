using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiNotifications.Services
{

    public class GeofencePreferencePlace : INotifyPropertyChanged
    {
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string AddressName { get; set; }
        public DateTime EnterAt { get; set; }
        public DateTime LeftAt { get; set; }
        public string LatLon => $"{Lat} {Lon}";
        public event PropertyChangedEventHandler PropertyChanged;
        public uint Id { get; set; }

        int radius;
        public int Radius
        {
            get => radius;
            set
            {
                if (radius != value)
                {
                    radius = value;

                    OnPropertyChanged();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public record struct GeofencePreference(bool active, uint currentPlace, List<GeofencePreferencePlace> places);

}
