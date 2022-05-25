using System.Threading.Tasks;

public interface ISample
{
    void PrintTasks();
    Task Run(string identityEndpoint, string username, string password, string project, string region);
}