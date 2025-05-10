using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using TestAppB.Services;
using TestAppB.Models;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AchievementsPage : ContentPage
    {
        private List<Achievement> filteredAchievements;
        private bool showOnlyUnlocked = false;

        public AchievementsPage()
        {
            InitializeComponent();
            LoadAchievements();
        }

        private void LoadAchievements()
        {
            var allAchievements = AchievementService.AllAchievements.ToList();

            // Обновляем статус каждого достижения
            foreach (var ach in allAchievements)
            {
                ach.IsUnlocked = Preferences.Get($"Ach_{ach.Id}", false);
            }

            // Сортируем: сначала разблокированные, потом заблокированные
            filteredAchievements = allAchievements
                .OrderByDescending(a => a.IsUnlocked)
                .ThenBy(a => a.Title)
                .ToList();

            // Обновляем UI
            achievementsView.ItemsSource = filteredAchievements;
            UpdateStatistics(allAchievements);
        }

        private void UpdateStatistics(List<Achievement> achievements)
        {
            int total = achievements.Count;
            int unlocked = achievements.Count(a => a.IsUnlocked);
            double progress = total > 0 ? (double)unlocked / total * 100 : 0;

            totalAchievementsLabel.Text = total.ToString();
            unlockedAchievementsLabel.Text = $"{unlocked}/{total}";
            progressLabel.Text = $"{progress:F1}%";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Обновляем достижения при каждом появлении страницы
            LoadAchievements();
        }

        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Frame;

            // Анимация вращения кнопки
            await button.RotateTo(360, 500);
            button.Rotation = 0;

            // Перезагружаем достижения
            LoadAchievements();

            // Показываем уведомление
            await DisplayAlert("Обновлено", "Статус достижений обновлен!", "OK");
        }

        protected override bool OnBackButtonPressed()
        {
            // Возвращаемся на главную страницу
            Application.Current.MainPage = new MainPage();
            return true;
        }
    }
}