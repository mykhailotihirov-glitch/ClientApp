using System.Collections.Generic;
using System.Linq;

namespace ClientAppe.Models
{
    public class CartModel
    {
        public List<FoodModel> Items { get; set; } = new List<FoodModel>();
        public decimal? TotalPrice => Items.Sum(item => item.Price);
    }
}