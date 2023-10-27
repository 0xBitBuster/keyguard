using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Helpers;
using KeyGuard.Models;
using KeyGuard.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    public partial class CreateManagerViewModel : ObservableValidator
    {
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [MaxLength(512, ErrorMessage = "Password must be at most 512 characters long.")]
        [ObservableProperty]
        private string _password;

        [Required(ErrorMessage = "Please confirm your password.")]
        [ObservableProperty]
        private string _confirmPassword;

        [RelayCommand]
        public async Task Create()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                await Shell.Current.DisplayAlert("Error", GetErrors().First().ToString(), "OK");
                return;
            }
            else if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            try
            {
                KeyRepository.ResetKeys();
                string cipherText = EncryptionHelper.SimpleEncryptWithPassword(KeyRepository.ExportKeysAsJson(), Password);

                var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(cipherText));
                var savedFile = await FileSaver.Default.SaveAsync("keyguard.dat", keyStream, default);
                savedFile.EnsureSuccess();

                Preferences.Default.Set("KeyFilePath", savedFile.FilePath);
                await SecureStorage.Default.SetAsync("ManagerPassword", Password);
                KeyRepository.FileLoaded = true;
                await Shell.Current.GoToAsync(nameof(KeysPage));
            }
            catch
            {
                await Shell.Current.DisplayAlert("Error", "Something went wrong while creating the file. Please try again or select another folder.", "OK");
            }
        }
    }
}
