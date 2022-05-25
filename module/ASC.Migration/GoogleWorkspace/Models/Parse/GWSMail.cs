using System.Collections.Generic;

using MimeKit;

namespace ASC.Migration.GoogleWorkspace.Models.Parse
{
    class GwsMail
    {
        public string ParentFolder { get; set; }
        public List<MimeMessage> Message { get; set; }
    }
}
