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
            // await BasicEFAndDiagnosisDemoAsync();

            // Demo  2
            //await ManyToManyWithoutLinkModelDemoAsync();

            // Demo 3
            await ManyToManyExplicitLinkDemoAsyn();

            // Demo 4
            await TablePerHierarchyDemoAsync();
        }

        private static async Task BasicEFAndDiagnosisDemoAsync()
        {
            var context = CreateDbContext();
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            var user = new User() { Username = "ponggung", Email = "ponggung@codesanook.com" };
            context.Users.AddRange(admin, user);

            var blog = new Blog() { Name = "EF", Description = "Blog about EF", CreatedBy = admin };
            var post1 = new Article() { Title = "EF Mapping", Content = "Learn EF Mapping", Author = user };
            var post2 = new Article() { Title = "EF Eager loading", Content = "Learn EF Eager loading", Author = user };

            blog.Articles.Add(post1);
            blog.Articles.Add(post2);

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            // query projection
            var postTitlesWithMethodSyntax = context.Articles
                .Where(p => p.Blog.Name == "EF")
                .Select(p => p.Title);

            var postTitleWithQuerySyntax =
                from p in context.Articles
                where p.Blog.Name == "EF"
                select p.Title;

            // Console.WriteLine(postTitlesWithMethodSyntax.ToQueryString());

            // Iterate the result
            foreach (var title in postTitlesWithMethodSyntax)
            {
                Console.WriteLine($"post title {title}");
            }

            foreach (var title in postTitleWithQuerySyntax)
            {
                Console.WriteLine($"post content {title}");
            }
        }

        private static async Task ManyToManyWithoutLinkModelDemoAsync()
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

            // Add tag to the article
            article.Tags.Add(tag);

            // Save an article
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            // query projection
            var postTitlesWithMethodSyntax = context.Articles
                .Where(p => p.Blog.Name == "EF")
                .Select(p => p.Title);

            var postTitleWithQuerySyntax =
                from p in context.Articles
                where p.Blog.Name == "EF"
                select p.Title;

            // Console.WriteLine(postTitlesWithMethodSyntax.ToQueryString());

            // Iterate the result
            foreach (var title in postTitlesWithMethodSyntax)
            {
                Console.WriteLine($"post title {title}");
            }

            foreach (var title in postTitleWithQuerySyntax)
            {
                Console.WriteLine($"post content {title}");
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
            context.ArticleTags.Add(articleTag);
            await context.SaveChangesAsync();

            var tagCreatedFrom = DateTime.UtcNow.AddMonths(-2);
            var postTitlesWithMethodSyntax = context.Articles
                .Where(p => p.ArticleTags.Any(at => at.CreatedUtc >= tagCreatedFrom))
                .Select(p => p.Title);

            foreach (var title in postTitlesWithMethodSyntax)
            {
                Console.WriteLine($"post title: {title}");
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
            var rssBlog = new RssBlog() { Name = "RSS Entity Framework Core", Description = "RSS Blog about EF", CreatedBy = admin };
            dbContext.Blogs.AddRange(blog, rssBlog);
            dbContext.SaveChanges();

            // Iterate the result
            foreach (var name in dbContext.Blogs.Select(b => b.Name))
            {
                Console.WriteLine($"blog title {name}");
            }
        }

        private static async Task TablePerTypeDemoAsync()
        {
            var dbContext = CreateDbContext();
            var admin = new User() { Username = "admin", Email = "admin@codesanook.com" };
            dbContext.Users.Add(admin);

            // Disciminator => Blog
            var blog = new Blog() { Name = "Entity Framework Core", Description = "Blog about EF", CreatedBy = admin };
            // Disciminator => Blog 
            var rssBlog = new RssBlog() { Name = "RSS Entity Framework Core", Description = "RSS Blog about EF", CreatedBy = admin };
            dbContext.Blogs.AddRange(blog, rssBlog);
            dbContext.SaveChanges();

            // Iterate the result
            foreach (var name in dbContext.Blogs.Select(b => b.Name))
            {
                Console.WriteLine($"blog title {name}");
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
