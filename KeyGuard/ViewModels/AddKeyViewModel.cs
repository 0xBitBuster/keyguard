using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Helpers;
using KeyGuard.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    public partial class AddKeyViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _service;

        [ObservableProperty]
        private string _serviceLink;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _securityQuestion;

        [ObservableProperty]
        private string _other;

        [RelayCommand]
        public void GenerateSecurePassword()
        {
            var passwordLength = Preferences.Default.Get("PasswordGeneratorLength", 18);
            Password = PasswordGenerator.GeneratePassword(passwordLength);
        }

        [RelayCommand]
        public async Task SaveKey()
        {
            var key = new Key() { Service = Service?.Trim(), ServiceLink = ServiceLink?.Trim(), Username = Username?.Trim(), Email = Email?.Trim(), Password = Password, SecurityQuestion = SecurityQuestion?.Trim(), Other = Other?.Trim() };

            var validation = key.Validate();
            if (validation != ValidationResult.Success)
            {
                await Shell.Current.DisplayAlert("Error", validation.ErrorMessage, "OK");
                return;
            }

            KeyRepository.AddKey(key);
            OnPropertyChanged(nameof(KeyRepository.Keys));

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
