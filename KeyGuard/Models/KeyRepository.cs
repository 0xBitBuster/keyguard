using KeyGuard.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KeyGuard.Models
{
    public class KeyRepository
    {
        private static List<Key> _keys = new List<Key>() {
            new Key { Id = "0f8fad5b-d9cb-469f-a165-70867728950e", Service="Google", ServiceLink = "https://google.com", Username = "john_doe", Email="john_doe@gmail.com", Password="123456789", SecurityQuestion = "What's your favorite place? Johnny's Pizza.", Other = "Account will soon be deleted." }
        };

        public static event PropertyChangedEventHandler PropertyChanged;
        public static List<Key> Keys { 
            get { return _keys; } 
            set { 
                _keys = value;
                PropertyChanged?.Invoke(Keys, new PropertyChangedEventArgs(nameof(Key)));
            } 
        }

        public static bool FileLoaded = false;

        public static Key GetKeyById(string keyId)
        {
            var key = _keys.FirstOrDefault(x => x.Id == keyId);
            if (key != null)
            {
                return new Key
                {
                    Id = key.Id,
                    Service = key.Service,
                    ServiceLink = key.ServiceLink,
                    Username = key.Username,
                    Email = key.Email,
                    Password = key.Password,
                    SecurityQuestion = key.SecurityQuestion,
                    Other = key.Other
                };
            }

            return null;
        }

        public static void ResetKeys()
        {
            Keys = new List<Key> {
                new Key { Id = "0f8fad5b-d9cb-469f-a165-70867728950e", Service = "Google", ServiceLink = "https://google.com", Username = "john_doe", Email = "john_doe@gmail.com", Password = "123456789", SecurityQuestion = "What's your favorite place? Johnny's Pizza.", Other = "Account will soon be deleted." }
            };

            PropertyChanged?.Invoke(Keys, new PropertyChangedEventArgs(nameof(Key)));
        }

        public static async Task AddKeys(List<Key> keys)
        {
            Keys.AddRange(keys);
            PropertyChanged?.Invoke(Keys, new PropertyChangedEventArgs(nameof(Key)));
            await WriteKeysToFile();
        }

        public static async void AddKey(Key key)
        {
            key.Id = Guid.NewGuid().ToString();

            _keys.Add(key);
            PropertyChanged?.Invoke(Keys, new PropertyChangedEventArgs(nameof(Key)));

            await WriteKeysToFile();
        }

        public static async void UpdateKey(string keyId, Key key)
        {
            var keyToUpdate = _keys.FirstOrDefault(x => x.Id == keyId);
            if (keyToUpdate != null)
            {
                keyToUpdate.Service = key.Service;
                keyToUpdate.ServiceLink = key.ServiceLink;
                keyToUpdate.Username = key.Username;
                keyToUpdate.Email = key.Email;
                keyToUpdate.Password = key.Password;
                keyToUpdate.SecurityQuestion = key.SecurityQuestion;
                keyToUpdate.Other = key.Other;

                await WriteKeysToFile();
            }
        }

        public static async void DeleteKey(string keyId)
        {
            var keyToRemove = _keys.FirstOrDefault(x => x.Id == keyId);
            if (keyToRemove != null)
            {
                _keys.Remove(keyToRemove);
                PropertyChanged?.Invoke(Keys, new PropertyChangedEventArgs(nameof(Key)));

                await WriteKeysToFile();
            }
        }

        public static List<Key> SearchKeys(string query)
        {
            var keys = _keys.Where(x => !string.IsNullOrWhiteSpace(x.Service) && x.Service.Contains(query, StringComparison.OrdinalIgnoreCase))?.ToList();

            if (keys == null || keys.Count == 0)
                keys = _keys.Where(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email.Contains(query, StringComparison.OrdinalIgnoreCase))?.ToList();
            else
                return keys;

            if (keys == null || keys.Count == 0)
                keys = _keys.Where(x => !string.IsNullOrWhiteSpace(x.Username) && x.Username.Contains(query, StringComparison.OrdinalIgnoreCase))?.ToList();
            else
                return keys;

            return keys;
        }

        public static string ExportKeysAsJson() => JsonConvert.SerializeObject(_keys);
        public static string ExportKeysAsJson(JsonSerializerSettings options) => JsonConvert.SerializeObject(_keys, options);

        public static async Task WriteKeysToFile()
        {
            var path = Preferences.Default.Get("KeyFilePath", "");
            if (path == "")
            {
                await Shell.Current.DisplayAlert("Error", "File path to keys not found. Please re-import the keys file.", "OK");
                return;
            }

            try
            {
                using FileStream outputStream = File.Create(path);
                using StreamWriter streamWriter = new StreamWriter(outputStream);

                string cipherText = EncryptionHelper.SimpleEncryptWithPassword(ExportKeysAsJson(), await SecureStorage.Default.GetAsync("ManagerPassword"));
                
                await streamWriter.WriteAsync(cipherText);
            } catch
            {
                await Shell.Current.DisplayAlert("Error", "Something went wrong while saving the keys.", "OK");
            }
        }

        public static async Task ReadKeysFromFile()
        {
            var path = Preferences.Default.Get("KeyFilePath", "");
            if (path == "" || !File.Exists(path))
            {
                await Shell.Current.DisplayAlert("Error", "The keys file could not be found.", "OK");
                return;
            }

            using StreamReader streamReader = new StreamReader(path);
            string cipherText = streamReader.ReadToEnd();
            string content = EncryptionHelper.SimpleDecryptWithPassword(cipherText, await SecureStorage.Default.GetAsync("ManagerPassword"));
            if (content == null)
                throw new Exception("Password is incorrect.");

            try { 
                _keys = JsonConvert.DeserializeObject<List<Key>>(content); 
            } catch
            {
                throw new Exception("The file could not be parsed, maybe it is corrupted.");
            }
        }
    }
}
