using System.Collections.Generic;

namespace ClientAppe.Models
{
    public class RestaurantModel
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OwnerLogin { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
        public string DeliveryTime { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Avalonia.Media.Imaging.Bitmap ImageBitmap { get; set; }
        public string Distance { get; set; }
        public string Description { get; set; }

        public List<FoodModel> Menu { get; set; }
    }
}