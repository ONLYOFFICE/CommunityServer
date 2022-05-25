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


/* Config */

module.exports = {
  // Port listener WebDav Server
  port: 1900,
  // Path to pfx key
  pfxKeyPath: null,
  // Pass phrase for pfx key
  pfxPassPhrase: null,
  // Path to .crt
  certPath: null,
  // Path to .key
  keyPath: null,
  // Enable secure connection
  isHttps: false,
  // root virtual directory
  virtualPath: "webdav",
  // Logging level
  logLevel: "debug",
  // Maximum execution time of long-running operations
  maxExecutionTime: 600000,
  // User cache storage time (msec)
  userLifeTime : 3600000,
  // Cleanup interval of expired users (msec)
  usersCleanupInterval: 600000,
  // Port of community server OnlyOffice */
  onlyOfficePort: ":80",
  // Maximum chunk size
  maxChunkSize: 10485760,
  // Api constant
  api: "/api/2.0",
  // Api authentication method
  apiAuth: "authentication.json",
  // Sub-method for files/folders operations
  apiFiles: "/files",
  // Path to read the file
  fileHandlerPath: "/Products/Files/HttpHandlers/filehandler.ashx?action=stream&fileid={0}",

  method: {
    // Get root directory in "Root"
    pathRootDirectory: "@root",
    // Operations with folders
    folder: "/folder",
    // Operations with files
    file: "/file",
    // Create new file "*.txt"
    text: "/text",
    // Create new file "*.html"
    html: "/html",
    // Get all active operations
    fileops: "/fileops",
    // File saving method
    saveediting: "/saveediting",
    // Method copy for files or folders
    copy: "/copy",
    // Method move for files or folders
    move: "/move",
    // Method for getting a link to download a file
    presigneduri: "/presigneduri",
    // Submethod to create a session
    upload: "/upload",
    // Method for creating a session
    createSession: "/create_session",
    // Method to create session to edit existing file with multiple chunks
    editSession: "/edit_session"
  }
};