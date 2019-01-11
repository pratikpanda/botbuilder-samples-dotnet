using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Automate.Bot.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration;
        public IHostingEnvironment Environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<AutomateBot>(options =>
            {
                var botFilePath = Configuration.GetSection("botFilePath")?.Value;
                var botFileSecret = Configuration.GetSection("botFileSecret")?.Value;
                var botConfiguration = BotConfiguration.Load(Path.Combine(Environment.ContentRootPath, botFilePath), botFileSecret);
                services.AddSingleton(serviceProvider =>
                {
                    return botConfiguration;
                });

                GetCurrentEndpoint(options, botConfiguration);

                var globalExceptionHandler = new GlobalExceptionHandler(Environment);
                options.OnTurnError = globalExceptionHandler.OnExceptionAsync;
            });

            services.AddSingleton(serviceProvider =>
            {
                IStorage storage = new MemoryStorage();
                var jobState = new JobState(storage);
                var botAccessors = new BotAccessors(jobState);
                botAccessors.JobLogAccessor = botAccessors.JobState.CreateProperty<JobLog>(BotAccessors.JobLogAccessorKey);
                return botAccessors;
            });
        }

        private void GetCurrentEndpoint(BotFrameworkOptions options, BotConfiguration botConfiguration)
        {
            if (Environment.IsDevelopment())
            {
                var service = botConfiguration.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == "development");
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
                }
                SetCredentials(options, endpointService);
            }
            else
            {
                var service = botConfiguration.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == "production");
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain a production endpoint.");
                }
                SetCredentials(options, endpointService);
            }
        }

        private static void SetCredentials(Microsoft.Bot.Builder.Integration.BotFrameworkOptions options, EndpointService endpointService)
        {
            options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseBotFramework();
        }
    }
}
