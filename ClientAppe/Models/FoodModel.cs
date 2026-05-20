using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientAppe.Models
{
    public class FoodModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Avalonia.Media.Imaging.Bitmap ImageBitmap { get; set; }

        private decimal? _price;
        public decimal? Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalItemPrice));
                }
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsInCart));
                    OnPropertyChanged(nameof(IsNotInCart));
                    OnPropertyChanged(nameof(TotalItemPrice));
                }
            }
        }

        public decimal TotalItemPrice
        {
            get
            {
                decimal safePrice = Price ?? 0m;
                return safePrice * Quantity > 0 ? safePrice * Quantity : safePrice;
            }
        }

        public bool IsInCart => Quantity > 0;
        public bool IsNotInCart => Quantity == 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}