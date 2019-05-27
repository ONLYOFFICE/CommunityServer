namespace SelectelSharp.Models.Container
{
    public enum DeleteContainerResult
    {
        Deleted = 204,
        NotFound = 404,
        NotEmpty = 409
    }
}
