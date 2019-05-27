namespace Ical.Net.DataTypes
{
    public class FreeBusyEntry : Period
    {
        public virtual FreeBusyStatus Status { get; set; }

        public FreeBusyEntry()
        {
            Status = FreeBusyStatus.Busy;
        }

        public FreeBusyEntry(Period period, FreeBusyStatus status)
        {
            //Sets the status associated with a given period, which requires copying the period values
            //Probably the Period object should just have a FreeBusyStatus directly?
            CopyFrom(period);
            Status = status;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var fb = obj as FreeBusyEntry;
            if (fb != null)
            {
                Status = fb.Status;
            }
        }
    }
}