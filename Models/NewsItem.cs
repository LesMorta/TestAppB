using System;
using System.Collections.Generic;
using System.Text;

namespace TestAppB.Models
{
    public class NewsItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public DateTime PublishedDate { get; set; }
        public bool IsFeatured { get; set; }
        public List<string> Tags { get; set; }
        public int LikesCount { get; set; }
        public string Author { get; set; }
    }
}
