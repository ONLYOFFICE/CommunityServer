using System.Threading.Tasks;

public interface ISample
{
    void PrintTasks();
    Task Run(string username, string apiKey, string region);
}