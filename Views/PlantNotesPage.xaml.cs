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
    public partial class PlantNotesPage : ContentPage
    {
        private readonly Plant _plant;
        private readonly Action _saveCallback;

        public PlantNotesPage(Plant plant, Action saveCallback)
        {
            InitializeComponent();
            _plant = plant;
            _saveCallback = saveCallback;

            // Добавляем кнопку "Назад" в заголовок
            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Назад",
                Command = new Command(async () => await Navigation.PopModalAsync())
            });

            // Инициализируем заголовок
            plantNameLabel.Text = $"Записи о растении: {_plant.Name}";
        }

        protected override bool OnBackButtonPressed()
        {
            // Обрабатываем нажатие системной кнопки "Назад"
            Device.BeginInvokeOnMainThread(async () => {
                await Navigation.PopModalAsync();
            });

            return true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadNotes();
        }

        private void LoadNotes()
        {
            // Если у растения нет списка записей, создаем его
            if (_plant.Notes == null)
            {
                _plant.Notes = new List<PlantNote>();
            }

            // Обновляем счетчик записей
            notesCountLabel.Text = $"Всего записей: {_plant.Notes.Count}";

            // Проверяем, есть ли записи
            bool hasNotes = _plant.Notes.Count > 0;
            emptyNotesLayout.IsVisible = !hasNotes;
            notesListLayout.IsVisible = hasNotes;

            if (hasNotes)
            {
                // Очищаем контейнер перед добавлением
                notesListLayout.Children.Clear();

                // Сортируем записи по дате создания (от новых к старым)
                var sortedNotes = _plant.Notes.OrderByDescending(n => n.CreatedAt).ToList();

                // Добавляем каждую запись в список
                foreach (var note in sortedNotes)
                {
                    AddNoteToList(note);
                }
            }
        }

        // Вынесли создание карточки заметки в отдельный метод для избежания дублирования кода
        private void AddNoteToList(PlantNote note)
        {
            var noteFrame = new Frame
            {
                Style = (Style)Resources["NoteFrameStyle"]
            };

            var noteLayout = new StackLayout();

            // Добавляем заголовок записи
            var titleLabel = new Label
            {
                Text = note.Title,
                Style = (Style)Resources["NoteTitleStyle"]
            };
            noteLayout.Children.Add(titleLabel);

            // Добавляем содержание записи
            var contentLabel = new Label
            {
                Text = note.Content,
                Style = (Style)Resources["NoteContentStyle"]
            };
            noteLayout.Children.Add(contentLabel);

            // Добавляем дату создания
            var dateLabel = new Label
            {
                Text = note.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Style = (Style)Resources["NoteDateStyle"]
            };
            noteLayout.Children.Add(dateLabel);

            // Добавляем кнопки действий
            var actionsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Кнопка редактирования
            var editButton = new Button
            {
                Text = "Изменить",
                BackgroundColor = Color.FromHex("#42A5F5"),
                TextColor = Color.White,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 15,
                FontSize = 12,
                HeightRequest = 30,
                WidthRequest = 90,
                Padding = new Thickness(5, 0),
                Margin = new Thickness(5, 0)
            };
            editButton.Clicked += (sender, e) => EditNote_Clicked(note);
            actionsLayout.Children.Add(editButton);

            // Кнопка удаления
            var deleteButton = new Button
            {
                Text = "Удалить",
                BackgroundColor = Color.FromHex("#EF5350"),
                TextColor = Color.White,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 15,
                FontSize = 12,
                HeightRequest = 30,
                WidthRequest = 90,
                Padding = new Thickness(5, 0)
            };
            deleteButton.Clicked += (sender, e) => DeleteNote_Clicked(note);
            actionsLayout.Children.Add(deleteButton);

            noteLayout.Children.Add(actionsLayout);

            noteFrame.Content = noteLayout;
            notesListLayout.Children.Add(noteFrame);
        }

        private async void AddNote_Clicked(object sender, EventArgs e)
        {
            // Запрашиваем заголовок записи
            string title = await DisplayPromptAsync(
                "Новая запись",
                "Введите заголовок записи",
                "Далее",
                "Отмена");

            if (string.IsNullOrWhiteSpace(title))
                return;

            // Запрашиваем содержание записи
            string content = await DisplayPromptAsync(
                "Содержание записи",
                "Опишите свои наблюдения",
                "Сохранить",
                "Отмена");

            if (string.IsNullOrWhiteSpace(content))
                return;

            // Создаем новую запись
            var newNote = new PlantNote
            {
                Title = title,
                Content = content,
                CreatedAt = DateTime.Now
            };

            // Добавляем запись в список
            if (_plant.Notes == null)
                _plant.Notes = new List<PlantNote>();

            _plant.Notes.Add(newNote);

            // Сохраняем изменения
            _saveCallback?.Invoke();

            // Обновляем UI
            LoadNotes();

            // Показываем уведомление
            await DisplayAlert("Успех", "Запись добавлена", "OK");
        }

        private async void EditNote_Clicked(PlantNote note)
        {
            // Запрашиваем новый заголовок записи
            string newTitle = await DisplayPromptAsync(
                "Изменение записи",
                "Изменить заголовок",
                "Далее",
                "Отмена",
                initialValue: note.Title);

            if (string.IsNullOrWhiteSpace(newTitle))
                return;

            // Запрашиваем новое содержание записи
            string newContent = await DisplayPromptAsync(
                "Изменение записи",
                "Изменить содержание",
                "Сохранить",
                "Отмена",
                initialValue: note.Content);

            if (string.IsNullOrWhiteSpace(newContent))
                return;

            // Обновляем запись
            note.Title = newTitle;
            note.Content = newContent;

            // Сохраняем изменения
            _saveCallback?.Invoke();

            // Обновляем UI
            LoadNotes();

            // Показываем уведомление
            await DisplayAlert("Успех", "Запись обновлена", "OK");
        }

        private async void DeleteNote_Clicked(PlantNote note)
        {
            // Запрашиваем подтверждение удаления
            bool confirm = await DisplayAlert(
                "Удаление записи",
                "Вы уверены, что хотите удалить эту запись?",
                "Да, удалить",
                "Отмена");

            if (!confirm)
                return;

            // Удаляем запись из списка
            _plant.Notes.Remove(note);

            // Сохраняем изменения
            _saveCallback?.Invoke();

            // Обновляем UI
            LoadNotes();

            // Показываем уведомление
            await DisplayAlert("Успех", "Запись удалена", "OK");
        }
    }
}