# Change log

## Version 12.0.0

### General portal changes

* Added the ability to connect apps for authorization via AppleId and Microsoft.
* Changed keys for authorization via Twilio.
* Changed keys for authorization via bitly.
* Added the portal name in logs.
* Updated to CKEditor v4.16.1, added new styles for TextCut and Magicline.
* Edited bottom paddings in the layout.
* Redesigned Feed and Mail drop-down lists.
* Redesigned textarea for the Chrome browser.
* Added a new page with UserVisits information in ASC.Web.Stat.
* Disabled Community module by default for new portals.
* Updated default image for the authorization page.
* Default logo for the About page cannot be changed by an administrator via the WhiteLabel settings in the SaaS version.
* Fixed Telegram notifications and Zendesk chat.
* Portal name cannot be less than 3 characters.
* DotNetZip library replaced with ICSharpCode.SharpZipLib, AjaxMin library replaced with NUglify.
* Fixed the bug related with IP Security and Talk vulnerabilities.
* Removed the ASC.Mail.Autoreply project and service.
* Removed the log cleaner for NLog.
* Fixed the AjaxPro and BinaryFormatter vulnerabilities.
* Redesigned userselector, added avatars.
* Updated emails about changing email and password.
* Added a tip on security in the general settings for the server version.
* Duplicated password entry field.
* Fixed deprecated methods after updating jquery.
* In the DNS settings, a custom domain can be added via the request to support for the SaaS version.
* Added mentions in comments to tasks, email messages, discussions.
* Added support for deep link: when opening a document in a mobile browser, user can choose if the document should be opened in a browser or in a mobile app.
* Disabled asp.net sessions in the project.

### Documents module

* Added a new page when opening documents in the Private Room via a web browser.
* Updated empty files.
* Added new settings to automatically clean up the Trash folder.
* Added support of fb2 files for viewing.
* Added the ability to download xml files with conversion / open for viewing.
* Added the ability to download text files with conversion to epub, fb2, html, dotx, ott.
* Added the ability to download spreadsheets with conversion to xltx, ots.
* Added the ability to download presentations with conversion to potx, otp.
* Added the ability to download oxps files with conversion to pdf / open for viewing.
* Added the ability to download oxps files with conversion to pdf / open for viewing.
* Added the ability to download files with conversion to OOXML with macros (docm, dotm, xlsm, xltm, pptm, potm).
* When interacting with editors, a file type is processed in incoming requests and is added in outgoing requests.
* The default About window is displayed in editors in the SaaS version. The logo and advanced branding are transferred to the editors About window in the server version.
* Added a link to FAQ when connecting Yandex.
* Fixed the file selection area.
* Redesigned the Sharing settings window.
* Added a new feature in the Sharing settings window - Can't print, download and copy file (for Read Only and Comment).
* Added a new feature in the Sharing settings window - Can't change sharing settings (for Full Access).
* Added a new feature in Common settings - Default access rights in sharing settings.
* Added a new section in the Admin settings - Sharing settings.
* Hidden the Private Room section in the FileChoice, SaveAs dialogs.
* When converting files, regional settings are transferred.
* Added missing logs for actions with files and folders, added new api methods for actions.
* Added the ability to add folders to Favorites.
* Added support of WebDAV server.

### Calendar module

* Added WYSIWYG CKEditor in the Description field of the event.
* Connected a new config for the Toolbar.
* Redesigned the Description column for the List page.
* Added DOMPurify sanitizer to prevent possible XSS.
* Added the ability to attach local files and links to files from the Documents module to events.

### Projects module

* Added the ability to drag-and-drop subtasks.
* Reworked reports: User Activity, Project List, add a time interval.

### People module

* Added Telegram in Contact information.
* Moved Birthdays from Community to People.
* Added Birthdays and New employees to Feed.
* Added the connection list in the profile with the ability to log out.
* Added the ability for administrators to log out all connections of the certain user.
* Removed import from Yahoo.
* Added the ability to create CardDAV address books.

### Mail module

* Added IMAP synchronization for Mail Server.
* Added the ability to perform Mail Server database backup and restore.
* Added the ability to request a read receipt.
* Moved the Aggregator, StorageCleaner and Watchdog mail services from the .Net Framework platform to .NET 6.0.

