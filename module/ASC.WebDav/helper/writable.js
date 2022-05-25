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


const {
    Duplex
} = require('stream');
const {
    maxChunkSize
} = require('../server/config.js');
const FormData = require("form-data");
const {
    chunkedUploader,
    rewritingFile
} = require('../server/requestAPI.js');

class streamWrite extends Duplex {
    constructor(contents, arrayBufLength, ctx, location, user, file, createdNow) {
        super(null);
        this.contents = contents;
        this.contentsLength = 0;
        this.location = location;
        this.arrayBuf = arrayBufLength;
        this.firstPosition = 0;
        this.lastChunk = 0;
        this.count = -1;
        this.totalCount = 0;
        this.ctx = ctx;
        this.user = user;
        this.file = file;
        this.createdNow = createdNow;
    }

    _read() {
        for (let i = 0; i < this.contents.length; i++) {
            this.push(this.contents[i]);
        }
        this.push(null);
    }

    async _write(chunk, encoding, callback) {
        if (this.file) {
            if (!this.createdNow) {
                this.contents.push(chunk);
                this.contentsLength += chunk.length;
                if (this.contentsLength == this.ctx.estimatedSize) {
                    const form_data = new FormData();
                    form_data.append("FileExtension", this.file.fileExst);
                    form_data.append("DownloadUri", "");
                    form_data.append("Stream", this, {
                        filename: this.file.realTitle,
                        contentType: "text/plain"
                    });
                    form_data.append("Doc", "");
                    form_data.append("Forcesave", 'false');
                    await rewritingFile(this.ctx, this.file.id, form_data, this.user.token);
                }

            } else {
                for (let i = 0; chunk.length > i; i++) {
                    this.count++;
                    this.totalCount++;
                    this.arrayBuf[this.count] = chunk[i];

                    let fullChunk = this.count == (maxChunkSize - 1);
                    //let lastByte = chunk[i + 1] == undefined && (this.lastChunk > chunk.length || this.count == this.ctx.estimatedSize - 1);
                    let lastByte = this.totalCount == this.ctx.estimatedSize;

                    if (fullChunk || lastByte) {
                        this.arrayBuf.length = this.count + 1;
                        const form_data = new FormData();
                        form_data.append("files[]", Buffer.from(this.arrayBuf), "chunk" + i);
                        await chunkedUploader(this.ctx, this.location, form_data, this.user.token, this.ctx.estimatedSize, this.firstPosition, this.firstPosition + this.arrayBuf.length - 1);
                        this.firstPosition += this.arrayBuf.length;
                        if (this.ctx.estimatedSize < maxChunkSize) {
                            this.arrayBuf = [];
                        }
                        this.count = -1;
                    }
                    if (lastByte) {
                        if (global.gc) {
                            global.gc();
                        }
                        this.arrayBuf = null;
                        this.ctx = null;
                    }
                }
                this.lastChunk = chunk.length;
            }
        }
        callback(null);
    }
}

module.exports = streamWrite;