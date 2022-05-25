namespace ASC.AuditTrail.Types
{
    public enum ActionType
    {
        None,
        Create,
        Update,
        Delete,
        Link,
        Unlink,
        Attach,
        Detach,
        Send,
        Import,
        Export,
        UpdateAccess,
        Download,
        Upload,
        Copy,
        Move,
        Reassigns,
        Follow,
        Unfollow,
        Logout
    }
}