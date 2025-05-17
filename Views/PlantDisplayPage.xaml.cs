using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestAppB.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlantDisplayPage : ContentPage
    {
        private Plant _plant;
        private readonly Action _saveCallback;

        // Масиви імен зображень для кожного скіна у политому стані
        private readonly string[] _wateredPlantSkins = {
            "plant_watered.png",
            "plant_watered_1.png",
            "plant_watered_2.png"
        };

        // Масиви імен зображень для кожного скіна у сухому стані
        private readonly string[] _dryPlantSkins = {
            "plant_dry.png",
            "plant_dry_1.png",
            "plant_dry_2.png"
        };

        // Масив фреймів скінів для стилізації вибраного скіна
        private Frame[] _skinFrames;

        public PlantDisplayPage(Plant plant, List<Plant> allPlants, Action saveCallback)
        {
            InitializeComponent();
            _plant = plant;
            _saveCallback = saveCallback;

            // Додаємо кнопку "Назад" у заголовок
            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Назад",
                Command = new Command(async () => await Navigation.PopModalAsync())
            });

            // Ініціалізуємо масив фреймів для стилізації
            _skinFrames = new Frame[] { skinFrame0, skinFrame1, skinFrame2 };

            // Ініціалізуємо UI
            UpdateUI();
        }

        protected override bool OnBackButtonPressed()
        {
            // Обробляємо натискання системної кнопки "Назад"
            Device.BeginInvokeOnMainThread(async () => {
                await Navigation.PopModalAsync();
            });

            return true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Оновлюємо заголовок
            plantNameLabel.Text = _plant.Name;

            // Оновлюємо статус поливу
            string statusText = _plant.IsWatered ? "Полито" : "Не полито";
            plantStatusLabel.Text = $"Статус: {statusText}";
            statusDetailLabel.Text = statusText;

            // Оновлюємо інформацію про останній полив
            if (_plant.LastWatered > DateTime.MinValue)
            {
                lastWateredLabel.Text = _plant.LastWatered.ToString("dd.MM.yyyy HH:mm");

                // Розраховуємо приблизний час наступного поливу (через 2 дні після останнього)
                DateTime nextWatering = _plant.LastWatered.AddDays(2);
                if (DateTime.Now >= nextWatering)
                {
                    nextWateringLabel.Text = "Можна полити зараз";
                    nextWateringLabel.TextColor = Color.FromHex("#4CAF50");
                }
                else
                {
                    nextWateringLabel.Text = nextWatering.ToString("dd.MM.yyyy HH:mm");
                    nextWateringLabel.TextColor = Color.FromHex("#546E7A");
                }
            }
            else
            {
                lastWateredLabel.Text = "Ще не поливалося";
                nextWateringLabel.Text = "Можна полити зараз";
                nextWateringLabel.TextColor = Color.FromHex("#4CAF50");
            }

            // Оновлюємо кнопку поливу
            waterButton.IsEnabled = !_plant.IsWatered;
            waterButton.Text = _plant.IsWatered ? "Вже полито" : "Полити рослину";
            waterButton.BackgroundColor = _plant.IsWatered ? Color.FromHex("#BDBDBD") : Color.FromHex("#66BB6A");

            // Оновлюємо зображення рослини
            UpdatePlantImage();

            // Підсвічуємо вибраний скін
            UpdateSelectedSkinFrame();
        }

        private void UpdatePlantImage()
        {
            // Перевіряємо індекс скіна в допустимих межах
            int skinIndex = Math.Min(Math.Max(_plant.SkinIndex, 0), _wateredPlantSkins.Length - 1);

            // Вибираємо зображення залежно від статусу поливу
            if (_plant.IsWatered)
            {
                // Якщо рослина полита, використовуємо зображення политої рослини
                plantImage.Source = _wateredPlantSkins[skinIndex];
            }
            else
            {
                // Якщо рослина не полита, використовуємо зображення сухої рослини
                plantImage.Source = _dryPlantSkins[skinIndex];
            }
        }

        private void UpdateSelectedSkinFrame()
        {
            // Скидаємо стиль всіх фреймів
            foreach (var frame in _skinFrames)
            {
                frame.Style = (Style)Resources["SkinFrameStyle"];
            }

            // Застосовуємо стиль вибраного скіна
            int skinIndex = Math.Min(Math.Max(_plant.SkinIndex, 0), _skinFrames.Length - 1);
            _skinFrames[skinIndex].Style = (Style)Resources["SelectedSkinFrameStyle"];
        }

        private async void WaterPlant_Clicked(object sender, EventArgs e)
        {
            if (!_plant.IsWatered)
            {
                // Змінюємо статус поливу
                _plant.IsWatered = true;
                _plant.LastWatered = DateTime.Now;

                // Зберігаємо зміни
                _saveCallback?.Invoke();

                // Анімація поливу
                await AnimateWatering();

                // Оновлюємо UI
                UpdateUI();

                // Показуємо повідомлення
                await DisplayAlert("Успіх", $"Рослина {_plant.Name} полита!", "OK");
            }
        }

        private async Task AnimateWatering()
        {
            // Анімація зникнення
            await plantImage.FadeTo(0.3, 300);

            // Оновлюємо зображення на політу рослину
            UpdatePlantImage();

            // Анімація появи
            await plantImage.FadeTo(1, 300);
        }

        private async void SkinSelected_Tapped(object sender, EventArgs e)
        {
            var tappedFrame = sender as Frame;
            var tapGesture = tappedFrame.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;

            if (tapGesture != null && int.TryParse((string)tapGesture.CommandParameter, out int selectedSkinIndex))
            {
                // Перевіряємо, що індекс в допустимому діапазоні
                if (selectedSkinIndex >= 0 && selectedSkinIndex < _wateredPlantSkins.Length)
                {
                    // Якщо це вже вибраний скін, не робимо нічого
                    if (_plant.SkinIndex == selectedSkinIndex)
                        return;

                    // Зберігаємо поточний скін
                    _plant.SkinIndex = selectedSkinIndex;

                    // Зберігаємо зміни
                    _saveCallback?.Invoke();

                    // Анімуємо зміну скіна
                    await plantImage.ScaleTo(0.8, 150);
                    UpdatePlantImage();
                    UpdateSelectedSkinFrame();
                    await plantImage.ScaleTo(1, 150);

                    // Виводимо повідомлення
                    await DisplayAlert("Успіх", "Вигляд рослини змінено", "OK");
                }
            }
        }

        private async void ViewNotes_Clicked(object sender, EventArgs e)
        {
            // Використовуємо PushModalAsync для переходу на сторінку з записами
            await Navigation.PushModalAsync(new PlantNotesPage(_plant, _saveCallback));
        }
    }
}