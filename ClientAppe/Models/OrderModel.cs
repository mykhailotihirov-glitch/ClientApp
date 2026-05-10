using System;
using System.Collections.Generic;

namespace ClientAppe.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; }
        public string RestaurantName { get; set; }
        public string OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int RestaurantId { get; set; }
        public List<FoodModel> OrderedItems { get; set; }
        public string ItemsSummary { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string DeliveryAddress { get; set; }
    }
}