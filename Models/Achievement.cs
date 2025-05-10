using System;
using System.Collections.Generic;
using System.Text;

namespace TestAppB.Models
{
    public class Achievement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
        public string Icon { get; set; }
    }
}
