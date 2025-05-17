using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<Plant> _allPlants;
        private Action _saveCallback;

        // Массивы имен изображений для каждого скина (сухое и политое состояние)
        private readonly string[] _drySkinImages = {
            "plant_default_dry.png",
            "plant_skin1_dry.png",
            "plant_skin2_dry.png",
            "plant_skin3_dry.png",
            "plant_skin4_dry.png",
            "plant_skin5_dry.png"
        };

        private readonly string[] _wateredSkinImages = {
            "plant_default_watered.png",
            "plant_skin1_watered.png",
            "plant_skin2_watered.png",
            "plant_skin3_watered.png",
            "plant_skin4_watered.png",
            "plant_skin5_watered.png"
        };

        public PlantDisplayPage(Plant plant, List<Plant> allPlants, Action saveCallback)
        {
            InitializeComponent();
            _plant = plant;
            _allPlants = allPlants;
            _saveCallback = saveCallback;

            // Добавляем кнопку "Назад" в заголовок
            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Назад",
                Command = new Command(async () => await CloseCurrentPage())
            });

            // Инициализируем UI
            UpdateUI();
        }

        // Метод для закрытия текущей страницы
        private async Task CloseCurrentPage()
        {
            await Navigation.PopModalAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Обрабатываем нажатие системной кнопки "Назад"
            Device.BeginInvokeOnMainThread(async () => {
                await CloseCurrentPage();
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
            // Обновляем заголовок
            plantNameLabel.Text = _plant.Name;

            // Обновляем статус полива
            string statusText = _plant.IsWatered ? "Полито" : "Не полито";
            plantStatusLabel.Text = $"Статус: {statusText}";
            statusDetailLabel.Text = statusText;

            // Обновляем информацию о последнем поливе
            if (_plant.LastWatered > DateTime.MinValue)
            {
                lastWateredLabel.Text = _plant.LastWatered.ToString("dd.MM.yyyy HH:mm");

                // Рассчитываем примерное время следующего полива (через 2 дня после последнего)
                DateTime nextWatering = _plant.LastWatered.AddDays(2);
                if (DateTime.Now >= nextWatering)
                {
                    nextWateringLabel.Text = "Можно полить сейчас";
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
                lastWateredLabel.Text = "Еще не поливалось";
                nextWateringLabel.Text = "Можно полить сейчас";
                nextWateringLabel.TextColor = Color.FromHex("#4CAF50");
            }

            // Обновляем кнопку полива
            waterButton.IsEnabled = !_plant.IsWatered;
            waterButton.Text = _plant.IsWatered ? "Уже полито" : "Полить растение";
            waterButton.BackgroundColor = _plant.IsWatered ? Color.FromHex("#BDBDBD") : Color.FromHex("#66BB6A");

            // Обновляем изображение растения
            UpdatePlantImage();
        }

        private void UpdatePlantImage()
        {
            // Проверяем индекс скина в допустимых пределах
            int skinIndex = Math.Min(Math.Max(_plant.SkinIndex, 0), _drySkinImages.Length - 1);

            // Выбираем изображение в зависимости от статуса полива
            string imageName = _plant.IsWatered
                ? _wateredSkinImages[skinIndex]
                : _drySkinImages[skinIndex];

            plantImage.Source = imageName;
        }

        private async void WaterPlant_Clicked(object sender, EventArgs e)
        {
            if (!_plant.IsWatered)
            {
                // Меняем статус полива
                _plant.IsWatered = true;
                _plant.LastWatered = DateTime.Now;

                // Сохраняем изменения
                _saveCallback?.Invoke();

                // Анимация полива
                await AnimateWatering();

                // Обновляем UI
                UpdateUI();

                // Показываем уведомление
                await DisplayAlert("Успех", $"Растение {_plant.Name} полито!", "OK");
            }
        }

        private async Task AnimateWatering()
        {
            // Анимация исчезновения
            await plantImage.FadeTo(0.3, 300);

            // Меняем изображение на политое
            UpdatePlantImage();

            // Анимация появления
            await plantImage.FadeTo(1, 300);
        }

        private async void SkinSelected_Tapped(object sender, EventArgs e)
        {
            var tappedFrame = sender as Frame;
            var tapGesture = tappedFrame.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;

            if (tapGesture != null && int.TryParse((string)tapGesture.CommandParameter, out int selectedSkinIndex))
            {
                // Проверяем, что индекс в допустимом диапазоне
                if (selectedSkinIndex >= 0 && selectedSkinIndex < _drySkinImages.Length)
                {
                    // Сохраняем текущий скин
                    _plant.SkinIndex = selectedSkinIndex;

                    // Сохраняем изменения
                    _saveCallback?.Invoke();

                    // Анимируем изменение скина
                    await plantImage.ScaleTo(0.8, 150);
                    UpdatePlantImage();
                    await plantImage.ScaleTo(1, 150);

                    // Выводим уведомление
                    await DisplayAlert("Скин изменен", $"Вид растения изменен", "OK");
                }
            }
        }

        private async void ViewNotes_Clicked(object sender, EventArgs e)
        {
            // Используем PushModalAsync вместо PushAsync для перехода на страницу с записями
            await Navigation.PushModalAsync(new PlantNotesPage(_plant, _saveCallback));
        }
    }
}