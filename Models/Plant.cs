
using System;
using System.Collections.Generic;
using System.Text;

namespace TestAppB.Models
{
    public class Plant
    {
        public string Name { get; set; }
        public bool IsWatered { get; set; }
        public DateTime LastWatered { get; set; }
        public int SkinIndex { get; set; } = 0; // Индекс выбранного скина (0-4)
        public List<PlantNote> Notes { get; set; } = new List<PlantNote>();
    }

    public class PlantNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Title { get; set; }
    }
}