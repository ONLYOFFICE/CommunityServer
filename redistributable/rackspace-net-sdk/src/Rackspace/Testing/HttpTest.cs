namespace Rackspace.Testing
{
    /// <summary>
    /// Use this instead of Flurl.Http.Testing.HttpTest when writing unit tests against the Rackspace APIs.
    /// <para/>
    /// <para>
    /// This extends <see href="http://tmenier.github.io/Flurl/">Flurl's</see> the appropriate HttpMessageHandler in unit tests. 
    /// If you use the default HttpTest, then any tests which rely upon authentication handling (e.g retrying a request when a token expires) will fail.
    /// </para>
    /// </summary>
    public class HttpTest : OpenStack.Testing.HttpTest
    {
    }
}