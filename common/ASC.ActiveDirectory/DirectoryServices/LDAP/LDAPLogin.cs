namespace ASC.ActiveDirectory.DirectoryServices.LDAP
{
    public class LDAPLogin
    {
        public string Username { get; private set; }
        public string Domain { get; private set; }

        public LDAPLogin(string username, string domain)
        {
            Username = username;
            Domain = domain;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Domain) ? string.Format("{0}@{1}", Username, Domain) : Username;
        }
    }
}
