using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Models;
using KeyGuard.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    public partial class DecryptViewModel : ObservableValidator
    {
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [MaxLength(512, ErrorMessage = "Password must be at most 512 characters long.")]
        [ObservableProperty]
        private string _password;

        public string FilePath { get; set; }

        public DecryptViewModel()
        {
            FilePath = Preferences.Default.Get("KeyFilePath", "");
        }

        [RelayCommand]
        public async Task Decrypt()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                await Shell.Current.DisplayAlert("Error", GetErrors().First().ToString(), "OK");
                return;
            }

            await SecureStorage.Default.SetAsync("ManagerPassword", Password);
            try { await KeyRepository.ReadKeysFromFile(); }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
                return;
            }

            KeyRepository.FileLoaded = true;
            await Shell.Current.GoToAsync(nameof(KeysPage));
        }
    }
}
