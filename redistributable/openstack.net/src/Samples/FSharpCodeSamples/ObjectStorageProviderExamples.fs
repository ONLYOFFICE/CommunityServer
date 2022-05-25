module ObjectStorageProviderExamples

open System;
open System.Collections.Generic;
open net.openstack.Core.Domain;
open net.openstack.Core.Providers;

//#region ListObjectsInContainer
let listAllObjects(provider : IObjectStorageProvider, containerName : string) =
    seq {
        let lastContainerObject : ContainerObject ref = { contents = null }
        let finished : bool ref = { contents = false }
        while not !finished do
            let marker = if !lastContainerObject <> null then (!lastContainerObject).Name else null
            let containerObjects = provider.ListObjects(containerName, marker= marker)
            lastContainerObject := null
            for containerObject in containerObjects do
                lastContainerObject := containerObject
                yield containerObject
            if !lastContainerObject = null then
                finished := true
    }

let listObjects(provider : IObjectStorageProvider, containerName : string) =
    Console.WriteLine("Objects in container {0}", containerName)
    for containerObject in listAllObjects(provider, containerName) do
        Console.WriteLine("    {0}", containerObject.Name)
    ()
//#endregion
