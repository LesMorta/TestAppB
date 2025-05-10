using System;
using System.Collections.Generic;
using System.Text;
using TestAppB.Models;

namespace TestAppB.Services
{
    public static class AchievementService
    {
        public static List<Achievement> AllAchievements = new List<Achievement>
        {
            // Існуючі досягнення
            new Achievement { Id = "first_water", Title = "Перший полив", Description = "Полий рослину вперше", Icon = "achieve1.png" },
            new Achievement { Id = "ten_waters", Title = "Десять поливів", Description = "Полий рослину 10 разів", Icon = "achieve2.png" },
            new Achievement { Id = "open_app_5", Title = "Назад у гру", Description = "Відкрий додаток 5 разів", Icon = "achieve3.png" },
            
            // Нові досягнення
            // Базові досягнення поливу
            new Achievement { Id = "fifty_waters", Title = "Досвідчений садівник", Description = "Полий рослини 50 разів", Icon = "achieve4.png" },
            new Achievement { Id = "hundred_waters", Title = "Майстер поливу", Description = "Полий рослини 100 разів", Icon = "achieve5.png" },
            new Achievement { Id = "five_hundred_waters", Title = "Легенда садівництва", Description = "Полий рослини 500 разів", Icon = "achieve6.png" },
            
            // Досягнення за кількість рослин
            new Achievement { Id = "three_plants", Title = "Початківець колекціонер", Description = "Додай 3 рослини до колекції", Icon = "achieve7.png" },
            new Achievement { Id = "ten_plants", Title = "Домашній сад", Description = "Додай 10 рослин до колекції", Icon = "achieve8.png" },
            new Achievement { Id = "twenty_plants", Title = "Власник оранжереї", Description = "Додай 20 рослин до колекції", Icon = "achieve9.png" },
            
            // Досягнення за регулярність
            new Achievement { Id = "daily_3", Title = "Щоденний догляд", Description = "Заходь у додаток 3 дні поспіль", Icon = "achieve10.png" },
            new Achievement { Id = "daily_7", Title = "Тиждень турботи", Description = "Заходь у додаток 7 днів поспіль", Icon = "achieve11.png" },
            new Achievement { Id = "daily_30", Title = "Місяць з рослинами", Description = "Заходь у додаток 30 днів поспіль", Icon = "achieve12.png" },
            
            // Досягнення за поливи у різний час
            new Achievement { Id = "morning_water", Title = "Ранковий полив", Description = "Полий рослину зранку (6:00-10:00)", Icon = "achieve13.png" },
            new Achievement { Id = "evening_water", Title = "Вечірній полив", Description = "Полий рослину ввечері (18:00-22:00)", Icon = "achieve14.png" },
            new Achievement { Id = "night_owl", Title = "Нічна сова", Description = "Полий рослину вночі (23:00-5:00)", Icon = "achieve15.png" },
            
            // Спеціальні досягнення
            new Achievement { Id = "weekend_care", Title = "Вихідні з рослинами", Description = "Полий рослину в суботу та неділю одного тижня", Icon = "achieve16.png" },
            new Achievement { Id = "water_all", Title = "Тотальний полив", Description = "Полий усі рослини за один раз", Icon = "achieve17.png" },
            new Achievement { Id = "rename_plant", Title = "Нове ім'я", Description = "Перейменуй рослину", Icon = "achieve18.png" },
            new Achievement { Id = "delete_plant", Title = "Прощавай, друже", Description = "Видали рослину зі своєї колекції", Icon = "achieve19.png" },
            
            // Сезонні досягнення
            new Achievement { Id = "spring_water", Title = "Весняне пробудження", Description = "Полий рослину навесні", Icon = "achieve20.png" },
            new Achievement { Id = "summer_water", Title = "Літня спека", Description = "Полий рослину влітку", Icon = "achieve21.png" },
            new Achievement { Id = "autumn_water", Title = "Осінній догляд", Description = "Полий рослину восени", Icon = "achieve22.png" },
            new Achievement { Id = "winter_water", Title = "Зимова турбота", Description = "Полий рослину взимку", Icon = "achieve23.png" }
        };
    }
}
