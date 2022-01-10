namespace Body4U.Article
{
    using Body4U.Article.Data;
    using Body4U.Article.Messages;
    using Body4U.Article.Services.Article;
    using Body4U.Article.Services.Comment;
    using Body4U.Article.Services.Service;
    using Body4U.Article.Services.Trainer;
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddWebService<ArticleDbContext>(this.Configuration)
                .AddTransient<ExceptionMiddleware>()
                .AddCloudinary(this.Configuration)
                .AddTransient<ITrainerService, TrainerService>()
                .AddTransient<IArticleService, ArticleService>()
                .AddTransient<ICommentService, CommentService>()
                .AddTransient<IServiceService, ServiceService>()
                .AddMessaging(this.Configuration,
                              false,
                              typeof(CreateTrainerConsumer),
                              typeof(DeleteTrainerConsumer),
                              typeof(EdiTrainerNamesConsumer));

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseWebService(env)
                .UseCustomStaticFiles()
                .Initialize(); 
    }
}
