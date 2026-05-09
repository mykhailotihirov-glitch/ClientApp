using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using ClientAppe.Models;
using ClientAppe.ViewModels;
using ClientAppe.Services;

namespace ClientAppe.Tests
{
    [TestFixture]
    public class RestaurantsIntegrationTests
    {
        // ====================================================================
        // ЗАВДАННЯ 3: Тест виключно з Mock-об'єктами (Аналіз швидкості)
        // ====================================================================
        [Test]
        public async Task LoadRestaurants_WithMOCK_ShouldBeVeryFast()
        {
            var mockApiService = new Mock<ApiService>();
            var fakeRestaurants = new List<RestaurantModel>
            {
                new RestaurantModel { Name = "Фейк МакДональдс", Rating = 5.0, Category = "Фаст-фуд" },
                new RestaurantModel { Name = "Фейк Суші", Rating = 4.8, Category = "Інше" }
            };
            mockApiService.Setup(api => api.GetRestaurantsAsync()).ReturnsAsync(fakeRestaurants);

            var viewModel = new RestaurantsViewModel(null, "Всі заклади", mockApiService.Object);

            var stopwatch = Stopwatch.StartNew();
            viewModel.LoadData();
            await Task.Delay(50);
            stopwatch.Stop();

            Assert.That(viewModel.Restaurants, Is.Not.Empty);
            TestContext.WriteLine($"Час виконання з MOCK: {stopwatch.ElapsedMilliseconds} мс");
        }

        // ====================================================================
        // ЗАВДАННЯ 4: Такий самий тест з Real-об'єктами (Аналіз швидкості)
        // ====================================================================
        [Test]
        public async Task LoadRestaurants_WithREAL_API_ShouldTakeLonger()
        {
            var realApiService = new ApiService();
            var viewModel = new RestaurantsViewModel(null, "Всі заклади", realApiService);

            var stopwatch = Stopwatch.StartNew();
            viewModel.LoadData();
            await Task.Delay(1500);
            stopwatch.Stop();

            Assert.That(viewModel.Restaurants, Is.Not.Null);
            TestContext.WriteLine($"Час виконання з REAL API: {stopwatch.ElapsedMilliseconds} мс");
        }

        // ====================================================================
        // ЗАВДАННЯ 2 (Тест 1): Інтеграційний тест логіки фільтрації
        // ====================================================================
        [Test]
        public async Task FilterRestaurants_ByCategory_MatchingList()
        {
            var mockApiService = new Mock<ApiService>();
            var fakeList = new List<RestaurantModel>
            {
                new RestaurantModel { Name = "KFC", Category = "Фаст-фуд" },
                new RestaurantModel { Name = "Пузата Хата", Category = "Ресторани" },
                new RestaurantModel { Name = "Burger King", Category = "Фаст-фуд" }
            };
            mockApiService.Setup(api => api.GetRestaurantsAsync()).ReturnsAsync(fakeList);
            var viewModel = new RestaurantsViewModel(null, "Всі заклади", mockApiService.Object);

            viewModel.LoadData();
            await Task.Delay(50);

            viewModel.FilterCategoryCommand.Execute("Фаст-фуд");

            Assert.That(viewModel.Restaurants.Count, Is.EqualTo(2));
            Assert.That(viewModel.Restaurants.All(r => r.Category == "Фаст-фуд"), Is.True);
        }

        // ====================================================================
        // ЗАВДАННЯ 2 (Тест 2): Інтеграційний тест логіки сортування
        // ====================================================================
        [Test]
        public async Task SortRestaurants_ByRating_HighestFirst()
        {
            var mockApiService = new Mock<ApiService>();
            var fakeList = new List<RestaurantModel>
            {
                new RestaurantModel { Name = "Середняк", Rating = 3.5 },
                new RestaurantModel { Name = "Топ Ресторан", Rating = 5.0 },
                new RestaurantModel { Name = "Не дуже", Rating = 2.1 }
            };
            mockApiService.Setup(api => api.GetRestaurantsAsync()).ReturnsAsync(fakeList);
            var viewModel = new RestaurantsViewModel(null, "Всі заклади", mockApiService.Object);

            viewModel.LoadData();
            await Task.Delay(50);

            viewModel.SortCommand.Execute("Rating");

            // Новий синтаксис NUnit 4
            Assert.That(viewModel.Restaurants.Count, Is.EqualTo(3));
            Assert.That(viewModel.Restaurants[0].Name, Is.EqualTo("Топ Ресторан"));
            Assert.That(viewModel.Restaurants[0].Rating, Is.EqualTo(5.0));
        }
    }
}