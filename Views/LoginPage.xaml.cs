using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Newtonsoft.Json;
using System.Net.Http;

namespace TestAppB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyBJ7RL0sa-o7EXP5LxbaITq5UP-LTwSH3s";
        private bool _isLoading = false;

        public LoginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                string email = emailEntry.Text?.Trim();
                string password = passwordEntry.Text;

                // Валідація даних
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Помилка", "Будь ласка, заповніть всі поля", "OK");
                    return;
                }

                _isLoading = true;
                ((Button)sender).Text = "Завантаження...";
                ((Button)sender).IsEnabled = false;

                var client = new HttpClient();
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Успішно", "Вхід виконано", "OK");
                    Application.Current.MainPage = new MainPage();
                }
                else
                {
                    await DisplayAlert("Помилка", "Невірний email або пароль", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                ((Button)sender).Text = "Увійти";
                ((Button)sender).IsEnabled = true;
            }
        }

        private async void GoToRegister_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}