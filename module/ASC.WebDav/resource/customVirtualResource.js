/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */


const webdav = require('webdav-server').v2;
const {
    getStructDirectory,
    statusOperation,
    getPresignedUri,
    getReadStream,
    readFile,
    createFile,
    createFolder,
    deleteFile,
    deleteFolder,
    copyFile,
    copyFolder,
    moveFile,
    moveFolder,
    renameFile,
    renameFolder,
    createFileTxt,
    createFileHtml,
    createEditSession
} = require('../server/requestAPI.js');
const {
    method,
    maxChunkSize,
    virtualPath
} = require('../server/config.js');
const streamWrite = require('../helper/writable.js');
const SimpleStruct = require('./simpleStruct.js');
const parse = require('../helper/propertyParser.js');
const {
    maxExecutionTime
} = require('../server/config');
const { logMessage } = require('../helper/helper.js');

class CustomVirtualResources {
    constructor() {
        this.structСache = new SimpleStruct();
        this.createdNow = [];
    }

    async getRootFolder(ctx, user) {
        const structRoot = {
            files: [],
            folders: [],
            current: {}
        };
        try {
            logMessage('CustomVirtualResources.getRootFolder', user.username);
            const structDir = await getStructDirectory(ctx, method.pathRootDirectory, user.token);
            for (let i = 0; i < structDir.length; i++) {
                structRoot.folders.push(structDir[i].current);
            }
            return structRoot;
        } catch (error) {
            return webdav.Errors.ResourceNotFound;
        }
    }

    findFolder(struct, element) {
        try {
            return struct.folders.find(folder => folder.title == element);
        } catch (error) {
            return false;
        }
    }

    findFile(struct, element) {
        try {
            return struct.files.find(file => file.title == element);
        } catch (error) {
            return false;
        }
    }

    async isOperationOver(ctx, token, response) {
        let isFinished = false;
        let status = {};
        const startTime = new Date();
        while (!isFinished) {
            status = await statusOperation(ctx, response.id, token);
            if (!status) {
                isFinished = true;
                status = response;
            } else {
                isFinished = status.finished;
            }
            if ((new Date() - startTime) >= maxExecutionTime) {
                throw new Error('The maximum waiting time for the operation has been exceeded');
            }
            if (status.error != null) {
                throw new Error(`Operation failed. Error: ${status.error}`);
            }
            if (!isFinished) {
                await new Promise(resolve => setTimeout(resolve, 200));
            }
        }
        return status;
    }

    fastExistCheck(path, ctx) {
        //logMessage('CustomVirtualResources.fastExistCheck', ctx.user.username, path);
        const substituteVP = virtualPath ? virtualPath : '';
        if (path == '/' || path == '/' + substituteVP) {
            return true;
        }
        const user = ctx.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        let struct = this.structСache.getStruct(parentFolder, user.uid);
        if (struct) {
            if (this.findFile(struct, element) || this.findFolder(struct, element)) {
                return true;
            }
        }
        return false;
    }

    async create(ctx, path) {
        logMessage('CustomVirtualResources.create', ctx.context.user.username, path);
        const user = ctx.context.user;
        let {
            element,
            parentFolder
        } = parse.parsePath(path);
        let parentId = this.structСache.getStruct(parentFolder, user.uid).current.id;


        if (ctx.type.isDirectory) {
            let createdObj = await createFolder(ctx, parentId, element, user.token);
            this.structСache.setFolderObject(parentFolder, user.uid, createdObj);
            await this.readDir(ctx, path);
        }
        if (ctx.type.isFile) {
            let createdObj = null;
            switch (parse.parseFileExt(element)) {
                case 'docx':
                case 'pptx':
                case 'xlsx':
                    createdObj = await createFile(ctx, parentId, element, user.token, false);
                    this.createdNow.push(createdObj.id);
                    this.structСache.setFileObject(parentFolder, user.uid, createdObj);
                    break;
                case 'txt':
                    createdObj = await createFileTxt(ctx, parentId, element, user.token);
                    this.createdNow.push(createdObj.id);
                    this.structСache.setFileObject(parentFolder, user.uid, createdObj);
                    break;
                case 'html':
                    createdObj = await createFileHtml(ctx, parentId, element, user.token);
                    this.createdNow.push(createdObj.id);
                    this.structСache.setFileObject(parentFolder, user.uid, createdObj);
                    break;
                default:
                    //TODO: think about .tmp when create .xlsx in Windows Exploder
                    createdObj = await createFile(ctx, parentId, element, user.token, true);
                    this.createdNow.push(createdObj.id);
                    this.structСache.setFileObject(parentFolder, user.uid, createdObj);
                    break;
            }
        }
    }

