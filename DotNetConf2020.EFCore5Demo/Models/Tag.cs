using System.Collections.Generic;

namespace DotNetConf2020.EFCore5Demo.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Article> Articles { get; set; }

        // This does not required
        public ICollection<ArticleTag> ArticleTags { get; set; }
    }
}
