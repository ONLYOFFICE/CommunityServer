namespace ASC.Mail.Core.Entities
{
    public class Filter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
        public string User { get; set; }
        public bool Enabled { get; set; }
        public string FilterData { get; set; }
        public int Position { get; set; }
    }
}
