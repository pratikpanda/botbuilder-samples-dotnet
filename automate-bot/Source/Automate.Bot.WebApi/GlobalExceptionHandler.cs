using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automate.Bot.WebApi
{
    public class GlobalExceptionHandler
    {
        private readonly IHostingEnvironment environment;

        public GlobalExceptionHandler(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task OnExceptionAsync(ITurnContext turnContext, Exception exception)
        {
            await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            if (environment.IsDevelopment())
            {
                await turnContext.TraceActivityAsync("Exception", exception);
            }
        }
    }
}
