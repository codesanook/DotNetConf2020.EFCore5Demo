using DotNetConf2020.EFCore5Demo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetConf2020.EFCore5Demo
{
    public static class Program
    {
        public static async Task Main()
        {
            // Demo 1 Basic EF, diagnosis
            await BasicEFAndDiagnosisDemoAsync();

            // Demo  2
            //await ManyToManyWithoutLinkModelDemoAsync();

            // Demo 3
            // await ManyToManyExplicitLinkDemoAsyn();

            // Demo 4
            // modelBuilder.Entity<RssBlog>()
            //await TablePerHierarchyDemoAsync();

            // Demo 5
            // modelBuilder.Entity<RssBlog>().ToTable("RssBlogs")
            // await TablePerTypeDemoAsync();
        }

        private static async Task BasicEFAndDiagnosisDemoAsync()
        {
            var dbContext = CreateDbContext();

            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            var user = new User() { Username = "ponggung", Email = "ponggung@codesanook.com" };
            dbContext.Users.AddRange(admin, user);

            var blog = new Blog() { Name = "EF", Description = "Blog about EF", CreatedBy = admin };
            var article1 = new Article() { Title = "EF Mapping", Content = "Learn EF Mapping", Author = user };
            var article2 = new Article() { Title = "EF Eager loading", Content = "Learn EF Eager loading", Author = user };

            blog.Articles.Add(article1);
            blog.Articles.Add(article2);

            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync(); // persist db

            // query projection
            var articleTitlesWithMethodSyntax = dbContext.Articles
                .Where(p => p.Blog.Name == "EF")
                .Select(p => p.Title);

            // Console.WriteLine(articleTitlesWithMethodSyntax.ToQueryString());

            // Iterate the result
            foreach (var title in articleTitlesWithMethodSyntax)
            {
                Console.WriteLine($"article title: {title}");
            }

            // Update Titlle
            article1.Title = "EF is fun";
            await dbContext.SaveChangesAsync();

            var articleTitleWithQuerySyntax =
                from p in dbContext.Articles
                where p.Blog.Name == "EF"
                select p.Title;

            Console.WriteLine("\nAfter updated");
            foreach (var title in articleTitleWithQuerySyntax)
            {
                Console.WriteLine($"article title: {title}");
            }
        }

        private static async Task ManyToManyWithoutLinkModelDemoAsync()
        {
            var context = CreateDbContext();

            // Prepare data
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            context.Users.Add(admin);

            const string tagName = "ef";
            var tag = new Tag() { Name = tagName };
            context.Tags.Add(tag);

            var blog = new Blog() { Name = "Entity Framework Core", Description = "Blog about EF", CreatedBy = admin };
            context.Blogs.Add(blog);

            // Create a new article
            var article = new Article()
            {
                Title = "Eager loading in Enity Framework Core",
                Content = "Learn EF Eager loading",
                Author = admin,
                Blog = blog,
            };

            // Add tag to the article
            article.Tags.Add(tag);

            // Save an article
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var articleTitlesWithGivenTag = context.Articles
                .Where(p => p.Tags.Any(t => t.Name == tagName))
                .Select(p => p.Title);

            foreach (var title in articleTitlesWithGivenTag)
            {
                Console.WriteLine($"article title: {title}");
            }
        }

        private static async Task ManyToManyExplicitLinkDemoAsyn()
        {
            var context = CreateDbContext();

            // Prepare data
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            context.Users.Add(admin);

            var tag = new Tag() { Name = "ef" };
            context.Tags.Add(tag);

            var blog = new Blog() { Name = "Entity Framework Core", Description = "Blog about EF", CreatedBy = admin };
            context.Blogs.Add(blog);

            // Create a new article
            var article = new Article()
            {
                Title = "Eager loading in Enity Framework Core",
                Content = "Learn EF Eager loading",
                Author = admin,
                Blog = blog,
            };

            // Add association
            var articleTag = new ArticleTag() { Article = article, Tag = tag, CreatedUtc = DateTime.UtcNow.AddMonths(-1) };

            context.Articles.Add(article);
            context.Set<ArticleTag>().Add(articleTag);
            await context.SaveChangesAsync();

            var tagCreatedFrom = DateTime.UtcNow.AddMonths(-2);
            var articleTitlesCreatedFromDate = context.Articles
                .Where(p => p.ArticleTags.Any(at => at.CreatedUtc >= tagCreatedFrom))
                .Select(p => p.Title);

            foreach (var title in articleTitlesCreatedFromDate)
            {
                Console.WriteLine($"article title: {title}");
            }
        }

        private static async Task TablePerHierarchyDemoAsync()
        {
            var dbContext = CreateDbContext();
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            dbContext.Users.Add(admin);

            // Disciminator => Blog
            var blog = new Blog() { Name = "Entity Framework Core", Description = "Blog about EF", CreatedBy = admin };
            // Disciminator => Blog 
            var rssBlog = new RssBlog()
            {
                Name = "RSS Entity Framework Core",
                Description = "RSS Blog about EF",
                CreatedBy = admin,
                Url = "Codesanook.com"
            };

            dbContext.Blogs.AddRange(blog, rssBlog);
            dbContext.SaveChanges();

            // Iterate the result, Use .Set
            foreach (var blogItem in dbContext.Set<RssBlog>().Select(b => new { b.Name, b.Url }))
            {
                Console.WriteLine($"blog name: {blogItem.Name}, URL: {blogItem.Url}");
            }
        }

        private static async Task TablePerTypeDemoAsync()
        {
            var dbContext = CreateDbContext();
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            dbContext.Users.Add(admin);

            // new table Blogs
            var blog = new Blog() { Name = "Entity Framework Core", Description = "Blog about EF", CreatedBy = admin };
            // new Table RssBlogs
            var rssBlog = new RssBlog()
            {
                Name = "RSS Entity Framework Core",
                Description = "RSS Blog about EF",
                CreatedBy = admin,
                Url = "Codesanook.com"
            };
            dbContext.Blogs.AddRange(blog, rssBlog);
            dbContext.SaveChanges();

            // Iterate the result
            foreach (var blogItem in dbContext.Set<RssBlog>().Select(b => new { b.Name, b.Url }))
            {
                Console.WriteLine($"blog name: {blogItem.Name}, URL: {blogItem.Url}");
            }
        }

        private static BlogDbContext CreateDbContext()
        {
            // DotNetConf2020.EFCore5Demo\DotNetConf2020.EFCore5Demo\bin\Debug\net5.0
            const string databaseFileName = "ef5-demo.db";
            if (File.Exists(databaseFileName))
            {
                File.Delete(databaseFileName);
            }

            var connectionString = $"Data Source={databaseFileName}";
            var builder = new DbContextOptionsBuilder<BlogDbContext>()
                .UseSqlite(connectionString)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

            var context = new BlogDbContext(builder.Options);
            context.Database.EnsureCreated(); // Create a database and tables
            return context;
        }
    }
}
