using System.Windows.Input;
using ClientAppe.Models;
using ClientAppe.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private bool _isApplicationFormVisible;
        public bool IsApplicationFormVisible
        {
            get => _isApplicationFormVisible;
            set { _isApplicationFormVisible = value; OnPropertyChanged(); }
        }

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

        public ICommand EditProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowProfileTabCommand { get; }
        public ICommand ShowApplicationTabCommand { get; }
        public ICommand SubmitApplicationCommand { get; }

        public ProfileViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadProfile();

            // Відкриваємо вікно редагування
            EditProfileCommand = new RelayCommand(o => {
                EditLogin = User.Login;
                EditPhone = User.Phone;
                EditPassword = "";
                IsEditing = true;
            });

            CancelEditCommand = new RelayCommand(o => { IsEditing = false; });

            SaveProfileCommand = new RelayCommand(async o => {
                ErrorMessage = "";

                string phonePattern = @"^\+?[0-9]{10,12}$";
                if (!Regex.IsMatch(EditPhone, phonePattern))
                {
                    ErrorMessage = "Некоректний номер (напр. +380954123456)";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(EditPassword))
                {
                    string passPattern = @"^[a-zA-Z0-9]{8,16}$";
                    if (!Regex.IsMatch(EditPassword, passPattern))
                    {
                        ErrorMessage = "Пароль: 8-16 символів (A-Z, 0-9)";
                        return;
                    }
                }

                var updatedUser = new UserModel
                {
                    Id = User.Id,
                    Login = EditLogin,
                    Phone = EditPhone,
                    Password = EditPassword,
                    Token = ApiService.CurrentUser.Token
                };

                bool success = await _apiService.UpdateProfileAsync(updatedUser);
                if (success)
                {
                    User.Login = EditLogin;
                    User.Phone = EditPhone;
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(UserInitial));
                    IsEditing = false;
                }
            });

            LogoutCommand = new RelayCommand(o =>
            {
                ApiService.CurrentUser = null;
                _mainViewModel.NavigateTo(new AuthViewModel(_mainViewModel), false);
            });

            ShowProfileTabCommand = new RelayCommand(o =>
            {
                IsApplicationFormVisible = false;
                AppMessage = "";
            });

            ShowApplicationTabCommand = new RelayCommand(o =>
            {
                IsApplicationFormVisible = true;
                AppFullName = "";
                AppPhone = User?.Phone;
                AppEmail = User?.Email;
            });

            SubmitApplicationCommand = new RelayCommand(async o =>
            {
                // 1. Перевірка на порожні поля
                if (string.IsNullOrWhiteSpace(AppFullName) || string.IsNullOrWhiteSpace(AppPhone) || string.IsNullOrWhiteSpace(AppEmail))
                {
                    AppMessageColor = "#EF4444";
                    AppMessage = "Будь ласка, заповніть усі обов'язкові поля!";
                    return;
                }

                string phonePattern = @"^\+?[0-9]{10,12}$";
                if (!Regex.IsMatch(AppPhone, phonePattern))
                {
                    AppMessageColor = "#EF4444";
                    AppMessage = "Некоректний номер телефону (напр. +380954123456)";
                    return;
                }

                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(AppEmail, emailPattern))
                {
                    AppMessageColor = "#EF4444";
                    AppMessage = "Некоректний формат електронної пошти (напр. example@gmail.com)";
                    return;
                }

                AppMessageColor = "#10B981";
                AppMessage = "Відправка...";

                bool success = await _apiService.SubmitPartnerApplicationAsync(AppFullName, AppPhone, AppEmail, AppDescription);

                if (success)
                {
                    AppMessage = "Заявку успішно відправлено! Очікуйте на рішення.";

                    await Task.Delay(1500);
                    IsApplicationFormVisible = false;
                    AppMessage = "";
                }
                else
                {
                    AppMessageColor = "#EF4444";
                    AppMessage = "Помилка відправки. Можливо, ваша заявка вже знаходиться на розгляді.";
                }
            });
        }

        private async void LoadProfile()
        {
            User = await _apiService.GetProfileAsync() ?? new UserModel { Login = "Гість" };
        }
    }
}