### Control Panel

* Added the Data Import page that allows to import data from Nextcloud, ownCloud and GoogleWorkspace to ONLYOFFICE Workspace.
* Moved Elasticsearch to a separate container.
* Fixed bugs.

## Version 11.6.0

### Documents module

* Added the `.docxf` and `.oform` format definition. Own format icons are displayed.
* Added the ability to download `.docx` as `.docxf`. `.docxf` can be downloaded in the same formats as for `.docx` and in the `.oform` format.
* Added the ability to create an empty `.docxf` file and to create it on the base of an existing `.docx` file.
* Added the ability to create an `.oform` file from a `.docxf` file via the context menu.
* Added the ability to create an `.oform` file from a `.docxf` file from the editor specifying a folder.
* The ability to share a file with the Fill in the form permissions is only available for the `.oform` files. It's not possible to share encrypted files and files from third-party accounts for form filling. The Form Filling option is not available when sharing an `.oform` file via an external link.
* Added the Fill in the form icon in the file list to open an `.oform` file in the editor.
* If the `.oform` file is shared with the Form Filling rights, a copy of the file is created in the My documents section when a user opens the shared file. Filling in the form is performed in this copy and does not affect the original file. If the original file is changed by the owner, its copy will not be up-to-date.
* If the `.oform` file is shared with the Full Access rights, filling in the form is performed in the original file.
* Added sample `.docxf` and `.oform` files.
* For the editor dark theme, the usual logo is transferred so that it displays correctly in the embedded mode.

## Version 11.5.3

### General portal settings

* Added a dialog with the editors statistics on Payments page in Control Panel.
* Added a new slide to the start banner, added the Product Demo item to the Feedback & Support menu for trial SaaS portals.
* Added the Request training item to the Feedback & Support menu for paid portals (SaaS and Server versions).
* Added the Live Chat switcher to the Feedback & Support menu for paid portals (SaaS and Server versions).
* Added the Email support item to the Feedback & Support menu for paid portals (SaaS and Server versions).

## Version 11.5.2

### General portal settings

* ASC.Web.Studio: the `onlyoffice_logo` folder renamed to `logo`;
* Added a banner for the ONLYOFFICE Projects app in the App Store;
* Removed the button for downloading a paid invoice on the Payments page;
* Defining work from the Desktop on the server via userAgent (starting from version 6.1);
* Added the Zendesk chat display on the Payments page in the SaaS version;
* Updated the Payments page in the SaaS version. Prices are based on a number of users;
* Added the welcome dialog window for the portal owner on the main portal page;
* Removed the limitation on the amount of portal data when creating a backup;
* When storing backups to a third party service, chunk loading is now used;
* The Restore feature is available in the SaaS version if the pricing plan includes this option;
* Filter: a tip on the search is enabled in all modules, excep for People and Sample;
* Fixed calculating the total\current number of full-text search indexes;
* The full-text search feature is available in the SaaS version if the pricing plan includes this option;
* Updated ElasticSearch to the version 7.9;
* Reworked the receipt of data from the payment system for authorized requests in the SaaS version;
* The LDAP feature is available in the SaaS version if the pricing plan includes this option;
* Added the `temp` setting to specify a path for creating temporary files instead of the system `Path.GetTempFileName()` one;
* Changed email notifications for the SaaS version and messages about changing an email;
* Changed the assembly for archiving backup;
* Reworked the mechanism for working with resource files;
* All the available languages are enabled in the Personal cloud;
* The SSO feature is available in the SaaS version if the pricing plan includes this option. The `ssoauth service` moved from the control panel to portals;
* Updated the Yandex icons on the user profile page, authorization page and third party service connection settings;
* Added a checkpoint to the migration page and changed the email notification about the successful completion of the migration;
* Restored the mail migration functionality;
* Added the text about resetting users' passwords on the Restore settings page, added an email notification about the successful completion of the restoring process.

### Documents module

