using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automate.Bot.WebApi
{
    public enum JobStatusTypes
    {
        Running,
        Completed,
        Paused
    }

    public class Job
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public ConversationReference ConversationReference { get; set; }
        public JobStatusTypes Status { get; set; }
    }
}