    async delete(ctx, path) {
        logMessage('CustomVirtualResources.delete', ctx.context.user.username, path);
        const user = ctx.context.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        const struct = this.structСache.getStruct(parentFolder, user.uid);
        try {
            const folder = this.findFolder(struct, element);
            if (folder) {
                const deleteResponse = await deleteFolder(ctx, folder.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, deleteResponse);
                this.structСache.dropFolderObject(parentFolder, user.uid, folder);
                this.structСache.dropPath(path, user.uid);
            }
            const file = this.findFile(struct, element);
            if (file) {
                const deleteResponse = await deleteFile(ctx, file.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, deleteResponse);
                this.structСache.dropFileObject(parentFolder, user.uid, file);
            }
        } catch (error) {
            return new Error(error);
        }
    }


    addPrivilege(ctx, sPath, title, parentId, access, rootFolderType) {
        let path = "";
        let privilageList = "";

        if (parentId == -1) {
            path = sPath + title;
            privilageList = ['all'];
        } else if (parentId == 0) {
            if (rootFolderType == 1 || rootFolderType == 5) {
                path = sPath + '/' + title;
                privilageList = ['all'];
            } else {
                path = sPath + '/' + title;
                privilageList = ['canRead'];
            }
        } else switch (access) {
            case 0:
                path = sPath + '/' + title;
                privilageList = ['all'];
                break;
            case 1:
                path = sPath + '/' + title;
                privilageList = ['canRead', 'canWriteContent', 'canWriteProperties', 'canWriteLocks'];
                break;
            default:
                path = sPath + '/' + title;
                privilageList = ['canRead'];
                break;
        }

        ctx.context.server.privilegeManager.setRights(ctx.context.user, path, privilageList);
    }

    addStructPrivilege(ctx, path, struct) {
        struct.folders.forEach(el => {
            this.addPrivilege(ctx, path, el.title, el.parentId, el.access, el.rootFolderType);
        });

        struct.files.forEach(el => {
            this.addPrivilege(ctx, path, el.title, el.parentId, el.access);
        });
    }

    async readDir(ctx, path) {
        logMessage('CustomVirtualResources.readDir', ctx.context.user.username, path);
        const user = ctx.context.user;
        const substituteVP = virtualPath ? virtualPath : '';

        if (virtualPath && path == '/') {
            let virtualRootFolder = this.structСache.getStruct(path, user.uid);
            if (virtualRootFolder) {
                return virtualRootFolder;
            }
            virtualRootFolder = {
                files: [],
                current: {
                    id: -1
                },
                folders: [{
                    id: 0,
                    title: virtualPath,
                    parentId: -1
                }]
            };
            this.structСache.setStruct(path, user.uid, virtualRootFolder);
            this.addStructPrivilege(ctx, path, virtualRootFolder);
            return this.structСache.getStruct(path, user.uid);
        }

        if (path == '/' + substituteVP) {
            try {
                let rootFolder = this.structСache.getStruct(path, user.uid);
                if (rootFolder) {
                    return rootFolder;
                }
                rootFolder = await this.getRootFolder(ctx, user);
                this.structСache.setStruct(path, user.uid, rootFolder);
                this.addStructPrivilege(ctx, path, rootFolder);
                return this.structСache.getStruct(path, user.uid);
            } catch (error) {
                return new Error(webdav.Errors.ResourceNotFound);
            }
        }

        const {
            element,
            parentFolder
        } = parse.parsePath(path);

        let struct = this.structСache.getStruct(parentFolder, user.uid);
        if (!struct) {
            struct = await this.readDir(ctx, parentFolder);
            if (!struct || struct instanceof Error) {
                return new Error(webdav.Errors.ResourceNotFound);
            } else {
                const folder = this.findFolder(struct, element);
                if (folder) {
                    try {
                        const structDirectory = await getStructDirectory(ctx, folder.id, user.token);
                        this.structСache.setStruct(path, user.uid, structDirectory);
                        this.addStructPrivilege(ctx, path, structDirectory);
                        return this.structСache.getStruct(path, user.uid);
                    } catch (error) {
                        return new Error(webdav.Errors.ResourceNotFound);
                    }
                }
            }
        }

        const folder = this.findFolder(struct, element);
        if (folder) {
            if (this.structСache.structIsNotExpire(path, user.uid)) {
                return this.structСache.getStruct(path, user.uid);
            }
            try {
                const structDirectory = await getStructDirectory(ctx, folder.id, user.token);
                this.structСache.setStruct(path, user.uid, structDirectory);
                this.addStructPrivilege(ctx, path, structDirectory);
                return this.structСache.getStruct(path, user.uid);
            } catch (error) {
                return new Error(webdav.Errors.ResourceNotFound);
            }
        }

        return new Error(webdav.Errors.ResourceNotFound);
    }

