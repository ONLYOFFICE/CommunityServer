Imports net.openstack.Core.Providers
Imports net.openstack.Core.Domain

Public Class ObjectStorageProviderExamples

#Region "ListObjectsInContainer"
    Public Sub ListObjects(provider As IObjectStorageProvider, containerName As String)
        Console.WriteLine("Objects in container {0}", containerName)
        For Each containerObject In ListAllObjects(provider, containerName)
            Console.WriteLine("    {0}", containerObject.Name)
        Next
    End Sub

    Private Shared Iterator Function ListAllObjects(
        provider As IObjectStorageProvider,
        containerName As String,
        Optional blockSize As Nullable(Of Integer) = Nothing,
        Optional prefix As String = Nothing,
        Optional region As String = Nothing,
        Optional useInternalUrl As Boolean = False,
        Optional identity As CloudIdentity = Nothing) As IEnumerable(Of ContainerObject)

        If blockSize <= 0 Then
            Throw New ArgumentOutOfRangeException("blockSize")
        End If

        Dim lastContainerObject As ContainerObject = Nothing
        Do
            Dim marker = If(lastContainerObject IsNot Nothing, lastContainerObject.Name, Nothing)
            Dim containerObjects = provider.ListObjects(containerName, blockSize, marker, Nothing, prefix, region, useInternalUrl, identity)
            lastContainerObject = Nothing
            For Each containerObject In containerObjects
                lastContainerObject = containerObject
                Yield containerObject
            Next
        Loop While lastContainerObject IsNot Nothing
    End Function
#End Region


End Class
