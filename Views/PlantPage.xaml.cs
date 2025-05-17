
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
            "Не забывайте, что разные растения требуют разного режима полива. Проверяйте почву перед каждым поливом!",
            "Большинство комнатных растений предпочитают яркий непрямой свет. Избегайте размещать их под прямыми солнечными лучами.",
            "Используйте воду комнатной температуры для полива. Холодная вода может вызвать шок у растений.",
            "Не ставьте растения возле обогревателей или кондиционеров - резкие перепады температуры вредны для них.",
            "Регулярно очищайте листья растений от пыли влажной тканью. Это помогает им лучше дышать.",
            "Если у растения желтеют листья, это может быть признаком чрезмерного полива. Дайте почве просохнуть перед следующим поливом."
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

            // Обновляем достижение за вход в приложение
            int appOpenCount = Preferences.Get("app_open_count", 0) + 1;
            Preferences.Set("app_open_count", appOpenCount);

            // Проверяем достижения за открытие приложения
            if (appOpenCount == 5)
            {
                AchievementService.UnlockAchievement("open_app_5");
                UpdateLastAchievement(); // Обновляем отображение, если достижение получено
            }
        }

        private void LoadData()
        {
            // Загружаем растения из хранилища
            plants = LoadPlants();
            plantsCollectionView.ItemsSource = plants;

            // Обновляем статистику
            totalPlantsLabel.Text = plants.Count.ToString();
            wateredPlantsLabel.Text = plants.Count(p => p.IsWatered).ToString();

            // Определяем, нужно ли показывать пустой список
            emptyPlantsLayout.IsVisible = plants.Count == 0;
            plantsCollectionView.IsVisible = plants.Count > 0;

            // Обновляем дату и совет дня
            dateTimeLabel.Text = $"{DateTime.Now:dddd, d MMMM} • Прекрасный день для растений!";
            tipOfDayLabel.Text = GetRandomTip();

            // Обновляем количество разблокированных достижений
            var achievements = AchievementService.GetAchievements();
            achievementsLabel.Text = achievements.Count(a => a.IsUnlocked).ToString();
        }

        private void UpdateLastAchievement()
        {
            // Получаем последнее разблокированное достижение
            var lastAchievement = AchievementService.GetLastUnlockedAchievement();

            if (lastAchievement != null)
            {
                lastAchievementLayout.IsVisible = true;
                noAchievementLayout.IsVisible = false;

                lastAchievementTitle.Text = lastAchievement.Title;
                lastAchievementDescription.Text = lastAchievement.Description;
                lastAchievementIcon.Source = lastAchievement.Icon;

                // Форматируем время получения
                if (lastAchievement.UnlockTime > DateTime.MinValue)
                {
                    TimeSpan timeAgo = DateTime.Now - lastAchievement.UnlockTime;
                    if (timeAgo.TotalDays < 1)
                    {
                        if (timeAgo.TotalHours < 1)
                            lastAchievementTime.Text = "Получено только что";
                        else
                            lastAchievementTime.Text = $"Получено {(int)timeAgo.TotalHours} ч. назад";
                    }
                    else if (timeAgo.TotalDays < 30)
                    {
                        lastAchievementTime.Text = $"Получено {(int)timeAgo.TotalDays} дн. назад";
                    }
                    else
                    {
                        lastAchievementTime.Text = $"Получено {lastAchievement.UnlockTime:dd.MM.yyyy}";
                    }
                }
                else
                {
                    lastAchievementTime.Text = "Получено ранее";
                }
            }
            else
            {
                lastAchievementLayout.IsVisible = false;
                noAchievementLayout.IsVisible = true;
            }
        }

        private List<Plant> LoadPlants()
        {
            try
            {
                // Загружаем сохраненные растения
                string savedPlantsJson = Preferences.Get("saved_plants", string.Empty);
                if (string.IsNullOrEmpty(savedPlantsJson))
                {
                    return new List<Plant>(); // Возвращаем пустой список если нет сохраненных растений
                }

                var loadedPlants = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Plant>>(savedPlantsJson);

                // Проверяем и инициализируем поля, которые могут быть null после десериализации
                foreach (var plant in loadedPlants)
                {
                    if (plant.Notes == null)
                        plant.Notes = new List<PlantNote>();
                }

                return loadedPlants;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки растений: {ex.Message}");
                return new List<Plant>();
            }
        }

        private void SavePlants()
        {
            try
            {
                if (plants == null || plants.Count == 0)
                {
                    // Если список пуст, сохраняем пустую строку
                    Preferences.Remove("saved_plants");
                    return;
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(plants);
                Preferences.Set("saved_plants", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения растений: {ex.Message}");
            }
        }

        private string GetRandomTip()
        {
            Random random = new Random();
            return tips[random.Next(tips.Length)];
        }

        private async void AddPlant_Clicked(object sender, EventArgs e)
        {
            // Диалог для ввода имени растения
            string plantName = await DisplayPromptAsync("Новое растение",
                "Введите название вашего растения",
                "Добавить", "Отмена",
                "Например: Фикус, Орхидея...");

            if (!string.IsNullOrWhiteSpace(plantName))
            {
                // Создаем новое растение
                Plant newPlant = new Plant
                {
                    Name = plantName,
                    IsWatered = false,
                    LastWatered = DateTime.MinValue
                };

                // Добавляем в коллекцию
                plants.Add(newPlant);
                SavePlants();
                LoadData();

                // Проверяем достижения по количеству растений
                CheckPlantCountAchievements();
            }
        }

        private async void WaterPlant_Clicked(object sender, EventArgs e)
        {
            // Получаем название растения из параметра команды
            string plantName = (string)((Button)sender).CommandParameter;

            // Находим растение в коллекции
            Plant plant = plants.FirstOrDefault(p => p.Name == plantName);
            if (plant != null)
            {
                // Обновляем статус полива
                plant.IsWatered = true;
                plant.LastWatered = DateTime.Now;
                SavePlants();
                LoadData();

                // Анимация успешного полива
                await AnimateWateringSuccess(sender);

                // Проверяем достижения полива
                CheckWateringAchievements();
            }
        }

        private async Task AnimateWateringSuccess(object sender)
        {
            if (sender is Button button)
            {
                // Анимация для кнопки полива
                await button.ScaleTo(1.2, 150);
                await button.ScaleTo(1.0, 150);
            }
        }

        private async void PlantDetails_Tapped(object sender, EventArgs e)
        {
            // Получаем имя растения из параметра команды
            var tapGesture = ((Frame)sender).GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            if (tapGesture == null) return;

            string plantName = (string)tapGesture.CommandParameter;
            if (string.IsNullOrEmpty(plantName)) return;

            // Находим растение в коллекции
            Plant plant = plants.FirstOrDefault(p => p.Name == plantName);
            if (plant == null) return;

            // Показываем контекстное меню
            string action = await DisplayActionSheet(
                $"Действия с растением: {plant.Name}",
                "Отмена",
                null,
                "Отобразить цветок",
                "Переименовать",
                "Добавить запись",
                "Удалить");

            switch (action)
            {
                case "Отобразить цветок":
                    await Navigation.PushModalAsync(new PlantDisplayPage(plant, plants, SavePlants));
                    break;

                case "Переименовать":
                    await RenamePlant(plant);
                    break;

                case "Добавить запись":
                    await Navigation.PushModalAsync(new PlantNotesPage(plant, SavePlants));
                    break;

                case "Удалить":
                    await DeletePlant(plant);
                    break;
            }
        }

        private async Task RenamePlant(Plant plant)
        {
            // Диалог для переименования растения
            string newName = await DisplayPromptAsync(
                "Переименование",
                "Введите новое название для растения",
                "Сохранить",
                "Отмена",
                initialValue: plant.Name);

            if (!string.IsNullOrWhiteSpace(newName) && newName != plant.Name)
            {
                // Проверяем, нет ли уже растения с таким именем
                if (plants.Any(p => p.Name == newName))
                {
                    await DisplayAlert("Ошибка", "Растение с таким названием уже существует", "OK");
                    return;
                }

                // Запоминаем старое имя для проверки достижения
                string oldName = plant.Name;

                // Обновляем имя
                plant.Name = newName;

                // Сохраняем изменения
                SavePlants();
                LoadData();

                // Разблокируем достижение за переименование растения
                AchievementService.UnlockAchievement("rename_plant");
                UpdateLastAchievement();

                await DisplayAlert("Готово", $"Растение переименовано с \"{oldName}\" на \"{newName}\"", "OK");
            }
        }
        private async Task DeletePlant(Plant plant)
        {
            // Запрашиваем подтверждение удаления
            bool confirm = await DisplayAlert(
                "Удаление растения",
                $"Вы уверены, что хотите удалить растение \"{plant.Name}\"?",
                "Да, удалить",
                "Отмена");

            if (confirm)
            {
                // Удаляем растение из коллекции
                plants.Remove(plant);

                // Сохраняем изменения
                SavePlants();
                LoadData();

                // Разблокируем достижение за удаление растения
                AchievementService.UnlockAchievement("delete_plant");
                UpdateLastAchievement();

                await DisplayAlert("Готово", $"Растение \"{plant.Name}\" удалено", "OK");
            }
        }
        private void CheckWateringAchievements()
        {
            // Считаем общее количество поливов
            int totalWaterings = Preferences.Get("total_waterings", 0) + 1;
            Preferences.Set("total_waterings", totalWaterings);

            // Проверяем достижения за количество поливов
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

            // Проверяем достижения за время суток
            int hour = DateTime.Now.Hour;
            if (hour >= 6 && hour < 10)
                AchievementService.UnlockAchievement("morning_water");
            else if (hour >= 18 && hour < 22)
                AchievementService.UnlockAchievement("evening_water");
            else if (hour >= 23 || hour < 5)
                AchievementService.UnlockAchievement("night_owl");

            // Проверяем достижение за полив всех растений
            if (plants.Count > 0 && plants.All(p => p.IsWatered))
                AchievementService.UnlockAchievement("water_all");

            // Проверяем сезонные достижения
            int month = DateTime.Now.Month;
            if (month >= 3 && month <= 5) // Весна
                AchievementService.UnlockAchievement("spring_water");
            else if (month >= 6 && month <= 8) // Лето
                AchievementService.UnlockAchievement("summer_water");
            else if (month >= 9 && month <= 11) // Осень
                AchievementService.UnlockAchievement("autumn_water");
            else // Зима (декабрь, январь, февраль)
                AchievementService.UnlockAchievement("winter_water");

            // Проверяем достижение за выходные
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                bool saturdayWatered = Preferences.Get("saturday_watered", false);
                bool sundayWatered = Preferences.Get("sunday_watered", false);

                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    Preferences.Set("saturday_watered", true);
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    Preferences.Set("sunday_watered", true);

                // Если в эти выходные поливали в обе дни
                if ((DateTime.Now.DayOfWeek == DayOfWeek.Saturday && sundayWatered) ||
                    (DateTime.Now.DayOfWeek == DayOfWeek.Sunday && saturdayWatered))
                {
                    AchievementService.UnlockAchievement("weekend_care");
                }

                // Сбрасываем статус в начале новой недели
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    Preferences.Set("saturday_watered", false);
                    Preferences.Set("sunday_watered", false);
                }
            }

            // Обновляем отображение достижений
            UpdateLastAchievement();
        }

        private void CheckPlantCountAchievements()
        {
            // Проверяем достижения за количество растений
            int plantCount = plants.Count;

            if (plantCount >= 3)
                AchievementService.UnlockAchievement("three_plants");
            if (plantCount >= 10)
                AchievementService.UnlockAchievement("ten_plants");
            if (plantCount >= 20)
                AchievementService.UnlockAchievement("twenty_plants");

            // Обновляем отображение достижений
            UpdateLastAchievement();
        }

        private void ViewAllAchievements_Clicked(object sender, EventArgs e)
        {
            // Переходим на страницу достижений
            ((TabbedPage)Parent).CurrentPage = ((TabbedPage)Parent).Children[1]; // Предполагается, что AchievementsPage - второй таб
        }

        private void ViewNews_Clicked(object sender, EventArgs e)
        {
            // Переходим на страницу новостей
            ((TabbedPage)Parent).CurrentPage = ((TabbedPage)Parent).Children[2]; // Предполагается, что NewsPage - третий таб
        }
    }
}