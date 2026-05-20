using ClientAppe.Models;
using ClientAppe.Services;
using ClientAppe.Views;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ClientAppe.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly CartService _cartService = new CartService();
        private readonly Stack<ViewModelBase> _history = new Stack<ViewModelBase>();
        private CartWindow _openedCartWindow;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        private bool _isSuccessMessageVisible;
        public bool IsSuccessMessageVisible
        {
            get => _isSuccessMessageVisible;
            set { _isSuccessMessageVisible = value; OnPropertyChanged(); }
        }

        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToAuthCommand { get; }
        public ICommand NavigateToRestaurantsCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand CloseSuccessMessageCommand { get; }

        public MainViewModel()
        {
            NavigateToHomeCommand = new RelayCommand(o => NavigateTo(new HomeViewModel(this)));
            NavigateToAuthCommand = new RelayCommand(o => NavigateTo(new AuthViewModel(this)));

            NavigateToRestaurantsCommand = new RelayCommand(o => NavigateTo(new RestaurantsViewModel(this)));

            NavigateToCartCommand = new RelayCommand(o => OpenCartWindow());

            NavigateToProfileCommand = new RelayCommand(o => NavigateTo(new ProfileViewModel(this)));
            NavigateToOrdersCommand = new RelayCommand(o => NavigateTo(new OrdersViewModel()));

            GoBackCommand = new RelayCommand(o => GoBack());
            CloseSuccessMessageCommand = new RelayCommand(o => IsSuccessMessageVisible = false);

            NavigateToRestaurantsCommand = new RelayCommand(param =>
            {
                string category = param as string ?? "Всі заклади";
                NavigateTo(new RestaurantsViewModel(this, category));
            });
            // Стартуємо з форми авторизації
            NavigateTo(new AuthViewModel(this), false);
        }

        // МЕТОДИ НАВІГАЦІЇ (ГОЛОВНЕ ВІКНО)
        public void NavigateTo(ViewModelBase nextViewModel, bool saveToHistory = true)
        {
            if (nextViewModel == null) return;

            // Зберігаємо поточну сторінку в стек перед переходом
            if (saveToHistory && CurrentViewModel != null)
            {
                _history.Push(CurrentViewModel);
            }

            CurrentViewModel = nextViewModel;
        }

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                // Дістаємо попередню сторінку зі стеку
                CurrentViewModel = _history.Pop();
            }
        }

        // Спеціальний перехід до деталей ресторану
        public void NavigateToDetails(RestaurantModel restaurant)
        {
            if (restaurant != null)
            {
                NavigateTo(new RestaurantDetailsViewModel(this, restaurant, _cartService)); // Додав _cartService, якщо раптом ти з меню щось додаєш в кошик
            }
        }

        // ЛОГІКА МОДУЛЬНОГО ВІКНА КОШИКА
        public void OpenCartWindow()
        {
            // Перевірка, щоб не відкрити кошик двічі
            if (_openedCartWindow != null)
            {
                _openedCartWindow.Activate();
                return;
            }

            var cartWindowVM = new CartWindowViewModel(_cartService, OnOrderSuccess);

            _openedCartWindow = new CartWindow
            {
                DataContext = cartWindowVM
            };

            cartWindowVM.RequestClose = () => _openedCartWindow?.Close();

            _openedCartWindow.Closed += (s, e) => _openedCartWindow = null;
            _openedCartWindow.Show();
        }

        private void OnOrderSuccess()
        {
            IsSuccessMessageVisible = true;

            // Повертаємо головне вікно на головну сторінку
            NavigateTo(new HomeViewModel(this), false);

            _history.Clear();
        }

    }
}