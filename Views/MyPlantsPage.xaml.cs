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
            plantsList.ItemsSource = null;
            plantsList.ItemsSource = plants;
        }

        private async void AddPlant_Clicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Новое растение", "Введите название:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                plants.Add(new Plant { Name = result, IsWatered = false, LastWatered = DateTime.MinValue });
                SavePlants();
                LoadPlants();

                // Перевірка досягнень за кількістю рослин
                CheckPlantCountAchievements();
            }
        }

        private void SavePlants()
        {
            var json = JsonConvert.SerializeObject(plants);
            Application.Current.Properties["plants"] = json;
        }

        private void ToggleWatered_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var plant = button?.CommandParameter as Plant;

            if (plant != null)
            {
                plant.IsWatered = !plant.IsWatered;
                plant.LastWatered = plant.IsWatered ? DateTime.Now : plant.LastWatered;
                SavePlants();
                LoadPlants();

                // Перевірка досягнення "Тотальний полив"
                CheckWaterAllAchievement();
            }
        }

        private void DeletePlant_Invoked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var plant = swipeItem?.BindingContext as Plant;

            if (plant != null)
            {
                plants.Remove(plant);
                SavePlants();
                LoadPlants();

                // Відкриваємо досягнення за видалення рослини
                if (!Preferences.Get("Ach_delete_plant", false))
                {
                    var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "delete_plant");
                    if (achievement != null)
                    {
                        Preferences.Set($"Ach_{achievement.Id}", true);
                        DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
                    }
                }
            }
        }

        private async void OnPlantTapped(object sender, EventArgs e)
        {
            var label = sender as Label;
            var plant = label?.BindingContext as Plant;

            if (plant == null) return;

            string action = await DisplayActionSheet("Выберите действие", "Отмена", null, "Переименовать", "Удалить");

            if (action == "Переименовать")
            {
                string newName = await DisplayPromptAsync("Переименовать", "Введите новое имя:", initialValue: plant.Name);
                if (!string.IsNullOrWhiteSpace(newName) && newName != plant.Name)
                {
                    plant.Name = newName;
                    SavePlants();
                    LoadPlants();

                    // Відкриваємо досягнення за перейменування рослини
                    if (!Preferences.Get("Ach_rename_plant", false))
                    {
                        var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "rename_plant");
                        if (achievement != null)
                        {
                            Preferences.Set($"Ach_{achievement.Id}", true);
                            await DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
                        }
                    }
                }
            }
            else if (action == "Удалить")
            {
                bool confirm = await DisplayAlert("Удалить", $"Удалить растение \"{plant.Name}\"?", "Да", "Нет");
                if (confirm)
                {
                    plants.Remove(plant);
                    SavePlants();
                    LoadPlants();

                    // Відкриваємо досягнення за видалення рослини
                    if (!Preferences.Get("Ach_delete_plant", false))
                    {
                        var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "delete_plant");
                        if (achievement != null)
                        {
                            Preferences.Set($"Ach_{achievement.Id}", true);
                            await DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
                        }
                    }
                }
            }
        }

        private void CheckPlantCountAchievements()
        {
            int plantCount = plants.Count;

            // Перевірка досягнень за кількістю рослин
            foreach (var achievement in AchievementService.AllAchievements.Where(a =>
                (a.Id == "three_plants" && plantCount >= 3) ||
                (a.Id == "ten_plants" && plantCount >= 10) ||
                (a.Id == "twenty_plants" && plantCount >= 20)))
            {
                if (!Preferences.Get($"Ach_{achievement.Id}", false))
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
                }
            }
        }

        private void CheckWaterAllAchievement()
        {
            // Перевірка чи всі рослини политі
            if (plants.Count > 0 && plants.All(p => p.IsWatered) && !Preferences.Get("Ach_water_all", false))
            {
                var achievement = AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "water_all");
                if (achievement != null)
                {
                    Preferences.Set($"Ach_{achievement.Id}", true);
                    DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
                }
            }
        }
    }
}