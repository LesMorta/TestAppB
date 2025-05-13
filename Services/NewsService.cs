using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestAppB.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TestAppB.Services
{
    public class NewsService
    {
        private const string NewsFileName = "news_data.json";
        private const string LikedNewsKey = "liked_news_ids";
        private List<NewsItem> cachedNews;

        // Загрузка новостей из файла
        public async Task<List<NewsItem>> GetNewsAsync()
        {
            if (cachedNews != null)
            {
                return cachedNews;
            }

            try
            {
                // Проверяем наличие файла в локальном хранилище
                string localPath = Path.Combine(FileSystem.AppDataDirectory, NewsFileName);

                if (File.Exists(localPath))
                {
                    // Если файл существует, читаем из него
                    string json = File.ReadAllText(localPath);
                    cachedNews = JsonConvert.DeserializeObject<List<NewsItem>>(json);
                }
                else
                {
                    // Если файла нет, читаем встроенный файл ресурсов
                    cachedNews = await LoadDefaultNewsAsync();

                    // И сохраняем его в локальное хранилище
                    await SaveNewsToFileAsync(cachedNews);
                }

                // Сортируем по дате (новые сверху)
                return cachedNews.OrderByDescending(n => n.PublishedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading news: {ex.Message}");
                return new List<NewsItem>();
            }
        }

        // Загрузка новостей из встроенного ресурса
        private async Task<List<NewsItem>> LoadDefaultNewsAsync()
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(NewsService)).Assembly;
                Stream stream = assembly.GetManifestResourceStream("TestAppB.Resources.news_data.json");

                using (var reader = new StreamReader(stream))
                {
                    string json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<List<NewsItem>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default news: {ex.Message}");

                // Если не удалось загрузить встроенный файл, возвращаем тестовые данные
                return GetFallbackNews();
            }
        }

        // Обновление новостей из встроенного ресурса
        public async Task<bool> RefreshNewsFromEmbeddedResourceAsync()
        {
            try
            {
                var defaultNews = await LoadDefaultNewsAsync();
                cachedNews = defaultNews;
                await SaveNewsToFileAsync(defaultNews);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Сохранение новостей в файл
        private async Task SaveNewsToFileAsync(List<NewsItem> news)
        {
            try
            {
                string json = JsonConvert.SerializeObject(news, Formatting.Indented);
                string localPath = Path.Combine(FileSystem.AppDataDirectory, NewsFileName);
                File.WriteAllText(localPath, json);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving news: {ex.Message}");
            }
        }

        // Добавление лайка новости
        public async Task LikeNewsAsync(string newsId)
        {
            var likedNewsIds = GetLikedNewsIds();

            if (!likedNewsIds.Contains(newsId))
            {
                likedNewsIds.Add(newsId);
                SaveLikedNewsIds(likedNewsIds);

                // Обновляем счетчик лайков в файле
                var news = await GetNewsAsync();
                var newsItem = news.FirstOrDefault(n => n.Id == newsId);

                if (newsItem != null)
                {
                    newsItem.LikesCount++;
                    await SaveNewsToFileAsync(news);
                }
            }
        }

        // Удаление лайка
        public async Task UnlikeNewsAsync(string newsId)
        {
            var likedNewsIds = GetLikedNewsIds();

            if (likedNewsIds.Contains(newsId))
            {
                likedNewsIds.Remove(newsId);
                SaveLikedNewsIds(likedNewsIds);

                // Обновляем счетчик лайков в файле
                var news = await GetNewsAsync();
                var newsItem = news.FirstOrDefault(n => n.Id == newsId);

                if (newsItem != null && newsItem.LikesCount > 0)
                {
                    newsItem.LikesCount--;
                    await SaveNewsToFileAsync(news);
                }
            }
        }

        // Проверка, поставлен ли лайк новости
        public bool IsNewsLiked(string newsId)
        {
            return GetLikedNewsIds().Contains(newsId);
        }

        // Получение списка ID новостей, которым пользователь поставил лайк
        private List<string> GetLikedNewsIds()
        {
            if (Preferences.ContainsKey(LikedNewsKey))
            {
                string json = Preferences.Get(LikedNewsKey, "[]");
                return JsonConvert.DeserializeObject<List<string>>(json);
            }

            return new List<string>();
        }

        // Сохранение списка ID понравившихся новостей
        private void SaveLikedNewsIds(List<string> likedNewsIds)
        {
            string json = JsonConvert.SerializeObject(likedNewsIds);
            Preferences.Set(LikedNewsKey, json);
        }

        // Fallback данные на случай, если не удастся загрузить новости
        private List<NewsItem> GetFallbackNews()
        {
            return new List<NewsItem>
            {
                new NewsItem
                {
                    Id = "1",
                    Title = "5 правил успешного полива комнатных растений",
                    Summary = "Правильный полив - залог здоровья растений. Узнайте оптимальные способы полива.",
                    Content = "Правило 1: Проверяйте почву перед поливом. Верхний слой должен просохнуть на 1-2 см.\n\n" +
                              "Правило 2: Используйте воду комнатной температуры. Холодная вода может вызвать шок у растений.\n\n" +
                              "Правило 3: Поливайте утром или вечером, но не в жаркий полдень.\n\n" +
                              "Правило 4: Разные растения требуют разного режима полива. Суккуленты нуждаются в меньшем количестве воды, чем папоротники.\n\n" +
                              "Правило 5: Не допускайте застоя воды в поддоне - это может привести к гниению корней.",
                    ImageUrl = "watering_tips.png",
                    Category = "Советы",
                    PublishedDate = DateTime.Now.AddDays(-2),
                    IsFeatured = true,
                    Tags = new List<string> { "полив", "уход", "начинающим" },
                    LikesCount = 24,
                    Author = "Мария Садовник"
                },
                new NewsItem
                {
                    Id = "2",
                    Title = "ТОП-10 неприхотливых растений для начинающих",
                    Summary = "Только начинаете заниматься растениями? Эти 10 видов отлично подойдут для старта.",
                    Content = "1. Сансевиерия (щучий хвост) - практически неубиваемое растение, требует полива раз в 2-3 недели.\n\n" +
                              "2. Потос - вьющееся растение с красивыми листьями, хорошо растет даже при недостатке света.\n\n" +
                              "3. Драцена - неприхотливое растение с длинными листьями, хорошо очищает воздух.\n\n" +
                              "4. Хлорофитум - быстрорастущее растение, которое легко размножается отводками.\n\n" +
                              "5. Спатифиллум - цветущее растение, которое показывает когда ему нужен полив (листья опускаются).\n\n" +
                              "6. Замиокулькас - растение с блестящими листьями, выносит продолжительную засуху.\n\n" +
                              "7. Алоэ - суккулент с лечебными свойствами, требует яркого света и редкого полива.\n\n" +
                              "8. Кактусы - разнообразные по форме растения, которым нужен яркий свет и минимальный полив.\n\n" +
                              "9. Фикус каучуконосный - растение с блестящими кожистыми листьями, неприхотливое в уходе.\n\n" +
                              "10. Крассула (денежное дерево) - суккулент с круглыми листочками, символ финансового благополучия.",
                    ImageUrl = "beginner_plants.png",
                    Category = "Растения",
                    PublishedDate = DateTime.Now.AddDays(-5),
                    IsFeatured = true,
                    Tags = new List<string> { "начинающим", "неприхотливые", "список" },
                    LikesCount = 42,
                    Author = "Иван Цветов"
                },
                new NewsItem
                {
                    Id = "3",
                    Title = "Как спасти пересушенное растение",
                    Summary = "Забыли полить любимый цветок? Есть способы его реанимировать!",
                    Content = "Шаг 1: Пересушенное растение можно узнать по вялым, поникшим листьям и сухой почве, которая отстает от стенок горшка.\n\n" +
                              "Шаг 2: Перенесите растение в тень, чтобы уменьшить нагрузку и испарение влаги.\n\n" +
                              "Шаг 3: Для небольших растений можно применить метод погружения - поставьте горшок в таз с водой комнатной температуры на 15-30 минут. Вода должна доходить примерно до половины высоты горшка.\n\n" +
                              "Шаг 4: Для крупных растений аккуратно поливайте по краю горшка небольшими порциями, давая воде впитываться.\n\n" +
                              "Шаг 5: После восстановления влажности почвы можно слегка опрыскать листья, создав более влажный микроклимат.\n\n" +
                              "Шаг 6: В течение нескольких дней поддерживайте повышенную влажность воздуха вокруг растения.\n\n" +
                              "Шаг 7: Не удобряйте сразу после пересушки - дайте растению время восстановиться.\n\n" +
                              "Важно: некоторые растения могут не восстановиться после сильной пересушки. В таком случае, сохраните здоровые части для размножения.",
                    ImageUrl = "dry_plant_rescue.png",
                    Category = "Лайфхаки",
                    PublishedDate = DateTime.Now.AddDays(-10),
                    IsFeatured = false,
                    Tags = new List<string> { "спасение", "пересушка", "полив" },
                    LikesCount = 18,
                    Author = "Елена Флорист"
                }
            };
        }
    }
}