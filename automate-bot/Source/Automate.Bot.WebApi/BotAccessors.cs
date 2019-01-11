using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automate.Bot.WebApi
{
    public class BotAccessors
    {
        public static string JobLogAccessorKey { get; } = $"{nameof(BotAccessors)}.JobLogAccessor";

        public BotAccessors(JobState jobState)
        {
            JobState = jobState;
        }

        public IStatePropertyAccessor<JobLog> JobLogAccessor { get; set; }

        public JobState JobState { get; set; }
    }
}
