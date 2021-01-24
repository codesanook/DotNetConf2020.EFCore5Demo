using DotNetConf2020.EFCore5Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetConf2020.EFCore5Demo
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Article> Articles { get; set; }



        public DbSet<Tag> Tags { get; set; }

        // Explicitly association 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .HasMany(a => a.Tags)
                .WithMany(at => at.Articles)
                .UsingEntity<ArticleTag>(
                    at => at.HasOne(at => at.Tag).WithMany(a => a.ArticleTags), // Tag
                    at => at.HasOne(at => at.Article).WithMany(a => a.ArticleTags) // Article 
                );

            // Need to explicit tell EF we want a table for it
            //modelBuilder.Entity<RssBlog>();
            modelBuilder.Entity<RssBlog>().ToTable("RssBlogs");
        }
    }
}
