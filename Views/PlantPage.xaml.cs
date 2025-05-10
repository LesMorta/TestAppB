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
    public partial class PlantPage : ContentPage
    {
        private int waterCount;
        private int appOpenCount;
        private DateTime lastOpenDate;
        private int consecutiveDays = 0;
        private Random random = new Random();

        private const string StatusKey = "plant_watered";
        private const string LastWateredKey = "plant_last_watered";
        private const string LastOpenDateKey = "last_open_date";
        private const string ConsecutiveDaysKey = "consecutive_days";

        // Советы по уходу за растениями
        private string[] plantTips = new string[]
        {
            "Большинство комнатных растений предпочитают яркий непрямой свет.",
            "Поливайте растения рано утром или вечером, чтобы избежать быстрого испарения.",
            "Многие растения любят влажный воздух — можно использовать увлажнитель.",
            "Удобряйте растения весной и летом, когда они активно растут.",
            "Регулярно очищайте листья растений от пыли мягкой влажной тканью.",
            "Поворачивайте горшки с растениями, чтобы обеспечить равномерный рост.",
            "Большинство растений не любят сквозняки.",
            "Пересаживайте комнатные растения раз в 1-2 года для здоровья корней.",
            "Желтеющие листья могут быть признаком чрезмерного полива.",
            "Коричневые кончики листьев часто указывают на сухой воздух.",
            "Не ставьте растения рядом с отопительными приборами."
        };

        public PlantPage()
        {
            InitializeComponent();
            CheckPlantStatus();
            SetGreeting();
            ShowRandomTip();
            UpdateLastAchievement();
        }

        private void SetGreeting()
        {
            int hour = DateTime.Now.Hour;
            string greeting;

            if (hour >= 5 && hour < 12)
                greeting = "Доброе утро! 🌞";
            else if (hour >= 12 && hour < 18)
                greeting = "Добрый день! 🌤";
            else if (hour >= 18 && hour < 22)
                greeting = "Добрый вечер! 🌙";
            else
                greeting = "Доброй ночи! ✨";

            greetingLabel.Text = greeting;
        }

        private void ShowRandomTip()
        {
            int tipIndex = random.Next(plantTips.Length);
            plantTipDetailLabel.Text = plantTips[tipIndex];
        }

        private void UpdateLastAchievement()
        {
            // Находим последнее разблокированное достижение
            Achievement lastAchievement = null;

            foreach (var ach in AchievementService.AllAchievements)
            {
                if (Preferences.Get($"Ach_{ach.Id}", false))
                {
                    lastAchievement = ach;
                }
            }

            if (lastAchievement != null)
            {
                lastAchievementIcon.Source = lastAchievement.Icon;
                lastAchievementLabel.Text = lastAchievement.Title;
            }
        }

        private void CheckPlantStatus()
        {
            string lastWateredString = Preferences.Get(LastWateredKey, null);
            bool isWatered = Preferences.Get(StatusKey, false);

            if (!string.IsNullOrEmpty(lastWateredString))
            {
                DateTime lastWatered = DateTime.Parse(lastWateredString);

                if ((DateTime.Now - lastWatered).TotalHours >= 24)
                {
                    isWatered = false;
                    Preferences.Set(StatusKey, false);
                }

                TimeSpan timeSinceWatered = DateTime.Now - lastWatered;

                lastWateredLabel.Text = $"Последний полив: {lastWatered.ToString("dd MMMM HH:mm")}";

                // Показываем примерное время до следующего полива
                if (isWatered)
                {
                    TimeSpan timeRemaining = TimeSpan.FromHours(24) - timeSinceWatered;
                    nextWateringLabel.Text = $"Следующий полив через: {FormatTimeSpan(timeRemaining)}";
                }
                else
                {
                    nextWateringLabel.Text = "Растению требуется полив!";
                }
            }
            else
            {
                lastWateredLabel.Text = "Растение ещё не поливали";
                nextWateringLabel.Text = "Растению требуется полив!";
            }

            UpdateUI(isWatered);
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours} ч {timeSpan.Minutes} мин";
            }
            else
            {
                return $"{timeSpan.Minutes} мин";
            }
        }

        private void UpdateUI(bool isWatered)
        {
            if (isWatered)
            {
                statusLabel.Text = "Растение полито 🌿";
                plantImage.Source = "plant_watered.png";
            }
            else
            {
                statusLabel.Text = "Растению нужна вода 💧";
                plantImage.Source = "plant_dry.png";
            }
        }

        private async void WaterButton_Clicked(object sender, EventArgs e)
        {
            waterCount = Preferences.Get("WaterCount", 0) + 1;
            Preferences.Set("WaterCount", waterCount);

            CheckWaterTimeAchievements();
            CheckSeasonsAchievements();

            Preferences.Set(StatusKey, true);
            Preferences.Set(LastWateredKey, DateTime.Now.ToString());
            CheckPlantStatus();

            CheckAchievements();
            UpdateLastAchievement();

            // Добавим небольшую анимацию при поливе
            await plantImage.ScaleTo(1.1, 250, Easing.SpringOut);
            await plantImage.ScaleTo(1.0, 250, Easing.SpringIn);
        }

        private void OpenPlantsPage_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MyPlantsPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            appOpenCount = Preferences.Get("AppOpenCount", 0) + 1;
            Preferences.Set("AppOpenCount", appOpenCount);

            // Перевірка послідовних днів входу
            string lastOpenDateStr = Preferences.Get(LastOpenDateKey, DateTime.MinValue.ToString());
            if (DateTime.TryParse(lastOpenDateStr, out lastOpenDate))
            {
                DateTime today = DateTime.Today;
                TimeSpan difference = today - lastOpenDate.Date;

                if (difference.Days == 1)
                {
                    // Послідовний день
                    consecutiveDays = Preferences.Get(ConsecutiveDaysKey, 0) + 1;
                    Preferences.Set(ConsecutiveDaysKey, consecutiveDays);
                }
                else if (difference.Days > 1)
                {
                    // Розрив послідовності
                    consecutiveDays = 1;
                    Preferences.Set(ConsecutiveDaysKey, consecutiveDays);
                }
                // Якщо той самий день, нічого не змінюємо
            }
            else
            {
                consecutiveDays = 1;
                Preferences.Set(ConsecutiveDaysKey, consecutiveDays);
            }

            // Зберігаємо поточну дату як останню дату входу
            Preferences.Set(LastOpenDateKey, DateTime.Today.ToString());

            CheckAchievements();
            SetGreeting();
            ShowRandomTip();
            UpdateLastAchievement();
        }

        private void CheckAchievements()
        {
            int plantCount = 0;
            if (Application.Current.Properties.ContainsKey("plants"))
            {
                var json = Application.Current.Properties["plants"] as string;
                var plants = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Plant>>(json);
                plantCount = plants?.Count ?? 0;
            }

            foreach (var ach in AchievementService.AllAchievements)
            {
                if (Preferences.Get($"Ach_{ach.Id}", false))
                    continue;

                switch (ach.Id)
                {
                    // Існуючі досягнення
                    case "first_water":
                        if (waterCount >= 1) Unlock(ach);
                        break;
                    case "ten_waters":
                        if (waterCount >= 10) Unlock(ach);
                        break;
                    case "open_app_5":
                        if (appOpenCount >= 5) Unlock(ach);
                        break;

                    // Нові досягнення поливу
                    case "fifty_waters":
                        if (waterCount >= 50) Unlock(ach);
                        break;
                    case "hundred_waters":
                        if (waterCount >= 100) Unlock(ach);
                        break;
                    case "five_hundred_waters":
                        if (waterCount >= 500) Unlock(ach);
                        break;

                    // Досягнення за кількість рослин
                    case "three_plants":
                        if (plantCount >= 3) Unlock(ach);
                        break;
                    case "ten_plants":
                        if (plantCount >= 10) Unlock(ach);
                        break;
                    case "twenty_plants":
                        if (plantCount >= 20) Unlock(ach);
                        break;

                    // Досягнення за регулярність
                    case "daily_3":
                        if (consecutiveDays >= 3) Unlock(ach);
                        break;
                    case "daily_7":
                        if (consecutiveDays >= 7) Unlock(ach);
                        break;
                    case "daily_30":
                        if (consecutiveDays >= 30) Unlock(ach);
                        break;

                        // Інші досягнення перевіряються в інших методах
                }
            }
        }

        private void CheckWaterTimeAchievements()
        {
            int currentHour = DateTime.Now.Hour;

            // Перевірка досягнень за часом поливу
            if (currentHour >= 6 && currentHour < 10 && !Preferences.Get("Ach_morning_water", false))
            {
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "morning_water"));
            }

            if (currentHour >= 18 && currentHour < 22 && !Preferences.Get("Ach_evening_water", false))
            {
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "evening_water"));
            }

            if ((currentHour >= 23 || currentHour < 5) && !Preferences.Get("Ach_night_owl", false))
            {
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "night_owl"));
            }

            // Перевірка для досягнення вихідного дня
            if ((DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday))
            {
                string weekendKey = $"Weekend_{DateTime.Now.Year}_{GetIso8601WeekOfYear(DateTime.Now)}";

                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                {
                    Preferences.Set($"{weekendKey}_Saturday", true);
                }
                else
                {
                    Preferences.Set($"{weekendKey}_Sunday", true);
                }

                // Якщо обидва дні виконані, відкриваємо досягнення
                if (Preferences.Get($"{weekendKey}_Saturday", false) &&
                    Preferences.Get($"{weekendKey}_Sunday", false) &&
                    !Preferences.Get("Ach_weekend_care", false))
                {
                    Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "weekend_care"));
                }
            }
        }

        private void CheckSeasonsAchievements()
        {
            int month = DateTime.Now.Month;

            // Визначення сезону
            if (month >= 3 && month <= 5 && !Preferences.Get("Ach_spring_water", false))
            {
                // Весна
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "spring_water"));
            }
            else if (month >= 6 && month <= 8 && !Preferences.Get("Ach_summer_water", false))
            {
                // Літо
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "summer_water"));
            }
            else if (month >= 9 && month <= 11 && !Preferences.Get("Ach_autumn_water", false))
            {
                // Осінь
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "autumn_water"));
            }
            else if ((month == 12 || month <= 2) && !Preferences.Get("Ach_winter_water", false))
            {
                // Зима
                Unlock(AchievementService.AllAchievements.FirstOrDefault(a => a.Id == "winter_water"));
            }
        }

        private async void Unlock(Achievement achievement)
        {
            if (achievement == null) return;

            Preferences.Set($"Ach_{achievement.Id}", true);
            await DisplayAlert("Досягнення!", $"Отримано: {achievement.Title}", "ОК");
            UpdateLastAchievement();
        }

        // Допоміжний метод для визначення номера тижня
        private int GetIso8601WeekOfYear(DateTime date)
        {
            var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
}