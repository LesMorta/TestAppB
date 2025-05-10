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
    public partial class RegisterPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyBJ7RL0sa-o7EXP5LxbaITq5UP-LTwSH3s";
        private bool _isLoading = false;

        public RegisterPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                string name = nameEntry?.Text?.Trim();
                string email = emailEntry?.Text?.Trim();
                string password = passwordEntry?.Text;
                string confirmPassword = confirmPasswordEntry?.Text;

                // Валідація даних
                if (string.IsNullOrEmpty(name))
                {
                    await DisplayAlert("Помилка", "Будь ласка, введіть ваше ім'я", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Помилка", "Будь ласка, введіть email", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Помилка", "Будь ласка, введіть пароль", "OK");
                    return;
                }

                if (password != confirmPassword)
                {
                    await DisplayAlert("Помилка", "Паролі не співпадають", "OK");
                    return;
                }

                if (password.Length < 6)
                {
                    await DisplayAlert("Помилка", "Пароль має містити не менше 6 символів", "OK");
                    return;
                }

                if (!termsCheckBox.IsChecked)
                {
                    await DisplayAlert("Помилка", "Будь ласка, прийміть умови використання", "OK");
                    return;
                }

                _isLoading = true;
                ((Button)sender).Text = "Створення акаунту...";
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

                var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Опціонально: Зберегти ім'я користувача на пристрої
                    Application.Current.Properties["UserName"] = name;
                    await Application.Current.SavePropertiesAsync();

                    await DisplayAlert("Успіх", "Акаунт успішно створено!", "OK");
                    await Navigation.PopAsync(); // Повернення на сторінку входу
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                    var error = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorResponse["error"].ToString());
                    string errorMessage = error["message"].ToString();

                    string userErrorMessage = "Не вдалося зареєструватися";

                    // Переклад помилок Firebase на зрозумілу для користувача мову
                    if (errorMessage.Contains("EMAIL_EXISTS"))
                    {
                        userErrorMessage = "Цей email вже використовується іншим акаунтом";
                    }
                    else if (errorMessage.Contains("WEAK_PASSWORD"))
                    {
                        userErrorMessage = "Пароль занадто слабкий";
                    }
                    else if (errorMessage.Contains("INVALID_EMAIL"))
                    {
                        userErrorMessage = "Некоректний формат email";
                    }

                    await DisplayAlert("Помилка", userErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                ((Button)sender).Text = "Зареєструватися";
                ((Button)sender).IsEnabled = true;
            }
        }

        private async void BackToLogin_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}