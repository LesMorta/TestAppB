using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TestAppB.Models;
using TestAppB.Services;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlantPage : ContentPage
    {
        private List<Plant> plants;
        private readonly string[] tips = new string[]
{
            "Не забувайте, що різні рослини вимагають різного режиму поливу. Перевіряйте ґрунт перед кожним поливом!",
            "Більшість кімнатних рослин віддають перевагу яскравому непрямому світлу. Уникайте розміщувати їх під прямими сонячними променями.",
            "Використовуйте воду кімнатної температури для поливу. Холодна вода може викликати шок у рослин.",
            "Не ставте рослини біля обігрівачів або кондиціонерів - різкі перепади температури шкідливі для них.",
            "Регулярно очищайте листя рослин від пилу вологою тканиною. Це допомагає їм краще дихати.",
            "Якщо у рослини жовтіють листя, це може бути ознакою надмірного поливу. Дайте ґрунту просохнути перед наступним поливом."
};

        public PlantPage()
        {
            InitializeComponent();
            LoadData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadData();
            UpdateLastAchievement();

            // Оновлюємо досягнення за вхід у додаток
            int appOpenCount = Preferences.Get("app_open_count", 0) + 1;
            Preferences.Set("app_open_count", appOpenCount);

            // Перевіряємо досягнення за відкриття додатку
            if (appOpenCount == 5)
            {
                AchievementService.UnlockAchievement("open_app_5");
                UpdateLastAchievement(); // Оновлюємо відображення, якщо досягнення отримано
            }
        }

        private void LoadData()
        {
            // Завантажуємо рослини зі сховища
            plants = LoadPlants();
            plantsCollectionView.ItemsSource = plants;

            // Оновлюємо статистику
            totalPlantsLabel.Text = plants.Count.ToString();
            wateredPlantsLabel.Text = plants.Count(p => p.IsWatered).ToString();

            // Визначаємо, чи потрібно показувати порожній список
            emptyPlantsLayout.IsVisible = plants.Count == 0;
            plantsCollectionView.IsVisible = plants.Count > 0;

            // Оновлюємо дату та пораду дня
            dateTimeLabel.Text = $"{DateTime.Now:dddd, d MMMM} • Чудовий день для виращування!";
            tipOfDayLabel.Text = GetRandomTip();

            // Оновлюємо кількість розблокованих досягнень
            var achievements = AchievementService.GetAchievements();
            achievementsLabel.Text = achievements.Count(a => a.IsUnlocked).ToString();
        }

        private void UpdateLastAchievement()
        {
            // Отримуємо останнє розблоковане досягнення
            var lastAchievement = AchievementService.GetLastUnlockedAchievement();

            if (lastAchievement != null)
            {
                // Оновлюємо текстові поля
                lastAchievementTitle.Text = lastAchievement.Title;
                lastAchievementDescription.Text = lastAchievement.Description;

                // Зображення не змінюємо! Воно фіксоване в XAML як "achievements.png"
                // lastAchievementIcon.Source = lastAchievement.Icon; -- цей рядок видалено

                // Форматуємо час отримання
                if (lastAchievement.UnlockTime > DateTime.MinValue)
                {
                    TimeSpan timeAgo = DateTime.Now - lastAchievement.UnlockTime;
                    if (timeAgo.TotalDays < 1)
                    {
                        if (timeAgo.TotalHours < 1)
                            lastAchievementTime.Text = "Отримано щойно";
                        else
                            lastAchievementTime.Text = $"Отримано {(int)timeAgo.TotalHours} год. тому";
                    }
                    else if (timeAgo.TotalDays < 30)
                    {
                        lastAchievementTime.Text = $"Отримано {(int)timeAgo.TotalDays} дн. тому";
                    }
                    else
                    {
                        lastAchievementTime.Text = $"Отримано {lastAchievement.UnlockTime:dd.MM.yyyy}";
                    }
                }
                else
                {
                    lastAchievementTime.Text = "Отримано раніше";
                }
            }
            else
            {
                // Якщо немає досягнень, встановлюємо дефолтні значення
                lastAchievementTitle.Text = "Немає досягнень";
                lastAchievementDescription.Text = "Доглядайте за рослинами, щоб отримати перше досягнення!";
                lastAchievementTime.Text = "";
            }
        }

        private List<Plant> LoadPlants()
        {
            try
            {
                // Завантажуємо збережені рослини
                string savedPlantsJson = Preferences.Get("saved_plants", string.Empty);
                if (string.IsNullOrEmpty(savedPlantsJson))
                {
                    return new List<Plant>(); // Повертаємо порожній список якщо немає збережених рослин
                }

                var loadedPlants = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Plant>>(savedPlantsJson);

                // Перевіряємо та ініціалізуємо поля, які можуть бути null після десеріалізації
                foreach (var plant in loadedPlants)
                {
                    if (plant.Notes == null)
                        plant.Notes = new List<PlantNote>();
                }

                return loadedPlants;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження рослин: {ex.Message}");
                return new List<Plant>();
            }
        }

        private void SavePlants()
        {
            try
            {
                if (plants == null || plants.Count == 0)
                {
                    // Якщо список порожній, зберігаємо порожній рядок
                    Preferences.Remove("saved_plants");
                    return;
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(plants);
                Preferences.Set("saved_plants", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка збереження рослин: {ex.Message}");
            }
        }

        private string GetRandomTip()
        {
            Random random = new Random();
            return tips[random.Next(tips.Length)];
        }

        private async void AddPlant_Clicked(object sender, EventArgs e)
        {
            // Діалог для введення назви рослини
            string plantName = await DisplayPromptAsync("Нова рослина",
                "Введіть назву вашої рослини",
                "Додати", "Скасувати",
                "Наприклад: Фікус, Орхідея...");

            if (!string.IsNullOrWhiteSpace(plantName))
            {
                // Створюємо нову рослину
                Plant newPlant = new Plant
                {
                    Name = plantName,
                    IsWatered = false,
                    LastWatered = DateTime.MinValue
                };

                // Додаємо до колекції
                plants.Add(newPlant);
                SavePlants();
                LoadData();

                // Перевіряємо досягнення за кількістю рослин
                CheckPlantCountAchievements();
            }
        }

        private async void WaterPlant_Clicked(object sender, EventArgs e)
        {
            // Отримуємо назву рослини з параметра команди
            string plantName = (string)((Button)sender).CommandParameter;

            // Знаходимо рослину в колекції
            Plant plant = plants.FirstOrDefault(p => p.Name == plantName);
            if (plant != null)
            {
                // Оновлюємо статус поливу
                plant.IsWatered = true;
                plant.LastWatered = DateTime.Now;
                SavePlants();
                LoadData();

                // Анімація успішного поливу
                await AnimateWateringSuccess(sender);

                // Перевіряємо досягнення поливу
                CheckWateringAchievements();
            }
        }

        private async Task AnimateWateringSuccess(object sender)
        {
            if (sender is Button button)
            {
                // Анімація для кнопки поливу
                await button.ScaleTo(1.2, 150);
                await button.ScaleTo(1.0, 150);
            }
        }

        private async void PlantDetails_Tapped(object sender, EventArgs e)
        {
            // Отримуємо назву рослини з параметра команди
            var tapGesture = ((Frame)sender).GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            if (tapGesture == null) return;

            string plantName = (string)tapGesture.CommandParameter;
            if (string.IsNullOrEmpty(plantName)) return;

            // Знаходимо рослину в колекції
            Plant plant = plants.FirstOrDefault(p => p.Name == plantName);
            if (plant == null) return;

            // Показуємо контекстне меню
            string action = await DisplayActionSheet(
                $"Дії з рослиною: {plant.Name}",
                "Скасувати",
                null,
                "Відобразити рослину",
                "Перейменувати",
                "Додати запис",
                "Видалити");

            switch (action)
            {
                case "Відобразити рослину":
                    await Navigation.PushModalAsync(new PlantDisplayPage(plant, plants, SavePlants));
                    break;

                case "Перейменувати":
                    await RenamePlant(plant);
                    break;

                case "Додати запис":
                    await Navigation.PushModalAsync(new PlantNotesPage(plant, SavePlants));
                    break;

                case "Видалити":
                    await DeletePlant(plant);
                    break;
            }
        }

        private async Task RenamePlant(Plant plant)
        {
            // Діалог для перейменування рослини
            string newName = await DisplayPromptAsync(
                "Перейменування",
                "Введіть нову назву для рослини",
                "Зберегти",
                "Скасувати",
                initialValue: plant.Name);

            if (!string.IsNullOrWhiteSpace(newName) && newName != plant.Name)
            {
                // Перевіряємо, чи немає вже рослини з такою назвою
                if (plants.Any(p => p.Name == newName))
                {
                    await DisplayAlert("Помилка", "Рослина з такою назвою вже існує", "OK");
                    return;
                }

                // Запам'ятовуємо стару назву для перевірки досягнення
                string oldName = plant.Name;

                // Оновлюємо назву
                plant.Name = newName;

                // Зберігаємо зміни
                SavePlants();
                LoadData();

                // Розблоковуємо досягнення за перейменування рослини
                AchievementService.UnlockAchievement("rename_plant");
                UpdateLastAchievement();

                await DisplayAlert("Готово", $"Рослину перейменовано з \"{oldName}\" на \"{newName}\"", "OK");
            }
        }
        private async Task DeletePlant(Plant plant)
        {
            // Запитуємо підтвердження видалення
            bool confirm = await DisplayAlert(
                "Видалення рослини",
                $"Ви впевнені, що хочете видалити рослину \"{plant.Name}\"?",
                "Так, видалити",
                "Скасувати");

            if (confirm)
            {
                // Видаляємо рослину з колекції
                plants.Remove(plant);

                // Зберігаємо зміни
                SavePlants();
                LoadData();

                // Розблоковуємо досягнення за видалення рослини
                AchievementService.UnlockAchievement("delete_plant");
                UpdateLastAchievement();

                await DisplayAlert("Готово", $"Рослину \"{plant.Name}\" видалено", "OK");
            }
        }
        private void CheckWateringAchievements()
        {
            // Рахуємо загальну кількість поливів
            int totalWaterings = Preferences.Get("total_waterings", 0) + 1;
            Preferences.Set("total_waterings", totalWaterings);

            // Перевіряємо досягнення за кількістю поливів
            if (totalWaterings == 1)
                AchievementService.UnlockAchievement("first_water");
            else if (totalWaterings == 10)
                AchievementService.UnlockAchievement("ten_waters");
            else if (totalWaterings == 50)
                AchievementService.UnlockAchievement("fifty_waters");
            else if (totalWaterings == 100)
                AchievementService.UnlockAchievement("hundred_waters");
            else if (totalWaterings == 500)
                AchievementService.UnlockAchievement("five_hundred_waters");

            // Перевіряємо досягнення за часом доби
            int hour = DateTime.Now.Hour;
            if (hour >= 6 && hour < 10)
                AchievementService.UnlockAchievement("morning_water");
            else if (hour >= 18 && hour < 22)
                AchievementService.UnlockAchievement("evening_water");
            else if (hour >= 23 || hour < 5)
                AchievementService.UnlockAchievement("night_owl");

            // Перевіряємо досягнення за полив усіх рослин
            if (plants.Count > 0 && plants.All(p => p.IsWatered))
                AchievementService.UnlockAchievement("water_all");

            // Перевіряємо сезонні досягнення
            int month = DateTime.Now.Month;
            if (month >= 3 && month <= 5) // Весна
                AchievementService.UnlockAchievement("spring_water");
            else if (month >= 6 && month <= 8) // Літо
                AchievementService.UnlockAchievement("summer_water");
            else if (month >= 9 && month <= 11) // Осінь
                AchievementService.UnlockAchievement("autumn_water");
            else // Зима (грудень, січень, лютий)
                AchievementService.UnlockAchievement("winter_water");

            // Перевіряємо досягнення за вихідні
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                bool saturdayWatered = Preferences.Get("saturday_watered", false);
                bool sundayWatered = Preferences.Get("sunday_watered", false);

                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    Preferences.Set("saturday_watered", true);
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    Preferences.Set("sunday_watered", true);

                // Якщо в ці вихідні поливали в обидва дні
                if ((DateTime.Now.DayOfWeek == DayOfWeek.Saturday && sundayWatered) ||
                    (DateTime.Now.DayOfWeek == DayOfWeek.Sunday && saturdayWatered))
                {
                    AchievementService.UnlockAchievement("weekend_care");
                }

                // Скидаємо статус на початку нового тижня
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    Preferences.Set("saturday_watered", false);
                    Preferences.Set("sunday_watered", false);
                }
            }

            // Оновлюємо відображення досягнень
            UpdateLastAchievement();
        }

        private void CheckPlantCountAchievements()
        {
            // Перевіряємо досягнення за кількістю рослин
            int plantCount = plants.Count;

            if (plantCount >= 3)
                AchievementService.UnlockAchievement("three_plants");
            if (plantCount >= 10)
                AchievementService.UnlockAchievement("ten_plants");
            if (plantCount >= 20)
                AchievementService.UnlockAchievement("twenty_plants");

            // Оновлюємо відображення досягнень
            UpdateLastAchievement();
        }

        private void ViewAllAchievements_Clicked(object sender, EventArgs e)
        {
            // Переходимо на сторінку досягнень
            ((TabbedPage)Parent).CurrentPage = ((TabbedPage)Parent).Children[1]; // Припускається, що AchievementsPage - другий таб
        }

        private void ViewNews_Clicked(object sender, EventArgs e)
        {
            // Переходимо на сторінку новин
            ((TabbedPage)Parent).CurrentPage = ((TabbedPage)Parent).Children[2]; // Припускається, що NewsPage - третій таб
        }
    }
}