using System.Collections.Generic;

namespace DotNetConf2020.EFCore5Demo.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public User CreatedBy { get; set; }

        public Blog() => Articles = new List<Article>();
        public ICollection<Article> Articles { get; set; }
    }
}
