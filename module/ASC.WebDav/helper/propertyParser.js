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


class PropertyParser {
    static parsePath(path) {
        let pathArray = path.split('/');
        let targetElement = pathArray.pop();
        let targetElementArray = targetElement.split('.');
        if (targetElementArray.length > 1) {
            let ext = targetElementArray.pop();
            targetElementArray.push(ext.toLowerCase());
            targetElement = targetElementArray.join('.');
        }
        if (pathArray.length <= 1) {
            pathArray[0] = '/';
        }
        let parentPath = pathArray.join('/');
        return {
            element: targetElement,
            parentFolder: parentPath
        };
    }

    static parsePathTo(pathTo) {
        let pathArray = pathTo.split('/');
        if (pathArray[pathArray.length - 1] == '' && pathTo !== '/') {
            pathArray.pop();
            var newPath = pathArray.join('/');
        } else {
            var newPath = pathTo;
        }
        return newPath;
    }

    static parseDate(dateString) {
        let dateArray = dateString.split('.');
        dateArray = dateArray[0].split('T');
        let date = dateArray[0].split('-');
        let time = dateArray[1].split(':');
        return new Date(date[0], date[1] - 1, date[2], time[0], time[1], time[2]);
    }

    static parseFileExt(fileName) {
        let fileNameArray = (fileName || '').split('.');
        return fileNameArray.length > 1 ? fileNameArray.pop() : null;
    }
}

module.exports = PropertyParser;