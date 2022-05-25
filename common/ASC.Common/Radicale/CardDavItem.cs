using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Common.Radicale
{
    public class CardDavItem
    {
        public Guid ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? Sex { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public List<string> Contacts { get; set; }

        public string MobilePhone { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1}", FirstName, LastName).Trim();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public CardDavItem(Guid iD, string firstName, string lastName, string userName, DateTime? birthDate, bool? sex, string title, string email, List<string> contacts, string mobilePhone)
        {
            ID = iD;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            BirthDate = birthDate;
            Sex = sex;
            Title = title;
            Email = email;
            Contacts = contacts;
            MobilePhone = mobilePhone;
        }
    }
}
