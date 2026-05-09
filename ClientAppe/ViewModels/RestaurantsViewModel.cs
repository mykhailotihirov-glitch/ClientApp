using ClientAppe.Models;
using ClientAppe.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientAppe.ViewModels
{
    public class RestaurantsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly MainViewModel _mainViewModel;

        // Зберігаємо повний список ресторанів
        private List<RestaurantModel> _allRestaurants = new List<RestaurantModel>();

        private ObservableCollection<RestaurantModel> _restaurants;
        public ObservableCollection<RestaurantModel> Restaurants
        {
            get => _restaurants;
            set { _restaurants = value; OnPropertyChanged(); }
        }

        private string _foundCountText;
        public string FoundCountText
        {
            get => _foundCountText;
            set { _foundCountText = value; OnPropertyChanged(); }
        }

        // Зберігаємо назву активного фільтра
        private string _activeFilter = "Всі заклади";
        public string ActiveFilter
        {
            get => _activeFilter;
            set { _activeFilter = value; OnPropertyChanged(); }
        }

        // Зберігаємо назву активного сортування
        private string _activeSort = "Популярні";
        public string ActiveSort
        {
            get => _activeSort;
            set { _activeSort = value; OnPropertyChanged(); }
        }

        public ICommand FilterCategoryCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand NavigateToDetailsCommand { get; }

        public RestaurantsViewModel(MainViewModel mainVM, string initialCategory = "Всі заклади", ApiService apiService = null)
        {
            _apiService = apiService ?? new ApiService();
            _mainViewModel = mainVM;

            // Просто зберігаємо передану категорію
            ActiveFilter = initialCategory;
            ActiveSort = "Rating";

            FilterCategoryCommand = new RelayCommand(category =>
            {
                if (category is string catStr)
                {
                    ActiveFilter = catStr;
                    ApplyFiltersAndSort();
                }
            });

            SortCommand = new RelayCommand(sortType =>
            {
                if (sortType is string sortStr)
                {
                    ActiveSort = sortStr;
                    ApplyFiltersAndSort();
                }
            });

            NavigateToDetailsCommand = new RelayCommand(param =>
            {
                if (param is RestaurantModel selected)
                {
                    _mainViewModel.NavigateToDetails(selected);
                }
            });
            LoadData();
        }

        // Робить і фільтрацію, і сортування одночасно
        private void ApplyFiltersAndSort()
        {
            var result = _allRestaurants.AsEnumerable();

            if (ActiveFilter != "Всі заклади")
            {
                // Тепер ми шукаємо точний збіг по Категорії
                result = result.Where(r =>
                    r.Category != null &&
                    r.Category.Trim().ToLower() == ActiveFilter.Trim().ToLower()
                );
            }

            if (ActiveSort == "Rating")
            {
                result = result.OrderByDescending(r => r.Rating);
            }
            else if (ActiveSort == "Fastest")
            {
                result = result.OrderBy(r => r.DeliveryTime);
            }

            var finalFilteredList = result.ToList();
            Restaurants = new ObservableCollection<RestaurantModel>(finalFilteredList);
            FoundCountText = $"Знайдено {Restaurants.Count} закладів";
        }

        public async void LoadData()
        {
            try
            {
                var data = await _apiService.GetRestaurantsAsync();
                if (data != null)
                {
                    _allRestaurants = data;

                    foreach (var restaurant in _allRestaurants)
                    {
                        restaurant.ImageBitmap = await LoadImageAsync(restaurant.ImagePath);
                    }

                    ApplyFiltersAndSort();
                }
            }
            catch (Exception ex)
            {
                FoundCountText = "Помилка зв'язку з сервером";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public async Task<Bitmap> LoadImageAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            string imageUrl = $"https://localhost:44333/Images/{fileName}";

            using var client = new HttpClient();
            var response = await client.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    return new Bitmap(stream);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}