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


class SimpleStruct {
    constructor() {
        this.struct = {};
    }

    setStruct(path, uid, structDir) {
        if (!this.struct) {
            this.struct = {};
        }
        if (!this.struct[uid]) {
            this.struct[uid] = {};
        }
        this.struct[uid][path] = structDir;
        this.struct[uid].lastUpdate = new Date();
    }

    getStruct(path, uid) {
        return this.struct[uid] && this.struct[uid][path];
    }

    deleteStruct(uid) {
        delete this.struct[uid];
    }

    deleteStructs(uids) {
        uids.forEach(uid => {
            this.deleteStruct(uid);
        });
    }

    setFileObject(path, uid, newFile) {
        this.struct[uid][path].files.push(newFile);
        this.struct[uid].lastUpdate = new Date();
    }

    setFolderObject(path, uid, newFile) {
        this.struct[uid][path].folders.push(newFile);
        this.struct[uid].lastUpdate = new Date();
    }

    dropFileObject(Folder, uid, file) {
        this.struct[uid][Folder].files.forEach(el => {
            if (el.id == file.id) {
                const id = this.struct[uid][Folder].files.indexOf(el);
                this.struct[uid][Folder].files.splice(id, 1);
                this.struct[uid].lastUpdate = new Date();
            }
        });
    }

    dropFolderObject(Folder, uid, folder) {
        this.struct[uid][Folder].folders.forEach(el => {
            if (el.id == folder.id) {
                const id = this.struct[uid][Folder].folders.indexOf(el);
                this.struct[uid][Folder].folders.splice(id, 1);
                this.struct[uid].lastUpdate = new Date();
            }
        });
    }

    dropPath(path, uid) {
        if (this.struct[uid][path]) {
            delete this.struct[uid][path];
            this.struct[uid].lastUpdate = new Date();
        }
    }

    checkRename(elementFrom, elementTo, parentFolderFrom, parentFolderTo, user) {

        if (parentFolderFrom != parentFolderTo) return false;
        let elementFromIsExist = false;
        let elementToIsExist = false;
        let structFrom = this.struct[user.uid][parentFolderFrom];
        let structTo = this.struct[user.uid][parentFolderTo];
        structFrom.files.forEach((el) => {
            if (elementFrom == el.title) {
                elementFromIsExist = true;
            }
        });
        if (!elementFromIsExist) {
            structFrom.folders.forEach((el) => {
                if (elementFrom == el.title) {
                    elementFromIsExist = true;
                }
            });
        }
        if (!elementFromIsExist) return false;

        structTo.files.forEach((el) => {
            if (elementTo == el.title) {
                elementToIsExist = true;
            }
        });
        if (!elementToIsExist) {
            structTo.folders.forEach((el) => {
                if (elementTo == el.title) {
                    elementToIsExist = true;
                }
            });
        }
        if (!elementToIsExist) return true;
        return true;
    }

    renameFolderObject(element, newName, parentFolder, uid) {
        this.struct[uid][parentFolder].folders.forEach(el => {
            if (el.title == element) {
                const id = this.struct[uid][parentFolder].folders.indexOf(el);
                this.struct[uid][parentFolder].folders[id].title = newName;
                this.struct[uid].lastUpdate = new Date();
            }
        });
    }

    renameFileObject(element, newName, parentFolder, uid) {
        this.struct[uid][parentFolder].files.forEach(el => {
            if (el.title == element) {
                const id = this.struct[uid][parentFolder].files.indexOf(el);
                this.struct[uid][parentFolder].files[id].title = newName;
                this.struct[uid].lastUpdate = new Date();
            }
        });
    }

    structIsNotExpire(path, uid) {
        if (!this.struct[uid][path]) {
            return false;
        } else {
            const difference = 1000;
            const notExpire = (new Date() - this.struct[uid].lastUpdate) < difference;
            return notExpire;
        }
    }
}

module.exports = SimpleStruct;