using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Helpers;
using KeyGuard.Models;
using KeyGuard.Views;
using KeyGuard.Views.Mobile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.ViewModels
{
    public partial class AppShellViewModel
    {
        public bool IsMobile { get; set; }
        public AppShellViewModel()
        {
            IsMobile = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;
        }

        #region File Commands
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

        [RelayCommand]
        public async Task CreateFile()
        {
            await Shell.Current.GoToAsync(nameof(CreateManagerPage));
        }

        [RelayCommand]
        public async Task ChangePassword()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            string passwordInput;
            while (true)
            {
                passwordInput = await Shell.Current.DisplayPromptAsync("Password required", "Please enter your current password.", "OK", "Cancel");
                if (passwordInput == null)
                    return;

                if (passwordInput.Length < 8)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                    continue;
                }

                if (passwordInput != await SecureStorage.Default.GetAsync("ManagerPassword"))
                {
                    await Shell.Current.DisplayAlert("Error", "Password is incorrect.", "OK");
                    continue;
                }

                break;
            }

            string newPasswordInput;
            while (true)
            {
                newPasswordInput = await Shell.Current.DisplayPromptAsync("Password required", "Please enter your new password.", "OK", "Cancel");
                if (newPasswordInput == null)
                    return;

                if (newPasswordInput.Length < 8)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                    continue;
                }
                else if (passwordInput.Length > 512)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at most 512 characters long.", "OK");
                    continue;
                }

                break;
            }

            string confirmNewPasswordInput;
            while (true)
            {
                confirmNewPasswordInput = await Shell.Current.DisplayPromptAsync("Password required", "Please confirm your new password.", "OK", "Cancel");
                if (confirmNewPasswordInput == null)
                    return;

                if (confirmNewPasswordInput != newPasswordInput)
                {
                    await Shell.Current.DisplayAlert("Error", "Passwords do not match.", "OK");
                    continue;
                }

                break;
            }

            await SecureStorage.Default.SetAsync("ManagerPassword", confirmNewPasswordInput);
            await KeyRepository.WriteKeysToFile();
            await Shell.Current.DisplayAlert("Info", "Your password has been changed.", "OK");
        }

        [RelayCommand]
        public async Task ExportAsText()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            try
            {
                var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(KeyRepository.ExportKeysAsJson(new JsonSerializerSettings { Formatting = Formatting.Indented }))); ;
                var savedFile = await FileSaver.Default.SaveAsync("keys.txt", keyStream, default);
                savedFile.EnsureSuccess();

                await Shell.Current.DisplayAlert("Info", "Your keys have been exported.", "OK");
            } catch
            {
                await Shell.Current.DisplayAlert("Error", "Something went wrong while exporting your keys.", "OK");
            }
        }

        [RelayCommand]
        public async Task EmbedInImage()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            var pickedFile = await FilePicker.Default.PickAsync();
            if (pickedFile == null)
                return;

            var fileExtension = Path.GetExtension(pickedFile.FullPath);
            if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
            {
                await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                return;
            }

            var jsonKeysString = KeyRepository.ExportKeysAsJson();
            var outputPath = Path.Combine(Directory.GetParent(pickedFile.FullPath).FullName, "keyguard_stego" + fileExtension);
            if (File.Exists(outputPath)) { 
                bool shouldOverwrite = await Shell.Current.DisplayAlert("Confirmation", "The image keyguard_stego" + fileExtension + " already exists. Do you want to overwrite the file?", "Yes", "No");
                if (!shouldOverwrite)
                    return;
            }

            try
            {
                SteganographyHelper.EmbedText(pickedFile.FullPath, outputPath, jsonKeysString);
            } catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
                return;
            }

            await Shell.Current.DisplayAlert("Info", "Your keys have been embedded into " + outputPath, "OK");
        }

        [RelayCommand]
        public async Task EmbedInImageEncrypted()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            var pickedFile = await FilePicker.Default.PickAsync();
            if (pickedFile == null)
                return;

            var fileExtension = Path.GetExtension(pickedFile.FullPath);
            if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
            {
                await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                return;
            }

            var outputPath = Path.Combine(Directory.GetParent(pickedFile.FullPath).FullName, "keyguard_stego.png");
            if (File.Exists(outputPath))
            {
                bool shouldOverwrite = await Shell.Current.DisplayAlert("Confirmation", "The image keyguard_stego" + fileExtension + " already exists. Do you want to overwrite the file?", "Yes", "No");
                if (!shouldOverwrite)
                    return;
            }

            var encryptedJsonKeysString = EncryptionHelper.SimpleEncryptWithPassword(KeyRepository.ExportKeysAsJson(), await SecureStorage.Default.GetAsync("ManagerPassword"));

            try
            {
                SteganographyHelper.EmbedText(pickedFile.FullPath, outputPath, encryptedJsonKeysString);
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
                return;
            }

            await Shell.Current.DisplayAlert("Info", "Your keys have been embedded into " + outputPath, "OK");
        }

        [RelayCommand]
        public async Task ImportFromText()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            var pickedFile = await FilePicker.Default.PickAsync();
            if (pickedFile == null)
                return;

            var fileExtension = Path.GetExtension(pickedFile.FullPath);
            if (fileExtension != ".txt" && fileExtension != ".json")
            {
                await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                return;
            }

            using StreamReader streamReader = new StreamReader(pickedFile.FullPath);
            string content = streamReader.ReadToEnd();
            List<Key> keys;

            try
            {
                keys = JsonConvert.DeserializeObject<List<Key>>(content);

                if (keys.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "The selected file has no keys in it.", "OK");
                    return;
                }
            }
            catch
            {
                await Shell.Current.DisplayAlert("Error", "The file could not be parsed, maybe it is corrupted.", "OK");
                return;
            }

            await KeyRepository.AddKeys(keys);
            await Shell.Current.DisplayAlert("Info", "The keys were imported successfully.", "OK");
        }

        [RelayCommand]
        public async Task ImportFromUnencryptedImage()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            var pickedFile = await FilePicker.Default.PickAsync();
            if (pickedFile == null)
                return;

            var fileExtension = Path.GetExtension(pickedFile.FullPath);
            if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
            {
                await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                return;
            }

            List<Key> keys;
            try
            {
                var content = SteganographyHelper.ExtractText(pickedFile.FullPath);
                if (String.IsNullOrEmpty(content))
                {
                    await Shell.Current.DisplayAlert("Error", "The extracted text from the image was empty.", "OK");
                    return;
                }

                keys = JsonConvert.DeserializeObject<List<Key>>(content);

                if (keys.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "The embedded image has no keys in it.", "OK");
                    return;
                }
            }
            catch
            {
                await Shell.Current.DisplayAlert("Error", "The embedded text could not be extracted, maybe it is corrupted.", "OK");
                return;
            }

            await KeyRepository.AddKeys(keys);
            await Shell.Current.DisplayAlert("Info", "The keys were imported successfully.", "OK");
        }

        [RelayCommand]
        public async Task ImportFromEncryptedImage()
        {
            if (!KeyRepository.FileLoaded)
            {
                await Shell.Current.DisplayAlert("Info", "Please open a file first.", "OK");
                return;
            }

            var pickedFile = await FilePicker.Default.PickAsync();
            if (pickedFile == null)
                return;

            var fileExtension = Path.GetExtension(pickedFile.FullPath);
            if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
            {
                await Shell.Current.DisplayAlert("Error", "The selected file has a unsupported file extension.", "OK");
                return;
            }

            var cipherContent = SteganographyHelper.ExtractText(pickedFile.FullPath);
            if (String.IsNullOrEmpty(cipherContent))
            {
                await Shell.Current.DisplayAlert("Error", "The extracted text from the image was empty.", "OK");
                return;
            }

            string content;
            while (true)
            {
                var passwordInput = await Shell.Current.DisplayPromptAsync("Password required", "Please enter the password of the encrypted image.", "OK", "Cancel");
                if (passwordInput == null)
                    return;

                if (passwordInput.Length < 8)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                    continue;
                }

                try
                {
                    content = EncryptionHelper.SimpleDecryptWithPassword(cipherContent, passwordInput);
                    if (content == null)
                    {
                        await Shell.Current.DisplayAlert("Error", "Password is incorrect.", "OK");
                        continue;
                    } else {
                        break;
                    }
                } catch
                {
                    await Shell.Current.DisplayAlert("Error", "Something went wrong while decrypting the image content.", "OK");
                    return;
                }
            }

            List<Key> keys;
            try
            {
                keys = JsonConvert.DeserializeObject<List<Key>>(content);

                if (keys.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "The embedded image has no keys in it.", "OK");
                    return;
                }
            }
            catch
            {
                await Shell.Current.DisplayAlert("Error", "The embedded text could not be extracted, maybe it is corrupted.", "OK");
                return;
            }

            await KeyRepository.AddKeys(keys);
            await Shell.Current.DisplayAlert("Info", "The keys were imported successfully.", "OK");
        }

        [RelayCommand]
        public async Task ConfigurePasswordGenerator()
        {
            string input;
            int pwLength;
            while (true)
            {
                input = await Shell.Current.DisplayPromptAsync("Password Generator", "Please enter how many characters the generated password should have.", "OK", "Cancel");
                if (input == null)
                    return;

                bool isInteger = int.TryParse(input, out pwLength);
                if (!isInteger)
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a number.", "OK");
                    continue;
                }
                
                if (pwLength < 8)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                    continue;
                }
                else if (pwLength > 512)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at most 512 characters long.", "OK");
                    continue;
                }

                break;
            }

            Preferences.Default.Set("PasswordGeneratorLength", pwLength);
        }
        #endregion

        #region About Commands
        [RelayCommand]
        public async Task ShowGithub()
        {
            await Launcher.OpenAsync("https://github.com/0xBitBuster/keyguard");
        }

        [RelayCommand]
        public void ShowVersion()
        {
            Shell.Current.DisplayAlert("Version", "You are using v" + AppInfo.Current.VersionString, "OK");
        }

        [RelayCommand]
        public void ShowLicense()
        {
            Shell.Current.DisplayAlert("License", """
Copyright (c) 2023 Tobias Scharl

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
""", "OK");
        }
        #endregion
    }
}
