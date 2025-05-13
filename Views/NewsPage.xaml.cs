using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TestAppB.Models;
using TestAppB.Services;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewsPage : ContentPage
    {
        private readonly NewsService _newsService;
        private List<NewsItem> _allNews;
        private string _currentCategory = null;

        public ICommand RefreshCommand { get; }

        public NewsPage()
        {
            InitializeComponent();

            // Инициализация сервиса новостей
            _newsService = new NewsService();

            // Команда обновления
            RefreshCommand = new Command(async () => await RefreshNewsAsync());

            // Привязка команды к RefreshView
            BindingContext = this;

            // Загрузка новостей при создании страницы
            LoadNewsAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Проверка на наличие данных
            if (_allNews == null || !_allNews.Any())
            {
                LoadNewsAsync();
            }
        }

        // Загрузка новостей
        private async void LoadNewsAsync()
        {
            try
            {
                // Показываем индикатор загрузки
                newsRefreshView.IsRefreshing = true;

                // Загружаем новости
                _allNews = await _newsService.GetNewsAsync();

                // Применяем фильтр категории если есть
                FilterNewsByCategory(_currentCategory);

                // Скрываем индикатор загрузки
                newsRefreshView.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                await DisplayAlert("Ошибка", $"Не удалось загрузить новости: {ex.Message}", "OK");
                newsRefreshView.IsRefreshing = false;
            }
        }

        // Фильтрация новостей по категории
        private void FilterNewsByCategory(string category)
        {
            if (_allNews == null) return;

            if (string.IsNullOrEmpty(category))
            {
                // Показать все новости
                newsCollectionView.ItemsSource = _allNews;
            }
            else
            {
                // Фильтрация по категории
                newsCollectionView.ItemsSource = _allNews.Where(n => n.Category == category).ToList();
            }
        }

        // Обновление новостей
        private async Task RefreshNewsAsync()
        {
            try
            {
                // Загружаем данные из встроенного ресурса
                bool success = await _newsService.RefreshNewsFromEmbeddedResourceAsync();

                if (success)
                {
                    // Перезагружаем новости
                    _allNews = await _newsService.GetNewsAsync();

                    // Применяем фильтр категории если есть
                    FilterNewsByCategory(_currentCategory);

                    // Сбрасываем фильтр категории визуально
                    ResetCategoryFilters();
                }
                else
                {
                    await DisplayAlert("Информация", "Не удалось обновить новости", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при обновлении: {ex.Message}", "OK");
            }
            finally
            {
                // Скрываем индикатор загрузки
                newsRefreshView.IsRefreshing = false;
            }
        }

        // Сброс визуальных фильтров категории
        private void ResetCategoryFilters()
        {
            // Визуально выделяем "Все" категории
            _currentCategory = null;

            // Найти и обновить все фреймы категорий
            var categoryFrames = GetCategoryFrames();
            foreach (var frame in categoryFrames)
            {
                if (frame.Content is Label label)
                {
                    if (label.Text == "Все")
                    {
                        frame.BackgroundColor = Color.FromHex("#66BB6A");
                        label.TextColor = Color.White;
                        label.FontAttributes = FontAttributes.Bold;
                    }
                    else
                    {
                        frame.BackgroundColor = Color.FromHex("#E0F2F1");
                        label.TextColor = Color.FromHex("#00796B");
                        label.FontAttributes = FontAttributes.None;
                    }
                }
            }
        }

        // Получение всех фреймов категорий
        private List<Frame> GetCategoryFrames()
        {
            var frames = new List<Frame>();

            // Получаем доступ к StackLayout, который содержит ScrollView с категориями
            var mainStackLayout = this.Content as Grid;
            if (mainStackLayout != null)
            {
                var headerStackLayout = mainStackLayout.Children[0] as StackLayout;
                if (headerStackLayout != null && headerStackLayout.Children.Count > 1)
                {
                    var scrollView = headerStackLayout.Children[1] as ScrollView;
                    if (scrollView != null)
                    {
                        var categoriesStackLayout = scrollView.Content as StackLayout;
                        if (categoriesStackLayout != null)
                        {
                            // Собираем все фреймы из горизонтального StackLayout
                            foreach (var child in categoriesStackLayout.Children)
                            {
                                if (child is Frame frame)
                                {
                                    frames.Add(frame);
                                }
                            }
                        }
                    }
                }
            }

            return frames;
        }

        // Обработчик нажатия на новость
        private async void NewsItem_Tapped(object sender, EventArgs e)
        {
            if (e is TappedEventArgs tappedArgs)
            {
                string newsId = tappedArgs.Parameter as string;
                if (!string.IsNullOrEmpty(newsId))
                {
                    var selectedNews = _allNews.FirstOrDefault(n => n.Id == newsId);
                    if (selectedNews != null)
                    {
                        await Navigation.PushModalAsync(new NewsDetailPage(selectedNews, _newsService));
                    }
                }
            }
        }

        // Обработчик нажатия на категорию "Все"
        private void CategoryAll_Tapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            if (frame != null)
            {
                // Обновляем визуальное состояние
                ApplyCategoryFilter(frame, null);

                // Применяем фильтр ко всем новостям
                _currentCategory = null;
                FilterNewsByCategory(null);
            }
        }

        // Обработчик нажатия на категорию
        private void Category_Tapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            var args = e as TappedEventArgs;

            if (frame != null && args?.Parameter is string category)
            {
                // Обновляем визуальное состояние
                ApplyCategoryFilter(frame, category);

                // Применяем фильтр по категории
                _currentCategory = category;
                FilterNewsByCategory(category);
            }
        }

        // Применение фильтра по категории визуально
        private void ApplyCategoryFilter(Frame selectedFrame, string category)
        {
            // Сбрасываем все категории
            var frames = GetCategoryFrames();
            foreach (var frame in frames)
            {
                if (frame.Content is Label label)
                {
                    if (frame == selectedFrame)
                    {
                        frame.BackgroundColor = Color.FromHex("#66BB6A");
                        label.TextColor = Color.White;
                        label.FontAttributes = FontAttributes.Bold;
                    }
                    else
                    {
                        frame.BackgroundColor = Color.FromHex("#E0F2F1");
                        label.TextColor = Color.FromHex("#00796B");
                        label.FontAttributes = FontAttributes.None;
                    }
                }
            }
        }

        // Обработчик нажатия кнопки обновления
        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            newsRefreshView.IsRefreshing = true;
            await RefreshNewsAsync();
        }
    }
}