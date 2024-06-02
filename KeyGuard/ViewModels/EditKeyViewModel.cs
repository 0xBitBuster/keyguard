using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Helpers;
using KeyGuard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    [QueryProperty(nameof(KeyId), "Id")]
    public partial class EditKeyViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _service = "";

        [ObservableProperty]
        private string _serviceLink = "";

        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _securityQuestion = "";

        [ObservableProperty]
        private string _other = "";

        private Key key;
        public string KeyId
        {
            set
            {
                key = KeyRepository.GetKeyById(value);

                if (key != null)
                {
                    Service = key.Service;
                    ServiceLink = key.ServiceLink;
                    Username = key.Username;
                    Email = key.Email;
                    Password = key.Password;
                    SecurityQuestion = key.SecurityQuestion;
                    Other = key.Other;
                }
            }
        }

        [RelayCommand]
        public void GenerateSecurePassword()
        {
            var passwordLength = Preferences.Default.Get("PasswordGeneratorLength", 18);
            Password = PasswordGenerator.GeneratePassword(passwordLength);
        }

        [RelayCommand]
        public async Task UpdateKey() 
        {
            var updatedKey = new Key() { 
                Service = Service?.Trim(), 
                ServiceLink = ServiceLink?.Trim(), 
                Username = Username, 
                Email = Email, 
                Password = Password, 
                SecurityQuestion = SecurityQuestion, 
                Other = Other 
            };

            var validation = updatedKey.Validate();
            if (validation != ValidationResult.Success)
            {
                await Shell.Current.DisplayAlert("Error", validation.ErrorMessage, "OK");
                return;
            }

            var oldKeyId = key.Id;
            key = updatedKey;

            KeyRepository.UpdateKey(oldKeyId, key);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task DeleteKey() {
            bool continueDeletetion = await Shell.Current.DisplayAlert("Attention required!", "Are you sure you want to delete the key?", "Yes", "No");
            if (continueDeletetion == false)
                return;

            KeyRepository.DeleteKey(key.Id);

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
