using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetConf2020.EFCore5Demo.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public User Author { get; set; }

        [Required] // It will create inner join query, without it we will get left outer join
        public virtual Blog Blog { get; set; }

        public Article() => Tags = new List<Tag>();
        public ICollection<Tag> Tags { get; set; }

        public ICollection<ArticleTag> ArticleTags { get; set; }
    }
}
