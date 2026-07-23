using System.Collections.Generic;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Models.ViewModels
{
    public class NewsViewModel
    {
        public List<NewsArticle> Articles { get; set; } = new();
        public List<CareerEvent> Events { get; set; } = new();
    }
}
