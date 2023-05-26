# Change log

## Version 12.5.2

### General portal changes

* Fixed issue with Interface Theme and Contact Information arranging incorrectly upon reducing window width in People (Bug #62061).
* Fixed issue with an uploaded folder appearing only after reloading the page (Bug #61506).
* Fixed issue in CRM when address not appearing in the Invoice generated through the Invoice tab on the contact page (Bug #62050).
* Fixed issue in Calendar when an event from an attached ics file not synchronized via the CalDAV protocol (Bug #62048).
* Increased horizontal scroll in Desktop editors when connected to a portal (Bug #62153).
* Fixed issue with Show backup codes button would not working in People (Bug #62159).
* Fixed the inability to download a >5 MB file from an external access folder.
* Added translation of comments in source code into English.
* Fixed styles for Dark mode.
* Fixed styles for SSO.
* Refactoring of AutoCleanUp.
* Improvements for building server versions.
* Fixed dependency installation for python version 3.11 or later.
* Fixed moving backup file after backup to s3.
* Quota: fixed SetTenantQuotaRow function.
* Refactoring of DbManager.

### Documents module

* Added conversion of dps, dpt, et, ett, htm, mhtml, stw, sxc, sxi, sxw, wps, wpt, and xlsb files to supported formats.

## Version 12.5.1

### General portal changes

* Fixed issue with editors not opening (Error 500. Internal server error) when external sharing is disabled (Bug #61800).
* Fixed issue with 'Warning! Connection is lost' appearing error upon restoring version from editor while file is being edited (Bug #61772).
* Fixed issue when pressing Enter while creating an external link in the editor would also close the Sharing Setting window (Bug #61939).
* Fixed issue in Mail when the file selection window wouldn't expand when many folders are opened (Bug #61158).
* Fixed issue in Calendar when Invitations and cancellations are not sent for an event created in another calendar using a CalDAV link (Bug #48022).
* Fixed issue in Calendar when the Create button woudl not close after changing the year or month (Bug #59590).
* Fixed issue in CRM when a reset filter is reactivated on subsequent page transitions (Bug #54608).
* Added the ability in CRM to create a company to which another company is linked via the personList field (Bug #61715).
* Replaced the 'important' icon in a contact's history (Bug #61766).
* Fixed issue with updating a database with a missing stamp column (Bug #61838).
* Fixed issue with 'Error 500. Internal server error' when going to user's LDAP profile (Bug #61966).
* Fixed issue with infinite loading of files when creating them in a private room (Bug #61517).
* Fixed issue with incorrect width of text fields on the Settings > Integration > SSO (SaaS) page (Bug #61979).
* Fixed issue with Restore default settings action working incorrectly for LDAP Settings (SaaS) (Bug #49668).
* Fixed issue when synchronization with the LDAP server is enabled, the "+" sign is removed from the Primary Mobile Phone attribute in the user card (SaaS) (Bug #61986).
* Fixed issue with Dropdown menu styles in LDAP Settings > Auto Sync not adapted to dark theme (SaaS) (Bug #61974).
* Fixed issue with RabbitMQ not starting (Bug #1323).
* Fixed numerous issues for API documentation (Bugs #61734, #61735, #61737, #61740, #61742).
* Fixed issue with backup for s3: split large file uploads into pieces.
* User contact data template for telegram has been changed, now it is a http://t.me/{0} link with username substitution.
* Added redirect to the wrongportalname website page with the referer parameter (for correct operation when transferring the region from com to the co domain).
* Blocked display of user statistics until Quota is recalculated.
* Quota: added user recalculation by portal.
* Improvements for building server versions.

## Version 12.5.0

### General portal changes

* Logins and user actions can now be stored to syslog via nlog.
* Removed the unused 'department' column from the 'core_user' Sql table.
* Optimized backup.
* Removed the unused 'SendNoticeCallback' parameter.
* Updated nlog to v5.0.0.
* Added the ability to log in via SaslMechanismNtlm to SMTP settings.
* Added new icons for placeholder pages.
* Added the ability to set the number of login attempts, blocking time, and check period.
* Added the ability to set allowed IP addresses for users and admins separately in the IP Security setting. Added support for CIDR masking.
* Added API methods for changing email/password without email activation.
* Added the ability to set trusted IP addresses for which two-factor verification will not be performed, as well as to add users or groups for which two-factor verification will be performed. Added support for CIDR masking.
* Added the ability to add self-signed certificate for SSO, WebDAV.
* Optimized Feed operation.
* Changed settings for connecting third-party storages. Added tooltips for fields. Added the 'Server Side Encryption Method' block for Amazon AWS S3.
* Added Dark theme.
* Added logos for dark theme in the White Labeling section. Logos for the About page are now moved to the Control Panel.  
* Added the new 'Lead' field to the team template.
* Added Impersonation Settings which allow the portal owner and full access administrators to log in to the portal on behalf of any user.
* Added the ability to set the password maximum length.
* Added the ability to set the memory quota for users.
* Added the ability to recalculate the space used by users.
* Added policies for working with third-party services, such as bar, helpcenter, moneyconverter. Policies unclude Retry policy, Timeout policy, Circuit policy.

### Documents module

* Added thumbnails adaptive layout.
* Improved conversion of password protected files.
* Add the ability to upload a structure of empty folders via drag-and-drop.
* Hidden Private Room in dialog windows for files/folders selection.
* Added empty file templates in Armenian, Basque, and Malay.
* Removed Wordpress, EasyBib from the Third-Party services settings.
* Fixed the issue when documents, spreadsheets, presentations created in Google Drive are not correctly recognized when opening on the portal.
* Errors are now displayed on the DeepLink page.
* Added the ability to create multiple external links to files and folders.
* Added the ability to set password protection for external links.
* Added the ability to set time limit for external links.
* Added the ability to insert data from a third-party source to a spreadsheet (compartible with Docs v.7.3).
* Common viewer is used instead of live viewer for anonymous users.
* Added the mobile view for the Sharing settings dialog window in the flat mode.

### Calendar module

* Moved scripts from the client to the .cs file.

### Mail module

* Changed the request type of the mail/filters/check API method from GET to POST.
* Added the ability to log in via SaslMechanismNtlm.
* Added the Drafts folder synchronization for ImapSync.

### CRM module

* Added the Angolan Kwanza (AOA) non-convertible currency.
* Added the Venezuelan Bolivar Soberano (VES) convertible currency.

### Control Panel

* Changed API methods for migration, implemented progressQueue.
* Changed settings for connecting third-party storages. Added tooltips for fields. Added the 'Server Side Encryption Method' block for Amazon AWS S3.
* Added logos for dark theme in the Branding section. Logos for the About page are now separate fields in the Advanced tab.
* Added the ability to set the portal memory quota.

## Version 12.1.0

### General portal changes

* Fixed issue with processing mail messages containing a calendar event in the attachment (Bug #58533).
* Fixed issue with group filtering (Bug #58230).
* Fixed issue with loading of the currency convertor in CRM Opportunities (Bug #58651).
* Fixed issue with synchronizing between portal and mail client when grouping email chains (Bug #57194).
* Fixed issue with synchronizing between portal and mail client when filtering emails (Bug #57201).
* Fixed issue with Mail Services when installing on Ubuntu 22.04 (Bug #58608).
* Fixed issue with Redis and MySQL service dependencies in the systemd service files (Bug #58550).
* Fixed issue with functioning of the god service /etc/god/conf.d/services.god (Bug #58547).
* Fixed issue with installing on Ubuntu via Hetzner hosting (Bug #58609).
* Updated mysql-apt-config package (Bug #58374).
* Fixed issue with opening a document after successful 2FA authorization (Bug #58709).
* Fixed issue with sending notification emails about updates in the calendar event when changing it through the editing window (Bug #58726).
* Fixed issue with deleting a folder with several emails (Bug #58921).
* Fixed issue with unread/read emails when synchronizing between portal and mail client (Bug #57173).
* Fixed issue with installing a DEB package on Ubuntu 20.04 and Debian 10 (Bug #58920).
* Fixed SSL issues with WarmUp of Docker installations under https when restarting the container.  
* API methods for changing email/password without mail activation added.
* The autocomplete="new-password" attribute is set in the password setting field when creating a user.
* Fixed displaying of the notification window for unactivated emails.
* Fixed Feed styles for mobile devices.
* API rebranding methods (for the default tenant) are available only to the administrator.
* Updated MySQL version to 8.0.30.
* Added mysql-apt-config update.
* Added automatic getting of the MySQL repository version.
* Corrected OCI for Ubuntu 22.04.
* Fixed restart confirmation for Ubuntu Jammy.
* Fixed and updated Node.js installation for Ubuntu Jammy.
* Fixed and updated MySQL installation for Ubuntu Jammy.
* Added always restart for Node.js/DotNet services.
* Removed restart of mail services by using the god service.
* Updated monoserve.service.

### Documents module

* Added push notifications for events related to folders and files. Subscription/unsubscription within mobile applications.

### Mail module

* Added functionality for receiving emails from custom folders and their synchronization with the Mail Server for ImapSync.
* Fixed issue with emails moved to the custom folder (Bug #58742).
* Fixed issue with synchronizing between portal and mail client when making changes in the client (Bug #55800).
* Fixed issue with lost email in the web when moving it to the custom folder in the synchronized email client (Bug #56745).
* Fixed issue marking an email as important when sending/receiving it from a custom mailbox (Bug #56932).

## Version 12.0.1

### General portal changes

* Fixed issue with deep linking.
* Fixed Migration feature.
* Storing indexing status as a list instead of a field for ElasticSearch API.
* Enhanced Backup service.
* Fixed unavailable Feed after data restoring (Bug #58135).
* Fixed invalid ‘Fork me on GitHub’ button link in the Sample (Bug #57588).
* Portal users and guests are no more able to make the API SMTP settings request (Bug #57244).
* Fixed issue with displaying a new company name (after changing White Label settings) in the password change emails and other similar requests (Bug #56435).
* Personal user info is hidden from those who has no access to the People module (Bug #57851).
* A drop-down menu in the calendar added when clicking on a month/year (Bug #54767).
* Displaying an input cursor in the search bar when filtering with a drop-down list of users (Bug #57317).

### Server installations

* Removed storage_root parameter when starting Mail services.
* Fixed dotnet dependency installation.
* Fixed msttcore-fonts installation.
* Upgraded elasticsearch to version 7.16.3 in packages.
* Fixed memory allocation for elasticsearch.
* Fixed issue with config files after updating on Windows (Bug #50992).
* Fixed issue with security configs when making GET requests (Bug #57254).
* Fixed issue with mail services when installing on RedHat 8.6 and Centos 8 (Bug #57624).

### Documents module

* Changed frequency of displaying a hint page when opening a file in Private Room from the web version.
* Updated layout of the files list due to new Favorites icons.
* Restriction to open DOC files for editing on mobile devices (Bug #57373).
* Added new filtering parameters searchInContent and withSubfolders to API methods.
* Changed type of some API methods from GET to PUT/POST (Bug #57371).
* Users and their emails are not displayed in mentions if there isn’t access to the People module (Bug #58037).

### Mail module

* Added ‘On top’ button when zooming in empty folders (Bug #57671).
* Fixed issue with sending an email with a link to a non-editable file in the trial portal version (Bug #54637).
* Fixed DOCXF and OFORM icons when attaching files as a link (Bug #57657).
* Fixed issue with forwarding emails added to the Templates (Bug #57466).
* Fixed issue with filter settings (Bug #57200).
* Fixed issue with selecting emails as read/unread (Bug #57390).
* Fixed issue with the pop-up notification about a disabled account when printing out emails (Bug #57324).
* Fixed issue with re-opening of email signature settings (Bug #57322).
* Fixed issue with using the filter when selecting a date by custom period (Bug #57510).
* Fixed issue with downloading a file when clicking on the .docxf/.oform format link in Chrome (Bug #57651).

### People module

* Fixed Active connections check.
* Fixed issue with generating a CardDav book in case user emails contain capital letters (Bug #57831).
* Updated drop-down tooltip when setting a password (Bug #57673).
* Fixed issue with the https link in the invitation email when importing users (Bug #57519).

### Projects module

* Fixed issue with displaying the Time Tracking entry after its creation (Bug #57901)
* Fixed issue with closing CKEditor window after editing a task title (Bug #57625).
* Restriction of using XSS script in the milestone title (Bug #57559).
* Restriction of using XSS script in the Gantt Chart status (Bug #57256).
* Fixed issue with the incorrect link to the re-opened task in Telegram notifications (Bug #58107).
* Fixed issue with custom task status when re-opening it (Bug #57140).
* Administrator is automatically added to the project team when administrator assigns a task to themselves (Bug #57052).
* Fixed issue with filtering overdue milestones (Bug #57356).
* Fixed issue with filtering tasks when changing the Responsible: Me filter (Bug #57354).
* Fixed displaying of date format in Gantt Chart to match the format set on the portal (Bug #57370).

### CRM module

* Restriction of using XSS script in the Products & Services settings (Bug #57242).
* Fixed issue with the drop-down list in Invoices when page scrolling (Bug #57578).
* Removed ‘Show total amount’ link for deals without a budget (Bug #57386).

### Calendar module

* Updated functionality of attaching files from the Documents module. 
* Restriction to access events of other users using the historybyid.json method (Bug #58057).
* Fixed issue with unsubscribing from the event (Bug #58118).
* Restriction of using XSS script in To-do list (Bug #57307).
* Fixed issue with the doubled window in the mini-calendar when selecting a month/year (Bug #57480)
* Fixed issue with the CalDav link when the HTTPS certificate is activated (Bug #53265).

### Control Panel

* Fixed issue with brand logos after updating in the Docker installation (Bug #57331).
* Fixed issue with data import from Google Workspace in case the archive contains incorrect meta-information files (Bug #57617).

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
