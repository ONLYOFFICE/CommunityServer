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


var addRealTitle = function (response, folderId) {
    if (folderId != '@root') {
        let structFile = response.data.response.files;
        for (let i = 0; i < structFile.length; i++) {
            response.data.response.files[i]['realTitle'] = structFile[i].title;
        }
        let structFolder = response.data.response.folders;
        for (let i = 0; i < structFolder.length; i++) {
            response.data.response.folders[i]['realTitle'] = structFolder[i].title;
        }
        return response;
    } else {
        return response;
    }
};

var checkDuplicateNames = function (response) {
    let structFile = response.data.response.files;
    for (let i = 0; i < structFile.length; i++) {
        for (let j = i; j < structFile.length; j++) {
            if ((i != j) && (structFile[i].title == structFile[j].title)) {
                return false;
            }
        }
    }
    let structFolder = response.data.response.folders;
    for (let i = 0; i < structFolder.length; i++) {
        for (let j = i; j < structFolder.length; j++) {
            if ((i != j) && (structFolder[i].title == structFolder[j].title)) {
                return false;
            }
        }
    }
    return true;
};



var localRename = function (response, folderId) {
    if (folderId != '@root') {
        if (!checkDuplicateNames(response)) {
            let structFile = response.data.response.files;
            for (let i = 0; i < structFile.length; i++) {
                let c = 1;
                for (let j = i; j < structFile.length; j++) {
                    if ((i != j) && (structFile[i].title == structFile[j].title)) {
                        const title = structFile[j].title;
                        const splitedTitle = title.split(".");
                        const realTitle = structFile[j].realTitle;
                        if (realTitle == title) {
                            response.data.response.files[j].title = splitedTitle[0] + `(${c}).` + splitedTitle[1];
                            c++;
                        } else {
                            let reversTitle = title.split("").reverse().join("");
                            let num = reversTitle.split(")", 2)[1].split("(")[0].split("").reverse().join("");
                            response.data.response.files[j].title = realTitle.split(".")[0] + `(${Number(num)+1}).` + splitedTitle[1];
                        }
                    }
                }
            }
            let structFolders = response.data.response.folders;
            for (let i = 0; i < structFolders.length; i++) {
                let c = 1;
                for (let j = i; j < structFolders.length; j++) {
                    if ((i != j) && (structFolders[i].title == structFolders[j].title)) {
                        const title = structFolders[j].title;
                        const realTitle = structFolders[j].realTitle;
                        if (realTitle == title) {
                            response.data.response.folders[j].title = title + `(${c})`;
                            c++;
                        } else {
                            let reversTitle = title.split("").reverse().join("");
                            let num = reversTitle.split(")", 2)[1].split("(")[0].split("").reverse().join("");
                            response.data.response.folders[j].title = realTitle.split(".")[0] + `(${Number(num)+1})`;
                        }
                    }
                }
            }
            return localRename(response, folderId);
        } else {
            return response;
        }
    } else {
        return response;
    }
};

module.exports = {
    addRealTitle,
    checkDuplicateNames,
    localRename
};