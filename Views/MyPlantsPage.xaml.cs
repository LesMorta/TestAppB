using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using TestAppB.Models;
using TestAppB.Services;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyPlantsPage : ContentPage
    {
        private List<Plant> plants = new List<Plant>();

        public MyPlantsPage()
        {
            InitializeComponent();
            LoadPlants();
        }

        private void LoadPlants()
        {
            if (Application.Current.Properties.ContainsKey("plants"))
            {
                var json = Application.Current.Properties["plants"] as string;
                plants = JsonConvert.DeserializeObject<List<Plant>>(json);
            }

            // Проверяем и обновляем статус растений (24 часа)
            CheckAndUpdatePlantStatus();

            // Обновляем UI
            UpdateUI();
        }

        private void CheckAndUpdatePlantStatus()
        {
            DateTime now = DateTime.Now;
            bool needSave = false;

            foreach (var plant in plants)
            {
                if (plant.IsWatered && plant.LastWatered != DateTime.MinValue)
                {
                    TimeSpan timeSinceWatered = now - plant.LastWatered;
                    if (timeSinceWatered.TotalHours >= 24)
                    {
                        plant.IsWatered = false;
                        needSave = true;
                    }
                }
            }

            if (needSave)
            {
                SavePlants();
            }
        }

        private void UpdateUI()
        {
            // Устанавливаем источник данных
            plantsList.ItemsSource = null;
            plantsList.ItemsSource = plants;

            // Обновляем статистику
            totalPlantsLabel.Text = plants.Count.ToString();
            wateredTodayLabel.Text = plants.Count(p => p.IsWatered).ToString();

            // Показываем/скрываем пустое состояние
            emptyState.IsVisible = plants.Count == 0;
            plantsList.IsVisible = plants.Count > 0;
        }

        private async void AddPlant_Clicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Новое растение", "Введите название:",
                placeholder: "Например: Фикус");

            if (!string.IsNullOrWhiteSpace(result))
            {
                plants.Add(new Plant
                {
                    Name = result,
                    IsWatered = false,
                    LastWatered = DateTime.MinValue
                });

                SavePlants();
                LoadPlants();

                // Проверка достижений за количество растений
                CheckPlantCountAchievements();

                // Показываем анимацию
                await DisplayAlert("🌱", $"Растение \"{result}\" добавлено!", "OK");
            }
        }

        private void SavePlants()
        {
            var json = JsonConvert.SerializeObject(plants);
            Application.Current.Properties["plants"] = json;
            Application.Current.SavePropertiesAsync();
        }

        private async void ToggleWatered_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var plant = button?.CommandParameter as Plant;

            if (plant != null)
            {
                plant.IsWatered = !plant.IsWatered;
                plant.LastWatered = plant.IsWatered ? DateTime.Now : plant.LastWatered;

                SavePlants();
                LoadPlants();

                // Анимация кнопки
                await button.ScaleTo(1.2, 100);
                await button.ScaleTo(1.0, 100);

                // Проверка достижения "Тотальный полив"
                CheckWaterAllAchievement();

                // Обновляем визуальное состояние кнопки
                button.BackgroundColor = plant.IsWatered ? Color.FromHex("#66BB6A") : Color.FromHex("#757575");
                button.TextColor = Color.White;
            }
        }

        private async void DeletePlant_Invoked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var plant = swipeItem?.BindingContext as Plant;

            if (plant != null)
            {
                bool confirm = await DisplayAlert("Удаление растения",
                    $"Вы уверены, что хотите удалить \"{plant.Name}\"?",
                    "Да", "Нет");

                if (confirm)
                {
                    plants.Remove(plant);
                    SavePlants();
                    LoadPlants();

                    // Открываем достижение за удаление растения
                    CheckDeletePlantAchievement();
                }
            }
        }

        private async void EditPlant_Invoked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var plant = swipeItem?.BindingContext as Plant;

            if (plant != null)
            {
                string newName = await DisplayPromptAsync("Переименовать",
                    "Введите новое имя:",
                    initialValue: plant.Name);

                if (!string.IsNullOrWhiteSpace(newName) && newName != plant.Name)
                {
                    plant.Name = newName;
                    SavePlants();
                    LoadPlants();

                    // Открываем достижение за переименование растения
                    CheckRenamePlantAchievement();
                }
            }
        }

        private async void OnPlantTapped(object sender, EventArgs e)
        {
            var label = sender as Label;
            var plant = label?.BindingContext as Plant;

            if (plant == null) return;

            string action = await DisplayActionSheet(
                $"Растение: {plant.Name}",
                "Отмена",
                null,
                "📝 Переименовать",
                "💧 Полить",
                "🗑 Удалить");

            switch (action)
            {
                case "📝 Переименовать":
                    await RenamePlant(plant);
                    break;
                case "💧 Полить":
                    await WaterPlant(plant);
                    break;
                case "🗑 Удалить":
                    await DeletePlant(plant);
                    break;
            }
        }

        private async Task RenamePlant(Plant plant)
        {
            string newName = await DisplayPromptAsync("Переименовать",
                "Введите новое имя:",
                initialValue: plant.Name);

            if (!string.IsNullOrWhiteSpace(newName) && newName != plant.Name)
            {
                plant.Name = newName;
                SavePlants();
                LoadPlants();
                CheckRenamePlantAchievement();
            }
        }

        private async Task WaterPlant(Plant plant)
        {
            plant.IsWatered = true;
            plant.LastWatered = DateTime.Now;
            SavePlants();
            LoadPlants();
            CheckWaterAllAchievement();

            await DisplayAlert("💧", $"Растение \"{plant.Name}\" полито!", "OK");
        }

        private async Task DeletePlant(Plant plant)
        {
            bool confirm = await DisplayAlert("Удаление растения",
                $"Вы уверены, что хотите удалить \"{plant.Name}\"?",
                "Да", "Нет");

            if (confirm)
            {
                plants.Remove(plant);
                SavePlants();
                LoadPlants();
                CheckDeletePlantAchievement();
            }
        }

        private void CheckPlantCountAchievements()
        {
            int plantCount = plants.Count;

            // Проверка достижений за количество растений
            foreach (var achievement in AchievementService.AllAchievements.Where(a =>
                (a.Id == "three_plants" && plantCount >= 3) ||
                (a.Id == "ten_plants" && plantCount >= 10) ||
                (a.Id == "twenty_plants" && plantCount >= 20)))
            {
                if (!Preferences.Get($"Ach_{achievement.Id}", false))
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    ShowAchievementNotification(achievement);
                }
            }
        }

        private void CheckWaterAllAchievement()
        {
            // Проверка что все растения политы
            if (plants.Count > 0 && plants.All(p => p.IsWatered) && !Preferences.Get("Ach_water_all", false))
            {
                var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "water_all");
                if (achievement != null)
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    ShowAchievementNotification(achievement);
                }
            }
        }

        private void CheckDeletePlantAchievement()
        {
            if (!Preferences.Get("Ach_delete_plant", false))
            {
                var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "delete_plant");
                if (achievement != null)
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    ShowAchievementNotification(achievement);
                }
            }
        }

        private void CheckRenamePlantAchievement()
        {
            if (!Preferences.Get("Ach_rename_plant", false))
            {
                var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "rename_plant");
                if (achievement != null)
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    ShowAchievementNotification(achievement);
                }
            }
        }

        private async void ShowAchievementNotification(Achievement achievement)
        {
            await DisplayAlert("🏆 Достижение получено!",
                $"{achievement.Title}\n\n{achievement.Description}",
                "Отлично!");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPlants();
        }
    }
}