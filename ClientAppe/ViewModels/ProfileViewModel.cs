using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ClientAppe.Models;
using ClientAppe.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientAppe.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ApiService _apiService = new ApiService();

        private UserModel _user;
        public UserModel User
        {
            get => _user;
            set { _user = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserInitial)); }
        }

        private string _activeTab = "Profile";
        public string ActiveTab
        {
            get => _activeTab;
            set {
                _activeTab = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileTabVisible));
                OnPropertyChanged(nameof(IsApplicationTabVisible));
                OnPropertyChanged(nameof(IsAddRestaurantTabVisible));
            }
        }
        public bool IsProfileTabVisible => ActiveTab == "Profile";
        public bool IsApplicationTabVisible => ActiveTab == "Application";
        public bool IsAddRestaurantTabVisible => ActiveTab == "AddRestaurant";

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }
        private string _editLogin;
        public string EditLogin
        {
            get => _editLogin;
            set { _editLogin = value; OnPropertyChanged(); }
        }

        private string _editPhone;
        public string EditPhone
        {
            get => _editPhone;
            set { _editPhone = value; OnPropertyChanged(); }
        }

        private string _editPassword;
        public string EditPassword
        {
            get => _editPassword;
            set { _editPassword = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        public string UserInitial => !string.IsNullOrEmpty(User?.Login) ? User.Login[0].ToString().ToUpper() : "?";

        private string _appFullName;
        public string AppFullName { get => _appFullName; set { _appFullName = value; OnPropertyChanged(); } }

        private string _appPhone;
        public string AppPhone { get => _appPhone; set { _appPhone = value; OnPropertyChanged(); } }

        private string _appEmail;
        public string AppEmail { get => _appEmail; set { _appEmail = value; OnPropertyChanged(); } }

        private string _appDescription;
        public string AppDescription { get => _appDescription; set { _appDescription = value; OnPropertyChanged(); } }

        private string _appMessage;
        public string AppMessage { get => _appMessage; set { _appMessage = value; OnPropertyChanged(); } }

        private string _appMessageColor = "#EF4444";
        public string AppMessageColor { get => _appMessageColor; set { _appMessageColor = value; OnPropertyChanged(); } }

        public string RestName { get; set; }
        private int _currentRestaurantId = 0;
        public string RestaurantFormTitle => _currentRestaurantId > 0 ? "Редагування закладу" : "Додати власний заклад";
        public string RestaurantSubmitButtonText => _currentRestaurantId > 0 ? "Зберегти зміни" : "Зберегти новий заклад у БД";
        public List<string> RestaurantCategories { get; } = new List<string> { "Ресторан", "Фаст-фуд", "Інше" };
        public string RestCategory { get; set; }
        public string RestDeliveryTime { get; set; }
        public string RestDescription { get; set; }
        public string RestImagePath { get; set; }
        public string RestAddress { get; set; }

        private string _restMessage;
        public string RestMessage { get => _restMessage; set { _restMessage = value; OnPropertyChanged(); } }
        public string RestMessageColor { get; set; } = "#EF4444";

        // Динамічний список страв для меню ресторану
        public ObservableCollection<FoodModel> NewRestaurantMenu { get; set; } = new ObservableCollection<FoodModel>();
      
        public ICommand EditProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ShowAddRestaurantTabCommand { get; }

        public ICommand LogoutCommand { get; }
        public ICommand ShowProfileTabCommand { get; }
        public ICommand ShowApplicationTabCommand { get; }
        public ICommand SubmitApplicationCommand { get; }

        public ICommand AddMenuFieldCommand { get; }
        public ICommand ImportJsonCommand { get; }
        public ICommand SaveRestaurantCommand { get; }
        public ICommand RemoveMenuFieldCommand { get; }
        public ICommand UploadRestaurantImageCommand { get; }
        public ICommand UploadMenuImageCommand { get; }

        public ProfileViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadProfile();

            EditProfileCommand = new RelayCommand(o => { EditLogin = User.Login; EditPhone = User.Phone; EditPassword = ""; IsEditing = true; });
            CancelEditCommand = new RelayCommand(o => { IsEditing = false; });
            SaveProfileCommand = new RelayCommand(async o => {
                ErrorMessage = "";
                string phonePattern = @"^\+?[0-9]{10,12}$";
                if (!Regex.IsMatch(EditPhone, phonePattern)) { ErrorMessage = "Некоректний номер"; return; }
                if (!string.IsNullOrWhiteSpace(EditPassword) && !Regex.IsMatch(EditPassword, @"^[a-zA-Z0-9]{8,16}$")) { ErrorMessage = "Пароль: 8-16 символів"; return; }

                var updatedUser = new UserModel { Id = User.Id, Login = EditLogin, Phone = EditPhone, Password = EditPassword, Token = ApiService.CurrentUser.Token };
                if (await _apiService.UpdateProfileAsync(updatedUser))
                {
                    User.Login = EditLogin; User.Phone = EditPhone;
                    OnPropertyChanged(nameof(User)); OnPropertyChanged(nameof(UserInitial));
                    IsEditing = false;
                }
            });
            LogoutCommand = new RelayCommand(o => { ApiService.CurrentUser = null; _mainViewModel.NavigateTo(new AuthViewModel(_mainViewModel), false); });

            ShowProfileTabCommand = new RelayCommand(o => { ActiveTab = "Profile"; AppMessage = ""; RestMessage = ""; });
            ShowApplicationTabCommand = new RelayCommand(o => { ActiveTab = "Application"; AppFullName = ""; AppPhone = User?.Phone; AppEmail = User?.Email; });
            ShowAddRestaurantTabCommand = new RelayCommand(async o => {
                ActiveTab = "AddRestaurant";
                RestMessage = "";

                // Завантажуємо всі ресторани і шукаємо свій
                var allRestaurants = await _apiService.GetRestaurantsAsync();
                var myRest = allRestaurants.FirstOrDefault(r => r.OwnerId == User.Id);

                if (myRest != null)
                {
                    _currentRestaurantId = myRest.Id;
                    RestName = myRest.Name;
                    RestCategory = myRest.Category;
                    RestAddress = myRest.Address;
                    RestDeliveryTime = myRest.DeliveryTime;
                    RestDescription = myRest.Description;
                    RestImagePath = myRest.ImagePath;

                    NewRestaurantMenu.Clear();
                    if (myRest.Menu != null)
                    {
                        foreach (var item in myRest.Menu) NewRestaurantMenu.Add(item);
                    }
                }
                else
                {
                    _currentRestaurantId = 0;
                    if (NewRestaurantMenu.Count == 0) NewRestaurantMenu.Add(new FoodModel { Quantity = 1 });
                }

                OnPropertyChanged(nameof(RestName)); OnPropertyChanged(nameof(RestCategory));
                OnPropertyChanged(nameof(RestAddress)); OnPropertyChanged(nameof(RestDeliveryTime));
                OnPropertyChanged(nameof(RestDescription)); OnPropertyChanged(nameof(RestImagePath));
                OnPropertyChanged(nameof(RestaurantFormTitle)); OnPropertyChanged(nameof(RestaurantSubmitButtonText));
            });

            SubmitApplicationCommand = new RelayCommand(async o => {
                if (string.IsNullOrWhiteSpace(AppFullName) || string.IsNullOrWhiteSpace(AppPhone) || string.IsNullOrWhiteSpace(AppEmail))
                { AppMessageColor = "#EF4444"; AppMessage = "Будь ласка, заповніть усі обов'язкові поля!"; return; }

                AppMessageColor = "#10B981"; AppMessage = "Відправка...";
                if (await _apiService.SubmitPartnerApplicationAsync(AppFullName, AppPhone, AppEmail, AppDescription))
                {
                    AppMessage = "Заявку успішно відправлено! Очікуйте на рішення.";
                    await Task.Delay(1500); ActiveTab = "Profile"; AppMessage = "";
                }
                else { AppMessageColor = "#EF4444"; AppMessage = "Помилка відправки."; }
            });

            AddMenuFieldCommand = new RelayCommand(o => {
                NewRestaurantMenu.Add(new FoodModel { Quantity = 1 });
            });

            // Картинка закладу
            UploadRestaurantImageCommand = new RelayCommand(async o => {
                string filePath = await PickImageFileAsync();
                if (filePath != null)
                {
                    RestMessageColor = "#10B981"; RestMessage = "Завантаження картинки...";
                    string uploadedFileName = await _apiService.UploadImageAsync(filePath);
                    if (uploadedFileName != null) { RestImagePath = uploadedFileName; OnPropertyChanged(nameof(RestImagePath)); RestMessage = "Картинку закладу успішно завантажено!"; }
                    else { RestMessageColor = "#EF4444"; RestMessage = "Помилка завантаження картинки на сервер."; }
                }
            });

            // Картинка страви
            UploadMenuImageCommand = new RelayCommand(async foodItem => {
                if (foodItem is FoodModel food)
                {
                    string filePath = await PickImageFileAsync();
                    if (filePath != null)
                    {
                        RestMessageColor = "#10B981"; RestMessage = "Завантаження картинки страви...";
                        string uploadedFileName = await _apiService.UploadImageAsync(filePath);
                        if (uploadedFileName != null)
                        {
                            food.ImagePath = uploadedFileName;
                            // Щоб Avalonia побачила зміни в об'єкті, ми "пересмикуємо" колекцію
                            var index = NewRestaurantMenu.IndexOf(food);
                            NewRestaurantMenu[index] = new FoodModel { Name = food.Name, Price = food.Price, Category = food.Category, Description = food.Description, ImagePath = uploadedFileName, Quantity = 1 };
                            RestMessage = "Картинку страви успішно завантажено!";
                        }
                        else { RestMessageColor = "#EF4444"; RestMessage = "Помилка завантаження картинки."; }
                    }
                }
            });

            // імпорт
            ImportJsonCommand = new RelayCommand(async o =>
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Оберіть файл ресторану (.json)",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { new FilePickerFileType("JSON файли") { Patterns = new[] { "*.json" } } }
                    });

                    if (files.Count > 0)
                    {
                        try
                        {
                            // Читаємо обраний файл
                            await using var stream = await files[0].OpenReadAsync();
                            using var reader = new StreamReader(stream);
                            string jsonContent = await reader.ReadToEndAsync();

                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var imported = JsonSerializer.Deserialize<RestaurantModel>(jsonContent, options);

                            if (imported != null)
                            {
                                // Заповнюємо форму даними з файлу
                                RestName = imported.Name;
                                RestCategory = imported.Category;
                                RestDeliveryTime = imported.DeliveryTime;
                                RestDescription = imported.Description;
                                RestImagePath = imported.ImagePath;
                                RestAddress = imported.Address;

                                OnPropertyChanged(nameof(RestName));
                                OnPropertyChanged(nameof(RestCategory));
                                OnPropertyChanged(nameof(RestDeliveryTime));
                                OnPropertyChanged(nameof(RestAddress));

                                // Заповнюємо меню
                                NewRestaurantMenu.Clear();
                                if (imported.Menu != null)
                                {
                                    foreach (var item in imported.Menu)
                                    {
                                        NewRestaurantMenu.Add(item);
                                    }
                                }

                                RestMessageColor = "#10B981";
                                RestMessage = "Файл успішно завантажено! Перевірте дані нижче.";
                            }
                        }
                        catch (Exception)
                        {
                            RestMessageColor = "#EF4444";
                            RestMessage = "Помилка: Невірний формат JSON файлу.";
                        }
                    }
                }
            });
            SaveRestaurantCommand = new RelayCommand(async o =>
            {
                // Валідація ресторану
                if (string.IsNullOrWhiteSpace(RestName) || string.IsNullOrWhiteSpace(RestAddress) ||
                    string.IsNullOrWhiteSpace(RestCategory) || string.IsNullOrWhiteSpace(RestDeliveryTime) ||
                    string.IsNullOrWhiteSpace(RestDescription) || string.IsNullOrWhiteSpace(RestImagePath))
                {
                    RestMessageColor = "#EF4444";
                    RestMessage = "Помилка: Усі поля закладу повинні бути заповнені!";
                    return;
                }
                // Валідація часу доставки
                string deliveryPattern = @"^\d+(-\d+)?\s*(хв|хв\.|год|год\.)$";
                if (!Regex.IsMatch(RestDeliveryTime ?? "", deliveryPattern, RegexOptions.IgnoreCase))
                {
                    RestMessageColor = "#EF4444";
                    RestMessage = "Некоректний формат часу доставки! Використовуйте наприклад: '30-40 хв', '15 хв' або '1 год'.";
                    return;
                }
                // Валідація меню
                if (NewRestaurantMenu.Count == 0)
                {
                    RestMessageColor = "#EF4444"; RestMessage = "Помилка: Додайте хоча б одну страву в меню!"; return;
                }

                foreach (var food in NewRestaurantMenu)
                {
                    if (string.IsNullOrWhiteSpace(food.Name) || string.IsNullOrWhiteSpace(food.Category) ||
                        string.IsNullOrWhiteSpace(food.Description) || string.IsNullOrWhiteSpace(food.ImagePath))
                    {
                        RestMessageColor = "#EF4444"; RestMessage = "Помилка: Заповніть усі поля для кожної страви!"; return;
                    }
                    if (food.Price <= 0)
                    {
                        RestMessageColor = "#EF4444"; RestMessage = $"Помилка: Ціна для '{food.Name}' повинна бути більше 0!"; return;
                    }
                }

                var newRest = new RestaurantModel
                {
                    Id = _currentRestaurantId,
                    OwnerId = User.Id,
                    OwnerLogin = User.Login,
                    Name = RestName,
                    Category = RestCategory,
                    DeliveryTime = RestDeliveryTime,
                    Description = RestDescription,
                    ImagePath = RestImagePath,
                    Address = RestAddress,
                    Rating = 0.0,
                    Menu = NewRestaurantMenu.ToList()
                };

                RestMessageColor = "#10B981"; RestMessage = "Збереження даних на сервер...";

                bool success;
                if (_currentRestaurantId > 0)
                {
                    // Якщо ресторан вже є - оновлюємо
                    success = await _apiService.UpdateRestaurantAsync(newRest);
                }
                else
                {
                    // Якщо ресторана немає - створюємо
                    success = await _apiService.CreateRestaurantAsync(newRest);
                }

                if (success)
                {
                    RestMessage = _currentRestaurantId > 0 ? "Дані закладу успішно оновлено!" : "Заклад успішно створено!";
                    await Task.Delay(1500); ActiveTab = "Profile";
                }
                else { RestMessageColor = "#EF4444"; RestMessage = "Помилка збереження! Перевірте сервер."; }
            });
        }

        private async Task<string> PickImageFileAsync()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Оберіть зображення",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("Зображення") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp" } } }
                });
                if (files.Count > 0) return files[0].Path.LocalPath;
            }
            return null;
        }
        private async void LoadProfile()
        {
            User = await _apiService.GetProfileAsync() ?? new UserModel { Login = "Гість" };
        }
    }
}