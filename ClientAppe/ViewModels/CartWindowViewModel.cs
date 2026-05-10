using ClientAppe.Models;
using ClientAppe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientAppe.ViewModels
{
    public class CartWindowViewModel : ViewModelBase
    {
        private readonly CartService _cartService;
        private readonly ApiService _apiService = new ApiService(); // Підключаємо мережу
        private readonly Stack<ViewModelBase> _history = new Stack<ViewModelBase>();
        private readonly Action _onSuccessCallback;

        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand GoBackCommand { get; }
        public ICommand ConfirmOrderCommand { get; }

        public Action RequestClose { get; set; }

        public CartWindowViewModel(CartService cartService, Action onSuccess)
        {
            _cartService = cartService;
            _onSuccessCallback = onSuccess;

            CurrentView = new CartViewModel(_cartService, this);

            GoBackCommand = new RelayCommand(o => {
                if (_history.Count > 0) CurrentView = _history.Pop();
                else RequestClose?.Invoke();
            });

            // Команда для фінальної кнопки (буде викликатися з CheckoutViewModel)
            ConfirmOrderCommand = new RelayCommand(async address => await ConfirmOrderAsync(address as string));
        }

        // Метод для переходу на сторінку оплати
        public void NavigateToCheckout()
        {
            _history.Push(CurrentView);
            CurrentView = new CheckoutViewModel(_cartService, this);
        }

        // Відправка замовлення на сервер
        public async Task ConfirmOrderAsync(string deliveryAddress)
        {
            if (_cartService.Items.Count == 0) return;

            var newOrder = new OrderModel
            {
                RestaurantId = _cartService.CurrentRestaurantId,

                OrderedItems = _cartService.Items.ToList(),

                TotalPrice = _cartService.GetTotal(),

                DeliveryAddress = string.IsNullOrWhiteSpace(deliveryAddress) ? "Самовивіз" : deliveryAddress,

            };

            bool success = await _apiService.CreateOrderAsync(newOrder);

            if (success)
            {
                _cartService.Items.Clear();
                _cartService.CurrentRestaurantId = 0;

                RequestClose?.Invoke();

                _onSuccessCallback?.Invoke();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Помилка: Сервер відхилив замовлення.");
            }
        }
    }
}