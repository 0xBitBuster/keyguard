using CommunityToolkit.Mvvm.Input;
using KeyGuard.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    public partial class HomeViewModel
    {
        public HomeViewModel()
        {
            CheckForLatestFile();
        }

        public async void CheckForLatestFile()
        {
            var path = Preferences.Default.Get("KeyFilePath", "");
            if (path == "" || !File.Exists(path))
                return;

            await Shell.Current.GoToAsync(nameof(DecryptPage));
        }

        [RelayCommand]
        public async Task GoToCreateManager()
        {
            await Shell.Current.GoToAsync(nameof(CreateManagerPage));
        }

        [RelayCommand]
        public async Task OpenFile()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    if (!result.FileName.EndsWith(".dat"))
                    {
                        await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                        return;
                    }

                    Preferences.Set("KeyFilePath", result.FullPath);

                    await Shell.Current.GoToAsync(nameof(DecryptPage));
                }
            }
            catch
            {
                 await Shell.Current.DisplayAlert("Error", "Something went wrong while opening your file. Please try again.", "OK");
            }
        }
    }
}
