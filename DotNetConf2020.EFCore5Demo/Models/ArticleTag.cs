using System;

namespace DotNetConf2020.EFCore5Demo.Models
{
    public class ArticleTag
    {
        public Article Article { get; set; }
        public Tag Tag { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}
