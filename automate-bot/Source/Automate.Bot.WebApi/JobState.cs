using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automate.Bot.WebApi
{
    public class JobState : BotState
    {
        public const string StorageKey = "AutomateBot.JobState";

        public JobState(IStorage storage) : base(storage, StorageKey)
        {
        }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            return StorageKey;
        }
    }
}
