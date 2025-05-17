using System;
using System.Collections.Generic;

namespace TestAppB.Models
{
    public class Plant
    {
        public string Name { get; set; }
        public bool IsWatered { get; set; }
        public DateTime LastWatered { get; set; }
        public int SkinIndex { get; set; } = 0; // Индекс выбранного скина (0-4)
        public List<PlantNote> Notes { get; set; } = new List<PlantNote>(); // Инициализация сразу при создании
    }

    public class PlantNote
    {
        public PlantNote()
        {
            // Устанавливаем значения по умолчанию в конструкторе
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}