    async getReadStream(ctx, path) {
        logMessage('CustomVirtualResources.getReadStream', ctx.context.user.username, path);
        const user = ctx.context.user;
        const { element, parentFolder } = parse.parsePath(path);
        const file = this.findFile(this.structСache.getStruct(parentFolder, user.uid), element);
        if (file) {
            const uri = await getPresignedUri(ctx, file.id, user.token);
            const readStream = await getReadStream(ctx, uri, user.token);
            return readStream;
        } else {
            throw new Error(webdav.Errors.ResourceNotFound);
        }
    }

    readFile(ctx, path, callback) {
        logMessage('CustomVirtualResources.readFile', ctx.context.user.username, path);
        const user = ctx.context.user;
        const { element, parentFolder } = parse.parsePath(path);
        const file = this.findFile(this.structСache.getStruct(parentFolder, user.uid), element);
        if (file) {
            readFile(ctx, file.id, user.token, callback);
        } else {
            callback(new Error(webdav.Errors.ResourceNotFound));
        }
    }

    async writeFile(path, ctx) {
        logMessage('CustomVirtualResources.writeFile', ctx.context.user.username, path);
        const user = ctx.context.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(path);

        if (ctx.estimatedSize > 0) {

            const struct = this.structСache.getStruct(parentFolder, user.uid);
            const file = this.findFile(struct, element);
            const content = [];
            const positionId = this.createdNow.indexOf(file.id);
            if (positionId != -1) {
                var createdNow = true;
                this.createdNow = this.createdNow.splice(positionId + 1, 1);

                var location = await createEditSession(ctx, file.id, ctx.estimatedSize, user.token);
            }
            let arrayBufLength = [];
            if (ctx.estimatedSize > maxChunkSize) {
                arrayBufLength = new Array(maxChunkSize);
            }
            const stream = new streamWrite(content, arrayBufLength, ctx, location, user, file, createdNow);
            return stream;
        } else {
            const content = [];
            const stream = new streamWrite(content, undefined, undefined, undefined, undefined);
            return stream;
        }
    }

