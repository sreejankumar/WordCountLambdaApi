using System;
using System.IO;
using Api.Core.Commands;
using Api.Core.Data;
using Api.Core.Middleware;
using Logging.Extensions;
using Logging.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WordCount.Api.Core.Commands;
using WordCount.Api.Core.Configuration;
using WordCount.Api.Core.Data.ExternalService;
using WordCount.Api.Core.Data.Service;
using WordCount.Api.Core.Services;

namespace WordCount.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        /// This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Commands
            services.AddCommandsFromAssembly();
            services.AddScoped<GetWordCountCommand>();

            services.AddScoped<IWordProcessorService, WordProcessorService>();

            services.AddHttpClient();

            services.Configure<DefinitionApiConfiguration>(
                Configuration.GetSection(DefinitionApiConfiguration.DefinitionApiConfigurationPrefix));
            services.AddScoped<IDefinitionsApiService, DefinitionsApiService>();

            services.AddScoped<IWordCounterService, WordCounterService>();

            services.AddConsoleLogging();
            services.AddScoped<IContextLogModel, ApiLogContext>();

            AddSwagger(services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<ErrorHandlingMiddleware>();


            //app.UseMiddleware(new CustomResponseHeadersBuilder().
            //    AddCustomResponseHeadersPolicy(new[] { "GET" })
            //    .AddCustomHeader("x-backend-server", Environment.MachineName));

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Word Count API (v1.0)"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                    });
            });
        }

        /// <summary>
        /// Adds Swagger documentation for the web api 
        /// </summary>
        /// <param name="services"></param>
        public void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Word Count API",
                    Version = "v1.0.0",
                    Description =
                        "Count the number of instances of each word in a text file and find definitions if found",
                    Contact = new OpenApiContact()
                    {
                        Name = "Sreejan Kumar",
                        Url = new Uri("https://github.com/sreejankumar")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "License",
                        Url = new Uri("https://github.com/sreejankumar/Api.Core/blob/main/LICENSE")
                    }
                });

                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    "WordCount.Api.xml"));
            });
        }
    }
}
