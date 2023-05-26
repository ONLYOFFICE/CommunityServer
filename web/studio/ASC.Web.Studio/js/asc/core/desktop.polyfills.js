/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


//desktop chrome 49
(function (arr) {
    arr.forEach(function (item) {
        if (!item.hasOwnProperty('append')) {
            Object.defineProperty(item, 'append', {
                configurable: true,
                enumerable: true,
                writable: true,
                value: function append() {
                    var argArr = Array.prototype.slice.call(arguments),
                        docFrag = document.createDocumentFragment();

                    argArr.forEach(function (argItem) {
                        docFrag.appendChild(argItem instanceof Node ? argItem : document.createTextNode(String(argItem)));
                    });

                    this.appendChild(docFrag);
                }
            });
        }

        if (!item.hasOwnProperty('prepend')) {
            Object.defineProperty(item, 'prepend', {
                configurable: true,
                enumerable: true,
                writable: true,
                value: function prepend() {
                    var argArr = Array.prototype.slice.call(arguments),
                        docFrag = document.createDocumentFragment();

                    argArr.forEach(function (argItem) {
                        docFrag.appendChild(argItem instanceof Node ? argItem : document.createTextNode(String(argItem)));
                    });

                    this.insertBefore(docFrag, this.firstChild);
                }
            });
        }

        if (!item.hasOwnProperty('after')) {
            Object.defineProperty(item, 'after', {
                configurable: true,
                enumerable: true,
                writable: true,
                value: function after() {
                    var argArr = Array.prototype.slice.call(arguments),
                        docFrag = document.createDocumentFragment();

                    argArr.forEach(function (argItem) {
                        docFrag.appendChild(argItem instanceof Node ? argItem : document.createTextNode(String(argItem)));
                    });

                    this.parentNode.insertBefore(docFrag, this.nextSibling);
                }
            });
        }
    });
})([Element.prototype, Document.prototype, DocumentFragment.prototype]);

function ReplaceWithPolyfill() {
    var parent = this.parentNode,
        i = arguments.length,
        currentNode;
    if (!parent) return;
    if (!i) {
        parent.removeChild(this);
    }
    while (i--) {
        currentNode = arguments[i];
        if (typeof currentNode !== "object") {
            currentNode = this.ownerDocument.createTextNode(currentNode);
        } else if (currentNode.parentNode) {
            currentNode.parentNode.removeChild(currentNode);
        }
        if (!i) {
            parent.replaceChild(currentNode, this);
        } else {
            parent.insertBefore(this.previousSibling, currentNode);
        }
    }
}

if (!Element.prototype.replaceWith)
    Element.prototype.replaceWith = ReplaceWithPolyfill;
if (!CharacterData.prototype.replaceWith)
    CharacterData.prototype.replaceWith = ReplaceWithPolyfill;
if (!DocumentType.prototype.replaceWith)
    DocumentType.prototype.replaceWith = ReplaceWithPolyfill;