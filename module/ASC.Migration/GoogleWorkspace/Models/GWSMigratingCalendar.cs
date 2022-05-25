using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

using Ical.Net;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GwsMigratingCalendar : MigratingCalendar
    {
        public override int CalendarsCount => calendars.Count;

        public override int EventsCount => calendars.Values.SelectMany(c => c.SelectMany(x=>x.Events)).Count();
        public override string ModuleName => MigrationResource.ModuleNameCalendar;

        public override void Parse()
        {
            //var calPath = Path.Combine(rootFolder, "Calendar");
            //if (!Directory.Exists(calPath)) return;

            //var calFiles = Directory.GetFiles(calPath, "*.ics");
            //if (!calFiles.Any()) return;

            //foreach (var calFile in calFiles)
            //{
            //    var events = DDayICalParser.DeserializeCalendar(File.ReadAllText(calFile));
            //    calendars.Add(Path.GetFileNameWithoutExtension(calFile), events);
            //}
        }

        public override void Migrate()
        {
            if (!ShouldImport) return;

            // \portals\module\ASC.Api\ASC.Api.Calendar\CalendarApi.cs#L2311
            throw new NotImplementedException();
        }

        public GwsMigratingCalendar(string rootFolder, Action<string, Exception> log) : base(log)
        {
            this.rootFolder = rootFolder;
        }

        private string rootFolder;
        private Dictionary<string, CalendarCollection> calendars = new Dictionary<string, CalendarCollection>();
    }
}
