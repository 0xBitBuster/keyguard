using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyGuard.Models;
using KeyGuard.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KeyGuard.ViewModels
{
    public partial class KeysViewModel : ObservableObject
    {
        private ObservableCollection<Key> keys;
        public ObservableCollection<Key> Keys
        {
            get => keys;
            set
            {
                if (keys == value) 
                    return;

                keys = value;
                OnPropertyChanged();
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetProperty(ref _searchQuery, value); HandleSearch(); }
        }

        private Key _selectedKey;
        public Key SelectedKey
        {
            get => _selectedKey;
            set
            {
                SetProperty(ref _selectedKey, value);

                if (value != null && Shell.Current.IsLoaded)
                {
                    SetProperty(ref _selectedKey, null);
                    Shell.Current.GoToAsync($"{nameof(EditKeyPage)}?Id={value.Id}");
                }
            }
        }

        public KeysViewModel() {
            Keys = new ObservableCollection<Key>(KeyRepository.Keys);
            KeyRepository.PropertyChanged += ContactRepository_PropertyChanged;
        }

        private void ContactRepository_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is List<Key>)
            {
                Keys = new ObservableCollection<Key>(KeyRepository.Keys);
            }
        }

        [RelayCommand]
        public async void DeleteKey(object key)
        {
            bool continueDeletetion = await Shell.Current.DisplayAlert("Attention required!", "Are you sure you want to delete the key?", "Yes", "No");
            if (continueDeletetion == false)
                return;

            KeyRepository.DeleteKey(((Key)key).Id);

            Keys = new ObservableCollection<Key>(KeyRepository.Keys);
            OnPropertyChanged("Keys");
        }

        [RelayCommand]
        public void HandleSearch()
        {
            Keys = new ObservableCollection<Key>(KeyRepository.SearchKeys(_searchQuery));
            OnPropertyChanged("Keys");
        }

        [RelayCommand]
        public async Task GoToAddKey()
        {
            await Shell.Current.GoToAsync(nameof(AddKeyPage));
        }
    }
}