* Added Thumbnails view. Thumbnails are not generated for video. Thumbnails are not generated in third-party storages;
* Added the ability to select the archiving type in the settings: zip (by default) or tar.gz;
* Removed the support of the version 5.3 of the editors;
* Updated the Google Drive icons;
* Fixed calculating a quote when downloading several files in an archive;
* Removed transferring the anonymous name to the editor for using the renaming functionality in the versions 6.2, 6.3 of the editors;
* Added the ability to convert the fb2 files;
* Files can be shared with guests for viewing only;
* Removed the button to share a file with a group in the Private Room;
* Transferring the favorite status of a file to the version 6.3 of the editors;
* Signing in JWT the `callbackUrl` of the editor initialization configuration;
* Video and audio files are added to start samples in all languages;
* Archiving and compressing via tar.gz instead of zip when downloading several files;
* Removed the limitation on the size of files when downloading several files in an archive;
* The EasyBib and WordPress plugins are not available in the free version of the editors.

### CRM module

* Moved queue settings to the configuration. 2 streams are allocated for data export.

### Mail module

* Added the ability to get the connection settings for connecting a mailbox to third-party mail clients and the ability to change passwords for mailboxes created on the Mail Server in the SaaS version.

### Calendar

* Fixed bugs.

## Version 11.0

### General portal changes

* Added the ability to receive portal notifications via Telegram;
* Added the ability to make an addon (Mail, Chat, Calendar) a default portal page;
* Added the additional license check when replacing ONLYOFFICE in the About this program window;
* Added the vsyscall check to the installation scripts when installing Mail Server on Debian with kernel 4.18.0 and later;
* Updated copirights in the source files;
* Added the ability to connect the Mail.ru, VK and Yandex applications for authorization;
* Added new icons and texts in the welcome placeholders of the empty modules;
* Added icons to context menus;
* Added two scrolling areas on the page: navigation and content;
* Reworked the mechanics for displaying messages on the authorization page;
* Updated Elasticsearch to v.7.4. Added the possibility to rebuild the index.
* Updated Mono to v.6.8, updated builds, improved performance;
* Added the file encryption at rest feature for server versions;
* When requesting the password recovery, it's not possible to check if an email address is used on the portal;
* It is prohibited to specify the current password when changing a password;
* Updated minimal password length to 8 characters;
* The password complexity check is now performed in the client-side browser;
* Added password hashing with the PBKDF2 algorithm when transmitting passwords to the server;
* Added the Sign in to domain option on the authorization page for the portals where the LDAP Authentication is enabled;
* Added a request for subscription to newsletters in the free Community version installation wizard;
* Removed the link to the forum;
* Added data cleaning before restoring backup;
* When the license expires, Document Server updates are blocked in the installation;
* Portals with an expired license and Enterprise portals with a free default license are not blocked;
* In the free Community version, a block with a proposal to install Enterprise Edition is added;
* The Payment page redirects to the Control Panel (if it is installed);
* Added the `-it`, `--installation_type` parameter to the installation scripts. The possible values are `GROUPS | WORKSPACE | WORKSPACE_ENTERPRISE`.

### Documents module

* Changed the note about the mention in the comment pop-up window depending on the ability to provide access to the document;
* Added the support for the .webp images;
* Using a new MailMerge API for the editors;
* Opening the default portal page when clicking to the logo in the upper left corner of the editors interface;
* Added the ability to connect kDrive via WebDav;
* Added the Favorites section, added the ability to add files to favorites via context menu;
* Added the Recent section;
* Added translations for the EasyBib and WordPress plugins;
* Added the ability to provide the Custom Filter access rights for spreadsheets;
* Replaced an empty tab with a static page containing the logo when creating a new file;
* If the DocuSign service is not connected, the instructions are displayed when clicking the Sign with DocuSign context menu option;
* Added a new Private Room section;
* If an administrator disabled the Allow users to connect third-party storages option, they also cannot connect third-party clouds;
* Added a separate section for quick access to admin settings on the left panel;
* Added links to download desktop and mobile apps;
* Added empty files for creation in Japanese (ja-JP).

### CRM module

* Added the Make a VoIP call action in the contact context menu;
* Removed the unnecessary Quantity field when creating a new invoice item;
* Added the ability to enter decimal fractions with two decimal places in the Discount field when creating an invoice.

### Mail module

* Added a new scroll mechanism for the screen width greater than 1200 px.

### Calendar

* Improved the Calendar API security;
* Reworked the authorization method for the CalDAV calendars;
* Reworked the Radicale plugins for the new version 3.0.
