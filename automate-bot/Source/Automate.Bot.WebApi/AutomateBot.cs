using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Automate.Bot.WebApi
{
    public class AutomateBot : IBot
    {
        private BotAccessors botAccessors;

        public AutomateBot(BotAccessors botAccessors)
        {
            this.botAccessors = botAccessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.ToLower().Split(" ");
                if (text.Length > 0)
                {
                    switch (text[0])
                    {
                        case "start":
                            await StartJob(turnContext);
                            break;
                        case "show":
                            await ShowJobs(turnContext);
                            break;
                        case "stop":
                            var jobId = text[1];
                            await StopJob(turnContext, jobId);
                            break;
                    }
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected.");
            }
        }

        private async Task ShowJobs(ITurnContext turnContext)
        {
            var jobLog = await botAccessors.JobLogAccessor.GetAsync(turnContext, () => new JobLog());
            await turnContext.SendActivityAsync("Jobs:");
            foreach (var item in jobLog)
            {
                await turnContext.SendActivityAsync($"**Id**: {item.Key}{Environment.NewLine}**Status**: {item.Value.Status}");
            }
        }

        private async Task StopJob(ITurnContext turnContext, string jobId)
        {
            var guid = new Guid(jobId);
            var jobLog = await botAccessors.JobLogAccessor.GetAsync(turnContext, () => new JobLog());            
            await turnContext.Adapter.ContinueConversationAsync("1", jobLog[guid].ConversationReference, Callback(jobId), default(CancellationToken));           
        }

        private BotCallbackHandler Callback(string jobId)
        {
            return async (turnContext, cancellationToken) =>
            {
                var jobLog = await botAccessors.JobLogAccessor.GetAsync(turnContext, () => new JobLog());
                var guid = new Guid(jobId);
                jobLog[guid].Status = JobStatusTypes.Completed;
                await botAccessors.JobLogAccessor.SetAsync(turnContext, jobLog);
                await botAccessors.JobState.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync($"Job **{jobId}** completed.");
            };
        }

        private async Task StartJob(ITurnContext turnContext)
        {
            await Task.Run(async () =>
            {
                var job = await CreateJob(turnContext);
                var jobLog = await botAccessors.JobLogAccessor.GetAsync(turnContext, () => new JobLog());
                jobLog[job.Id] = job;
                await botAccessors.JobLogAccessor.SetAsync(turnContext, jobLog);
                await botAccessors.JobState.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync($"Job **{job.Id}** started.");
            });
        }

        private Task<Job> CreateJob(ITurnContext turnContext)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                ConversationReference = turnContext.Activity.GetConversationReference(),
                TimeStamp = DateTime.UtcNow,
                Status = JobStatusTypes.Running
            };
            return Task.FromResult(job);
        }
    }
}
