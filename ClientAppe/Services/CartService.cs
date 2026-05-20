using System;
using System.Collections.ObjectModel;
using System.Linq;
using ClientAppe.Models;

namespace ClientAppe.Services
{
    public class CartService
    {
        public ObservableCollection<FoodModel> Items { get; } = new ObservableCollection<FoodModel>();
        public int CurrentRestaurantId { get; set; }
        public event Action CartUpdated;

        public void AddToCart(FoodModel food)
        {
            var existingItem = Items.FirstOrDefault(x => x.Name == food.Name);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                food.Quantity = 1;
                Items.Add(food);
            }

            CartUpdated?.Invoke();
        }

        public void RemoveFromCart(FoodModel food)
        {
            var existingItem = Items.FirstOrDefault(x => x.Name == food.Name);

            if (existingItem != null)
            {
                existingItem.Quantity--;

                if (existingItem.Quantity <= 0)
                {
                    existingItem.Quantity = 0;
                    Items.Remove(existingItem);
                }
            }

            CartUpdated?.Invoke();
        }

        public void ClearCart()
        {
            Items.Clear();

            CartUpdated?.Invoke();
        }

        public decimal? GetTotal()
        {
            return Items.Sum(x => x.Price * (decimal)x.Quantity);
        }
    }
}