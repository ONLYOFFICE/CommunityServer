using System;
using System.Threading.Tasks;

namespace CSharpCodeSamples
{
    public class AsynchronousExceptionsExamples
    {
        public void ExceptionPriorToTaskCreation()
        {
#pragma warning disable 168 // The variable 'var' is assigned but its value is never used
            #region ExceptionPriorToTaskCreation
            try
            {
                Task myTask = SomeOperationAsync();
            }
            catch (ArgumentException ex)
            {
                // ex was thrown directly by SomeOperationAsync. This cannot occur if SomeOperationAsync is an async
                // function (§10.15 - C# Language Specification Version 5.0).
            }
            #endregion
#pragma warning restore 168
        }

        public void ExceptionDuringTaskExecution()
        {
            #region ExceptionDuringTaskExecution
            try
            {
                Task myTask = SomeOperationAsync();
                myTask.Wait();
            }
            catch (AggregateException wrapperEx)
            {
                ArgumentException ex = wrapperEx.InnerException as ArgumentException;
                if (ex == null)
                    throw;

                // ex was thrown during the asynchronous portion of SomeOperationAsync. This is always the case if
                // SomeOperationAsync is an async function (§10.15 - C# Language Specification Version 5.0).
            }
            #endregion
        }

        public void AsynchronousMethodAsContinuation()
        {
            #region AsynchronousMethodAsContinuation
            // original asynchronous method invocation
            Task task1 = SomeOperationAsync();

            // method invocation treated as a continuation
            Task task2 = task1.ContinueWith(_ => SomeOperationAsync());
            #endregion
        }

        private static Task SomeOperationAsync()
        {
            throw new NotSupportedException();
        }
    }
}
