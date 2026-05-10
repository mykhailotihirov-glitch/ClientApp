using NUnit.Framework;
using ClientAppe.Models;
using ClientAppe.Services;
using System.Linq;

namespace ClientAppe.Tests
{
    [TestFixture]
    public class CartServiceTests
    {
        private CartService _cartService;

        [SetUp]
        public void Setup()
        {
            _cartService = new CartService();
        }

        // Unit-тести для CartService (додавання, сума, видалення)

        [Test]
        public void AddToCart_NewItem_ShouldAddItemAndSetQuantityToOne()
        {
            var food = new FoodModel { Name = "Піца Маргарита", Price = 250m };
            _cartService.AddToCart(food);

            Assert.That(_cartService.Items.Count, Is.EqualTo(1));
            Assert.That(_cartService.Items.First().Name, Is.EqualTo("Піца Маргарита"));
            Assert.That(_cartService.Items.First().Quantity, Is.EqualTo(1));
        }

        [Test]
        public void AddToCart_ExistingItem_ShouldIncreaseQuantity()
        {
            var food1 = new FoodModel { Name = "Бургер", Price = 150m };
            var food2 = new FoodModel { Name = "Бургер", Price = 150m };

            _cartService.AddToCart(food1);
            _cartService.AddToCart(food2);

            Assert.That(_cartService.Items.Count, Is.EqualTo(1));
            Assert.That(_cartService.Items.First().Quantity, Is.EqualTo(2));
        }

        [Test]
        public void GetTotal_MultipleItems_ShouldReturnCorrectSum()
        {
            var food1 = new FoodModel { Name = "Суші", Price = 300m };
            var food2 = new FoodModel { Name = "Кола", Price = 50m };

            _cartService.AddToCart(food1);
            _cartService.AddToCart(food2);
            _cartService.AddToCart(food2);

            var totalSum = _cartService.GetTotal();

            Assert.That(totalSum, Is.EqualTo(400m));
        }

        [Test]
        public void RemoveFromCart_ShouldDecreaseQuantityOrRemoveIfZero()
        {
            // Arrange
            var food = new FoodModel { Name = "Салат", Price = 100m };
            _cartService.AddToCart(food);
            _cartService.AddToCart(food);
            _cartService.RemoveFromCart(food);

            Assert.That(_cartService.Items.Count, Is.EqualTo(1));
            Assert.That(_cartService.Items.First().Quantity, Is.EqualTo(1));

            _cartService.RemoveFromCart(food);

            Assert.That(_cartService.Items.Count, Is.EqualTo(0));
            Assert.That(_cartService.GetTotal(), Is.EqualTo(0m));
        }

        // Інтеграційний тест (Взаємодія CartService та OrderModel)
        [Test]
        public void CreateOrderData_FromCart_ShouldMapCorrectly()
        {
            // Arrange: Наповнюємо кошик і задаємо ID ресторану
            _cartService.CurrentRestaurantId = 5;
            _cartService.AddToCart(new FoodModel { Name = "Суп", Price = 120m });
            _cartService.AddToCart(new FoodModel { Name = "Хліб", Price = 20m });

            // Імітуємо взаємодію сервісу з моделлю
            var order = new OrderModel
            {
                RestaurantId = _cartService.CurrentRestaurantId,
                TotalPrice = _cartService.GetTotal(),
                OrderedItems = _cartService.Items.ToList(),
                Status = "New"
            };

            Assert.That(order.RestaurantId, Is.EqualTo(5));
            Assert.That(order.TotalPrice, Is.EqualTo(140.0)); // Перевіряємо суму
            Assert.That(order.OrderedItems.Count, Is.EqualTo(2)); // Перевіряємо список страв
            Assert.That(order.OrderedItems[0].Name, Is.EqualTo("Суп"));
        }
    }
}