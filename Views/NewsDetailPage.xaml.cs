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
    public partial class NewsDetailPage : ContentPage
    {
        private readonly NewsItem _newsItem;
        private readonly NewsService _newsService;
        private bool _isLiked;

        public NewsDetailPage(NewsItem newsItem, NewsService newsService)
        {
            InitializeComponent();
            _newsItem = newsItem;
            _newsService = newsService;

            // Устанавливаем данные как BindingContext
            BindingContext = _newsItem;

            // Создаем и отображаем теги
            DisplayTags();

            // Проверяем, поставил ли пользователь лайк этой новости
            CheckLikedStatus();
        }

        // Отображение тегов
        private void DisplayTags()
        {
            if (_newsItem.Tags != null && _newsItem.Tags.Any())
            {
                tagsLayout.Children.Clear();

                foreach (var tag in _newsItem.Tags)
                {
                    var frame = new Frame
                    {
                        BackgroundColor = Color.FromHex("#E0F2F1"),
                        CornerRadius = 15,
                        Padding = new Thickness(10, 5),
                        Margin = new Thickness(0, 0, 8, 8),
                        HasShadow = false
                    };

                    var label = new Label
                    {
                        Text = $"#{tag}",
                        TextColor = Color.FromHex("#00796B"),
                        FontSize = 12
                    };

                    frame.Content = label;
                    tagsLayout.Children.Add(frame);
                }
            }
        }

        // Проверка, поставлен ли лайк данной новости
        private void CheckLikedStatus()
        {
            _isLiked = _newsService.IsNewsLiked(_newsItem.Id);
            UpdateLikeButton();
        }

        // Обновление внешнего вида кнопки лайка
        private void UpdateLikeButton()
        {
            if (_isLiked)
            {
                likeButton.Text = "❤️ Нравится";
                likeButton.BackgroundColor = Color.FromHex("#E57373");
            }
            else
            {
                likeButton.Text = "👍 Нравится";
                likeButton.BackgroundColor = Color.FromHex("#66BB6A");
            }
        }

        // Обработчик нажатия на кнопку "Нравится"
        private async void LikeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (_isLiked)
                {
                    // Если уже есть лайк, то убираем его
                    await _newsService.UnlikeNewsAsync(_newsItem.Id);
                    _isLiked = false;
                    _newsItem.LikesCount--; // Уменьшаем счетчик лайков
                }
                else
                {
                    // Если лайка нет, то добавляем его
                    await _newsService.LikeNewsAsync(_newsItem.Id);
                    _isLiked = true;
                    _newsItem.LikesCount++; // Увеличиваем счетчик лайков
                }

                // Обновляем отображение
                UpdateLikeButton();
                likesCountLabel.Text = _newsItem.LikesCount.ToString();

                // Добавляем небольшую анимацию для кнопки
                await likeButton.ScaleTo(1.1, 100);
                await likeButton.ScaleTo(1.0, 100);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось обновить статус лайка: {ex.Message}", "OK");
            }
        }

        // Обработчик нажатия на кнопку "Поделиться"
        private async void ShareButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Создаем текст для шаринга
                string shareText = $"{_newsItem.Title}\n\n{_newsItem.Summary}\n\nУзнайте больше в приложении 'Мои Растения'!";

                // Вызываем нативный диалог шаринга
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = shareText,
                    Title = "Поделиться статьей"
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось поделиться статьей: {ex.Message}", "OK");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Закрываем модальное окно при нажатии кнопки "Назад"
            Navigation.PopModalAsync();
            return true;
        }
    }
}