    async copy(ctx, pathFrom, pathTo) {
        logMessage('CustomVirtualResources.copy', ctx.context.user.username, pathFrom, pathTo);
        const user = ctx.context.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(pathFrom);
        pathTo = parse.parsePathTo(pathTo);
        const structTo = this.structСache.getStruct(pathTo, user.uid);
        if (!structTo) {
            return new Error(webdav.Errors.ResourceNotFound);
        }
        const folderId = structTo.current.id;
        const structFrom = this.structСache.getStruct(parentFolder, user.uid);
        const folder = this.findFolder(structFrom, element);
        if (folder) {
            try {
                const copyResponse = await copyFolder(ctx, folderId, folder.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, copyResponse);
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
        const file = this.findFile(structFrom, element);
        if (file) {
            try {
                const copyResponse = await copyFile(ctx, folderId, file.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, copyResponse);
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
    }

    async rename(ctx, path, newName) {
        logMessage('CustomVirtualResources.rename', ctx.context.user.username, path, newName);
        const user = ctx.context.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        const struct = this.structСache.getStruct(parentFolder, user.uid);
        const folder = this.findFolder(struct, element);
        if (folder) {
            try {
                await renameFolder(ctx, folder.id, newName, user.token);
                this.structСache.renameFolderObject(element, newName, parentFolder, user.uid);
                folder.realTitle = newName;
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
        const file = this.findFile(struct, element);
        if (file) {
            try {
                await renameFile(ctx, file.id, newName, user.token);
                this.structСache.renameFileObject(element, newName, parentFolder, user.uid);
                file.realTitle = newName;
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
    }

    async move(ctx, pathFrom, pathTo) {
        logMessage('CustomVirtualResources.move', ctx.context.user.username, pathFrom, pathTo);
        pathTo = parse.parsePathTo(pathTo);
        const {
            element: elementFrom,
            parentFolder: parentFolderFrom
        } = parse.parsePath(pathFrom);
        const {
            element: elementTo,
            parentFolder: parentFolderTo
        } = parse.parsePath(pathTo);
        const user = ctx.context.user;
        let isRename = false;
        if (parentFolderFrom == parentFolderTo) {
            isRename = this.structСache.checkRename(elementFrom, elementTo, parentFolderFrom, parentFolderTo, user);
        }
        if (isRename) {
            try {
                const rename = this.rename(ctx, pathFrom, elementTo);
                return rename;
            } catch (error) {
                return new Error(error);
            }
        }
        if (!this.structСache.getStruct(parentFolderTo, user.uid)) {
            return new Error(webdav.Errors.ResourceNotFound);
        }
        const folderId = this.structСache.getStruct(parentFolderTo, user.uid).current.id;
        const structFrom = this.structСache.getStruct(parentFolderFrom, user.uid);
        const folder = this.findFolder(structFrom, elementFrom);
        if (folder) {
            try {
                const moveResponse = await moveFolder(ctx, folderId, folder.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, moveResponse);
                this.structСache.dropFolderObject(parentFolderFrom, user.uid, folder);
                this.structСache.dropPath(pathFrom, user.uid);
                const folderWithNewLocation = status.folders.find(newLocation => newLocation.id == folder.id);
                this.structСache.setFolderObject(parentFolderTo, user.uid, folderWithNewLocation);
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
        const file = this.findFile(structFrom, elementFrom);
        if (file) {
            try {
                const moveResponse = await moveFile(ctx, folderId, file.id, user.token);
                const status = await this.isOperationOver(ctx, user.token, moveResponse);
                this.structСache.dropFileObject(parentFolderFrom, user.uid, file);
                const fileWithNewLocation = status.files.find(newLocation => newLocation.id == file.id);
                this.structСache.setFileObject(parentFolderTo, user.uid, fileWithNewLocation);
                return true;
            } catch (error) {
                return new Error(error);
            }
        }
    }

    getType(path, ctx) {
        const user = ctx.context.user;
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        const substituteVP = virtualPath ? virtualPath : '';
        if (parentFolder == '/' || parentFolder == '/' + substituteVP) {
            return webdav.ResourceType.Directory;
        }
        const struct = this.structСache.getStruct(parentFolder, user.uid);
        const folder = this.findFolder(struct, element);
        if (folder) {
            return webdav.ResourceType.Directory;
        }
        const file = this.findFile(struct, element);
        if (file) {
            return webdav.ResourceType.File;
        }
    }

    getSize(path, ctx) {
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        const user = ctx.context.user;
        const struct = this.structСache.getStruct(parentFolder, user.uid);
        const folder = this.findFolder(struct, element);
        if (folder) {
            return null;
        }
        const file = this.findFile(struct, element);
        if (file) {
            return file.pureContentLength;
        }
    }

    getlastModifiedDate(path, ctx) {
        const substituteVP = virtualPath ? virtualPath : '';
        if (path == '/' || path == '/' + substituteVP) {
            return new Date(0, 0, 0, 0, 0, 0);
        }
        const {
            element,
            parentFolder
        } = parse.parsePath(path);
        const user = ctx.context.user;
        const struct = this.structСache.getStruct(parentFolder, user.uid);
        const folder = this.findFolder(struct, element);
        if (folder) {
            return parse.parseDate(folder.updated);
        }
        const file = this.findFile(struct, element);
        if (file) {
            return parse.parseDate(file.updated);
        }
    }
}

module.exports = CustomVirtualResources;