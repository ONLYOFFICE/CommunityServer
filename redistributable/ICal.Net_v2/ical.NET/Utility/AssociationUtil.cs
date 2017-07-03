using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Utility
{
    public class AssociationUtil
    {
        public static void AssociateItem(object item, ICalendarObject objectToAssociate)
        {
            if (item is ICalendarDataType)
            {
                ((ICalendarDataType) item).AssociatedObject = objectToAssociate;
            }
            else if (item is ICalendarObject)
            {
                ((ICalendarObject) item).Parent = objectToAssociate;
            }
        }

        public static void DeassociateItem(object item)
        {
            if (item is ICalendarDataType)
            {
                ((ICalendarDataType) item).AssociatedObject = null;
            }
            else if (item is ICalendarObject)
            {
                ((ICalendarObject) item).Parent = null;
            }
        }
    }
}