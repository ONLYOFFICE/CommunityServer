using System.Threading.Tasks;
using ASC.Mail.Aggregator.Common;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    public class TaskData
    {
        public TaskData(MailBox mailBox, Task task)
        {
            Mailbox = mailBox;
            Task = task;
        }

        public MailBox Mailbox { get; private set; }

        public Task Task { get; private set; }
    }
}
