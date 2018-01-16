using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class AsyncExtensions
    {
        public static T ResultAndUnwrap<T>(this Task<T> task)
        {
            try
            {
                return task.Result;
            }
            catch (AggregateException exception)
            {
                if (exception.InnerExceptions.Count == 1)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }

        public static void WaitAndUnwrap(this Task task, int timeout)
        {
            try
            {
                if (timeout == 0)
                    task.Wait();
                else if (!task.Wait(timeout))
                    throw new SocketException(258); // WAIT_TIMEOUT
            }
            catch (AggregateException exception)
            {
                if (exception.InnerExceptions.Count == 1)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }
    }
}