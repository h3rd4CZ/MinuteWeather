using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiNotifications.ViewModels
{
    public partial class PersonViewModel : ObservableObject
    {
        [ObservableProperty]
        public int age;

        [RelayCommand]
        public async Task UpdateAge(int newAge) => age = newAge;
    }
}
