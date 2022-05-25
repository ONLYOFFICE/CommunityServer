Imports System.Threading.Tasks

Public Class AsynchronousExceptionsExamples

    Public Sub ExceptionPriorToTaskCreation()
        ' #Region ExceptionPriorToTaskCreation
        Try
            Dim myTask As Task = SomeOperationAsync()
        Catch ex As ArgumentException
            ' ex was thrown directly by SomeOperationAsync. This cannot occur if SomeOperationAsync is an async method
            ' (§10.1.3 - Visual Basic Language Specification Version 11.0).
        End Try
        ' #End Region
    End Sub

    Public Sub ExceptionDuringTaskExecution()
        ' #Region ExceptionDuringTaskExecution
        Try
            Dim myTask As Task = SomeOperationAsync()
            myTask.Wait()
        Catch wrapperEx As AggregateException
            Dim ex = TryCast(wrapperEx.InnerException, ArgumentException)
            If ex Is Nothing Then
                Throw
            End If

            ' ex was thrown during the asynchronous portion of SomeOperationAsync. This is always the case if
            ' SomeOperationAsync is an async method (§10.1.3 - Visual Basic Language Specification Version 11.0).
        End Try
        ' #End Region
    End Sub

    Public Sub AsynchronousMethodAsContinuation()
        ' #Region AsynchronousMethodAsContinuation
        ' original asynchronous method invocation
        Dim task1 = SomeOperationAsync()

        ' method invocation treated as a continuation
        Dim task2 = task1.ContinueWith(Function(task) SomeOperationAsync())
        ' #End Region
    End Sub

    Public Function SomeOperationAsync() As Task
        Throw New NotSupportedException()
    End Function

End Class
