# Change log

## Version 12.7.1

### General portal changes

* Fixed the issue with receiving an instance-id for AMI.
* Made changes for requests to docbuilder using the shardkey parameter.
* Fixed the critical issue in the AutoCleanUp service (Bug 71281).
* Fixed the issue with missing the Comment tab in PDF files (Bug 70867).
* Hidden buttons for creating a PDF form in the Private room.
* Enabled SharePoint connection on UNIX systems.
* Fixed the issue with the JWT secret during update (Bug 71036).
* Fixed the issue with single-component installation updates (Bug 70862).
* Fixed the issue when the POST /api/2.0/settings/customnavigation/create method allows passing an unlimited number of characters in the "label" parameter (Bug 71805).
* Fixed passing of the logoDark parameter to the editor.
* Fixed the ability to specify an unlimited number of characters in all SMTP parameters (Bug 71859).
* Fixed the ability to set port to "999999999" in Workspace SMTP parameters (Bug 71860).
* Fixed the issue when the POST /api/2.0/settings/whitelabel/save method allows passing an unlimited number of characters to the "logoText" string in Workspace (Bug 71857).
* Fixed the issue with the unlimited length of the share link name and password (Bug 71862).
* Fixed the error from log ( ERROR ASC.Api.ApiSetup - method error: http://127.0.0.1/api/2.0/project/securityinfo.json - Server error System.Web.HttpException (0x80004005): Forbidden ).
* Fixed the error from log ( ERROR ASC.Api.ApiSetup - method error: https://127.0.0.1/api/2.0/settings/security.json - Server error ASC.Common.Security.Authorizing.AuthorizingException: "[username]" access denied "Edit Portal Settings" ) (Bug 71913).
* Added deletion of the synchronization client in case of a critical error in ImapSync (to allow reconnection).
* Fixed the issue when the bottom part of the title in the event and task viewing window is cut off (Bug 47353).
* Fixed the issue with configuring Windows feature (Bug 71923).
* Fixed duplicate Downloading text in the prerequisites installation UI.
* Fixed the issue when Google connection as a Storage is not available on Linux distributions (Bug 72205).

### Documents module

* When creating a new document, a template with the Letter page format will be taken for the en-US language. For languages ​​without templates, a document with the A4 page format will be taken.
* Added empty file templates for creation in the following languages: fi-FI, he-IL, nb-NO, sl-SI.
* Starting documents now have titles in the corresponding language.

### Control Panel

* Fixed the issue when the /controlpanel/https page crashes with the 502 error after restarting the container. (Bug 71794).

## Version 12.7.0

### General portal changes

* Using the latest version of the Facebook API without specifying a version number. Previously, version 2.7 was used, which is outdated.
* Added Zoom login provider.
* Added the mysql port setting in the UrlShortener service config.
* Added methods for getting entities in parts and sorting by id.
* Fixed the dark theme issues: the deeplink dialog window for opening a file on mobile devices; the "On top" button in entity lists on a narrow screen; the letter container in the Mail module; the dialog window for mentioning a user via @ in ckeditor.
* Removed font styles when pasting text from MS Word to ckeditor.
* Disabled Facebook until the application is validated.
* Disabled Google Drive until the application is validated.
* Increased the length of the short link.
* Fixed the issue with indicating an incorrect maximum number of characters in the hint. TenantDomainValidator: A subdomain can be up to 255 characters long, but if you have multiple levels in your subdomain, each level can only be 63 characters long (Bug 66512).
* Fixed the issue when the /ajaxpro/ASC.Web.Studio.UserControls.Common.PollForm.PollForm,ASC.Web.Studio.ashx method allows voting through BS Turbo Intruder multiple times (Bug 66500).
* Fixed the issue when the api/2.0/settings/customnavigation/getall method is available to users without administrator rights (Bug 66647).
* Fixed errors in the Favorites folder after changing the settings for connecting a third-party storage, if its file was added to Favorites. (Bug 66624).
* Fixed issue when the /api/2.0/settings/security method is available to users without administrator rights. (Bug 66663).
* Fixed the issue with missing backend validation for the Success probability. (Bug 66667).
* Fixed the typo in the link to the Organization profile page (?type=organisation_profile). (Bug 66715).
* Fixed the issue when the /api/2.0/settings/security/loginSettings method allows seting values ​​of more than 4 characters for Brute Force Protection in Workspace. (Bug 66756).
* Fixed the issue when the /ajaxpro/ASP.usercontrols_management_cookiesettings_cookiesettings_ascx,ASC.ashx method allows setting a value of more than 4 characters for Session Lifetime in Workspace. (Bug 66709).
* Fixed the "Could not resolve current tenant" error when exporting a large number of contacts. (Bug 62984).
* Fixed the issue when contact avatars are available via direct link when access is closed for the user. (Bug 66708).
* Fixed the issue when the /fckuploader method allows users without access to the Community module to upload images. (Bug 66710).
* Fixed the issue when a user without access to the mail server can use the mailserver/domains/common method. (Bug 67100).
* Fixed the translation of the Unblock/Check-in button in Chinese. (Bug 67464).
* Fixed the issue when the /ajaxpro/ASC.Web.Studio.UserControls.Common.PollForm.PollForm,ASC.Web.Studio.ashx method allows a user with limited access to the Community module to vote. (Bug 67465).
* Fixed the issue with the white background of a document title on the Deeplink page with the dark interface theme. (Bug 67529).
* Fixed the issue with the "index was outside the bounds of the array" error when generating reports. (Bug 67986).
* Added a loader in the manager when saving .docxf via Save as PDF form. (Bug 66529).
* Fixed the issue when a user without administrator rights can download the backup file via a direct link. (Bug 68162).
* Switched FFmpeg-installer to use pre-downloaded file. (Bug 67421).
* Updated MySQL to v8.0.37. (Bug 68348).
* Fixed the issue with missing the Save as PDF form button in the context menu of the .oform file. (Bug 68646).
* Fixed the issue when the /app/onlyoffice/CommunityServer/data/Products/Files/00/00/01/temp/ folder is added to the backup file. (Bug 68392).
* Fixed the issue when the value "undefined" appears in the URL after selecting yourself as a responsible for a project. (Bug 68942).
* Fixed the issue when the /api/2.0/portal/usedspace method is available to users without administrator rights in Worksapce. (Bug 68990).
* Fixed the issue with the advanced sorting list button in the Recent folder. (Bug 68707).
* Fixed the issue with missing the link to download the temporary Backup file after refreshing the page. (Bug 67877).
* Fixed the issue with upgrading Workspace to Enterprise Edition (Bug 68561).
* Fixed the issue with null values ​​of imported data in a link to a third-party resource instead of a warning. (Bug 69914).
* Fixed the issue when the link to change email remains active after sending a message to change email to another address. (Bug 68836).
* Fixed the issue when the file list does not scroll when selecting with the left mouse button held down. (Bug 68654).
* Replaced Twitter icon with actual X icon. (Bug 70255).
* Fixed the issue when files without an extension are not included in the backup archive. (Bug 70294).
* Fixed the "DOMNodeInserted" error while creating message. (Bug 70295).
* Fixed hanging the Mail module when changing the number of displayed letters /page_size=1000/. (Bug 68786).
* Fixed the issue when documents are not available after performing restore. (Bug 70348).
* Fixed the issue when the POST /api/2.0/settings/rebranding/company method allows passing to the About window the email and website addresses using punycode. (Bug 70257).
* Fixed the issue when the /api/2.0/settings/tfaappcodes method allows viewing Backup codes after disabling two-factor authentication in Workspace. (Bug 70390).
* Fixed the issue when the PUT /api/2.0/portal/portalrename method allows renaming a portal with a space before the name. (Bug 70391).
* Fixed the issue with the Enterprise Edition license. (Bug 70051).
* Fixed the issue when the /api/2.0/portal/getshortenlink method allows substituting a malicious site into the shortlink. (Bug 70392).
* Fixed the issue when the PUT /api/2.0/people/{ID} method allows allows changing a type of a deactivated user. (Bug 70395).
* Fixed the issue when the api/2.0/group/{groupid}/manager method allows assigning a blocked user as a group manager. (Bug 70418).
* Fixed the issue with a timeout expected when entering an incorrect login/password for the mail/gmail mailbox. (Bug 68740).
* Fixed the issue when marking a To-do task as completed or uncompleted changes its start time (DTSTART label). (Bug 68970).
* Fixed the issue with missing logos of the social network (X) Twitter in the dark theme of the portal. (Bug 70457).
* Fixed the issue when the backup file created in the server version for Windows is not supported for importing to DocSpace (Bug 70558).
* Fixed the issue when there is no possibility to connect Google as a storage in the server version for Windows (Bug 70557).
* Fixed the issue when most of the ONLYOFFICE services do not start after updating RPM packages. (Bug 70583).
* Fixed the issue when the installation on Ubuntu 20.04 / Debian 11 fails with the error "error: metadata-generation-failed" (Bug 70596).
* Fixed the issue when the Full-text Search section content is not loaded  with the error "500 (Internal Server Error)" (Bug 70556).

### Documents module

* Updated the list of the editors error codes.
* The djvu, oxps, pdf, xps formats will be sent to the editor with the documentType=pdf  indication (editor 8.0 or higher is required). The docxf and oform formats will be sent to the editor with the documentType=pdf indication (editor 8.1 or higher is required).
* The Protected panel is hidden for the anonymous so that he cannot set a password for the file.
* When sharing via an external link for viewing, the chat is not available and the username is not requested.
* User avatars are transferred to the editor for authorized users if there is access to the People module.
* In a mobile browser, when opening the editor, the ONLYOFFICE logo will not be displayed if the license allows (editor 8.1 or higher is required).
* Files in djvu and oform formats can be converted to pdf (editor 7.1 or higher is required).
* When creating a form, an empty PDF form file will be opened for editing. In the context menu of any PDF file there are the following buttons: edit (the mode as for docxf with changing all contents and saving), fill in (the mode as for oform with filling in fields and saving) and view (the mode of opening for viewing with the ability to fill in without saving) (editor 8.1 or higher is required). The document manager does not distinguish a PDF file from our PDF form.
* For the docxf file, the "Save as PDF form" button is kept in the context menu. A similar button is added to the context menu of the oform file.
* Creating a PDF form from docx works through conversion, editor 8.1 or higher is required.
* Support for referencing a portal file in a spreadsheet in the IMPORTRANGE formula (editor 8.1 or higher is required).
* Added the sr-Cyrl-RS empty file.
* Updated empty file templates in English.
* Added the version parameter to GetPresignedUri.
* Added the shardkey parameter for requests to editors.

### Control Panel

* Fixed the issue with missing the link to download the temporary Backup file after refreshing the page. (Bug 70341).
* Fixed the issue when a curl request to the Control Panel stops the service on Windows. (Bug 70049).
* Fixed the issue when the description of the dark theme logo indicates that the logo is for the light theme. (Bug 68858).
* Added a checkbox when creating a backup for migration to DocSpace. 

## Version 12.6.0

### General portal changes

* Edited sources build.
* Removed mail services transferred to a separate repository (#1378).
* Hidden tariff for 1 and 3 years in SaaS.
* Fixed issue when the ‘With opportunities’ filter does not display contacts that are specified as primary ones in Opportunities but are not specified on the Participants tab (Bug 61833).
* Fixed issue when the drop-down menu in the counter of the space occupied by a user goes out of the browser frame (Bug 61911).
* Added the ‘Copy email’ button for actions with user’s email (Bug 60814).
* Fixed issue when some GUI elements are displayed incorrectly when increasing the font in the browser settings (Bug 62342).
* Fixed issue when the ‘Color’ caption in the calendar editing window from the ‘Other Calendars’ list is formatted in bold (Bug 58850).
* Fixed issue when the uploaded contact logo does not displayed in the contact list (Bug 62378).
* Fixed issue with using the system theme by default for new users (Bug 62067).
* Fixed issue when the filter by group does not work (Bug 58230).
* Fixed issue when the request to upload an image to the server can be performed without cookie (Bug 62933).
* Fixed issue when the ‘Folder title’ field is missing in the ‘Change connection settings’ window for some third-party storages (Bug 62617).
* Fixed issue with an excess dividing line in the menu for a deactivated external link (Bug 62620).
* Fixed issue when the ‘Advanced settings’ menu differs in the Sharing Settings window of the manager and in the editor in the Common documents (Bug 62666).
* Added the ability to restrict access rights to the application files for the Others group (Bug 61602).
* Fixed issue with redirect to the portal main page when opening Control Panel after a day on Ubuntu 22.10 (Bug 62076).
* Fixed the Server-Side Request Forgery (SSRF) vulnerability (Bug 55994).
* Fixed the 'Could not resolve current tenant' error when exporting a large number of contacts (Bug 62984).
* Fixed issue when the password recovery link does not work in EU and SG regions (Bug 62126).
* Replaced the mysql_native_password authorization plugin with a modern one (Bug 62854).
* Fixed issue with the ability to send requests to ApiSystem when expanding the default machinekey (Bug 62489)
* Implemented the recalculation progress bar (Bug 60900).
* Fixed issue when the “To” time is set incorrectly when selecting an area for an event in the Day, Week viewing mode (Bug 62118).
* Fixed issue with the direct link to connect Gmail Gmail via oauth (addons/mail/oauth.aspx) (Bug 62813).
* Fixed issue with the XSS in the “Your subscription has expired” banner (Bug 63457).
* Fixed issue with the XSS sent in the attachment file is triggered (Bug 62497).
* Fixed issue when the page scrolls down when clicking Home/End in the search field (toolbar) (Bug 63526).
* Fixed issue when Open Redirect is possible for an authentication link (refererurl) on the portal (Bug 63328).
* Fixed issue when the quick external link is available for administrators only (Bug 63550).
* Fixed issue with the 'Account' error when trying to log in as a blocked user (Bug 63629).
* Fixed issue when there is an ability to send the Change Email message to any participant under the user (Bug 63538).
* Fixed issue when logout from the portal occurs for the administrator logged in as a user when performing the ‘Log out from all active connections’ action (Bug 63630).
* Fixed issue when old indexes are not removed when upgrading the Elasticsearch version (Bug 63584).
* Fixed issue when the onlyofficeAutoCleanUp service is not removed after updating to v.12.5 (Bug 63684)
* Fixed the "/uploadComplete 500" error when uploading an archive (Bug 64024).
* Fixed issue when the large backup file is not removed from S3 (Bug 63838).
* Fixed issue when the PUT /api/2.0/settings/modetheme.json 401 (Unauthorized) error appears in the console when opening a folder via the external link if the dark theme is set in the system (Bug 64285).
* Fixed issue when the size of the created file is added to the statistics of the administrator who performed the backup after performing backup (Bug 63783).
* Fixed issue when the XSS is triggered by a direct link to a malicious SVG file (Bug 64203).
* Updated versions of all related components to the actual ones (nodejs, python, PostgreSQL) (Bug 62063)
* Fixed issue when the VC++ 2015 additional packages are not updated for older versions (Windows) (Bug 63771).
* Fixed issue with the HTML injection in the Recover Access message (Administrator Message Settings) (Bug 64685).
* Reduced the number of characters displayed in the action button title for the parent folder (Bug 64462).
* Fixed the "undefined (reading 'response')" error in the console when opening the Controls section (Bug 64592).
* Fixed issue when the page title does not return to the default value from Common Settings (Bug 64641).
* Fixed issue when the text in the URL bar blends into the background if the portal dark theme is set (Bug 64589).
* Fixed the "/imagescss/quotebord-b.gif 404 (Not Found)" error when quoting a message (Bug 64577).
* Fixed issue when closing / opening a task fails with the following error "Uncaught TypeError: Cannot read properties of null (reading 'unselectable')" onChrome (Bug 64562).
* Fixed the "ASC.Api.Exceptions.ItemNotFoundException (0x80004005): Item not found" error in logs after removing a project (Bug 64569).
* Fixed issue with the XSS via a direct link to the uploaded malicious file on the Wiki page (Bug 64580).
* Fixed issue with the XSS uploaded files in the forum topic (Bug 64571).
* Fixed the "/Images/volume.svg 404 (Not Found" error when opening an audio file (Bug 64551).
* Fixed issue when the Actions button for the parent folder no longer appears after deleting a subfolder via the Actions menu for the parent folder in the ‘Shared with me’ section (Bug 64465).
* Fixed issue with the ‘Sharing settings’ option in the drop-down menu for the parent folder with external access (Bug 64434).
* Fixed issue with the ‘Sharing settings’ option in the drop-down menu for the parent folder for a Guest in the ‘Shared with me’ section (Bug 64468).
* Fixed issue when the Actions menu button on the Participants tab of the main contact disappears after clicking (Bug 63888).
* Fixed issue when the function to reset the application is not displayed in the user profile if his IP address is listed in exceptions (Bug 64487).
* Fixed issue when the .sxc, .et, .ett, .sxi, .dps, .dpt, .sxw, .stw, .wps, .wpt formats are not converted via the manager (Bug 64705).
* Fixed issue when the .sxi format is not supported (Bug 64708).
* Fixed issue when there are no icons for the .sxc, .et, .ett, .sxi, .dps, .dpt, .sxw, .stw, .wps, .wpt formats (Bug 64706).
* Fixed issue when search by folder contents cannot be performed in the ‘Create form template from file’ window (Bug 64687).
* Fixed issue when the ‘Create form template from file’ window does not correspond to layouts (Bug 64647).
* Fixed issue when the authorization page uses the system theme, regardless of the one selected in the profile (Bug 64588).
* Fixed issue with the ‘Delete’ button in the menu of the parent folder for a third-party storage folder (Bug 64758).
* Fixed issue when the files moved from ‘My Documents’ to Google Drive are displayed in the ‘Common’ section in the statistics (Bug 64393).
* Fixed issue when the ‘No files in this folder’ placeholder does not appear after moving all files from a subfolder with external access (Bug 64644).
* Fixed issue when the folder deleted via the parent folder menu appears again on the ‘Shared with me’ page (Bug 64620).
* Fixed issue with the ‘Back’ button in a subfolder with external access via an inherited link (Bug 64643).
* Edited the Twitter functionality description (Bug 64858).
* Fixed the ‘An item with the same key has already been added.’ error when clicking the ‘Common’ folder counter on the user’s portal (Bug 64862).
* Fixed issue when an empty folder with the same name is duplicated when uploading from the local disk using the drag’n’drop method (Bug 62858).
* Fixed the "Index was out of range. Must be non-negative and less than the size of the collection." error in svcIndexer.data.log (Bug 64630).
* Fixed issue when all sessions are not logged out when changing an email by the portal administrator (Bug 61574).
* Fixed issue when the Mail Server data (connected domains and mailboxes) is not saved in the backup (Bug 33720).
* Fixed issue when the disk space quota is displayed in the profile under the user without rights (Bug 64909).
* Fixed issue when it’s possible to create a user on a custom mail domain without setting a password (Bug 65083).
* Fixed issue when the Imap Sync icon wraps to a new line when reducing the browser window (or setting Zoom to 200%) (Bug 65266).
* Fixed issue with an incoming event less than 30 minutes long from Google or Outlook comes with the wrong time (-1 hour) (Bug 65387).
* Fixed issue when emails from the custom domain are not sent after restoring the portal from SaaS to the server version (Bug 65462).
* Fixed issue when the MX record is not updated after restoring the portal (Bug 65460).
* Updated incorrect links (Bug 65278)
* Fixed issue when the date in the CRM filter is limited to December 2023 (Bug 65683).
* Fixed issue when creating a document in a task is performed with the following error: "Uncaught TypeError: Cannot read properties of null (reading 'document')" (Bug 65737).
* Fixed issue when the ICS file contains an incorrect PRODID attribute (Bug 65571).
* Fixed issue when backup to Google Cloud fails when enabling Public Access Prevention (Bug 65678).
* Fixed issue with endless loading Portal memory quota (Bug 66134).
* Fixed issue when the user who does not have access to the Community module can download any file from the Wiki (Bug 66298).
* Fixed issue with the black portal logo in the PDF Editor with the light theme (Bug 66304).
* Fixed issue when the /api/2.0/portal/startbackup method allows creating backup in My Documents (Bug 66307).
* Fixed issue when the POST /ajaxpro/ASC.Web.UserControls.Bookmarking.BookmarkingUserControl,ASC.Web.Community.ashx method allows a user who does not have access to the Community module to create a bookmark (Bug 66347).
* Fixed issue when the /api/2.0/security/activeconnections/logout/{loginEventId} method is available to users who do not have administrator rights (Bug 66390).
* Removed/replaced ONLYOFFICE Sample Form (.oform) on the portal (Bug 66407).
* Fixed error when converting XML (Bug 66491).
* Fixed issue with the loader when hovering over the Actions button of the parent folder again after deleting a subfolder via the same menu (Bug 66436).
* Added a new icon for the PDF Editor.
* Filling out PDF instead of OFORM is now used.
* Conversion DOCXF to OFORM is no longer supported.
* Added templates for empty files in ar-SA and sr-Latn-RS.
* The history when sharing for external users is now available only to authorized users.
* Closed the functionality for copying text from the editor when the DenyDownload option is set for a file.
* Fixed the ability to enable recaptcha for the login page. Added the description to the Help Center.
* Added display of an error in the title of the landing page for an external link to a file when access to it is closed.
* Edited the birthday page for several birthdays on the same day with long titles.
* Fixed bugs related to the dark theme (#1324, #1336, #1364, #1381, #1383, #1387, #1394, #1409)
* Fixed the bug with the ability to select several answers when voting in a poll with only one choice.
* Completely removed Twitter from CRM (#1371)
* Fixed the GetByFilter method (the target parameter was not passed into the request).
* Fixes for compatibility with DocSpace (#1358 ,#1359 ,#1363)
* Fixed the “ArgumentOutOfRangeException: The absolute expiration value must be in the future” error when specified zero blocking time.
* Added the .mhtml format icon.
* Added setting focus to the password field.

### Documents module

* File formats of the OOXML group (docm dotm dotx potm potx ppsm pptm xlsm xltm xltx) are opened for editing without conversion to docx, xlsx, pptx. They will be available for encryption in Private rooms (#1301). Now it’s also possible to share files for commenting – docm dotm dotx potm potx ppsm pptm xlsm xltm xltx, for reviewing – docm dotm dotx, for a custom filter – xlsm xltm xltx. (#1302)
* Added opening for new formats: sxc, et, ett, sxi, dps, dpt, sxw, stw, wps, wpt (Docs v7.4 required). (#1305)
* Conversion when loading and before opening a non-OOXML format is now performed to the ooxml format (not to docx, xlsx, pptx depending on the type). This way the files will be converted to the format selected by DS. For example, if the doc file contains a macros, it will be converted to docm, not to docx. (#1304)
* Users with access for reading are transferred to the editor to be able to specify full access to the protected region in the spreadsheet (Docs v7.4 required). (#1354)
* When adding data to a spreadsheet via an external link to another spreadsheet, the update is available from the edited file (Docs v7.5 required).
* Added the ability to open the editor for an external data source spreadsheet in a new tab if the data is inserted to the spreadsheet via an external link (Docs v7.5 required).
* Added templates for empty files for si-LK, ar-SA and sr-Latn-RS cultures.
* Added new formats for generating thumbnails (#1366)
* Added uiTheme in Desktop v7.5 when portal:login (For Bug 57821 - The color of the connected portal tab does not correspond to the interface theme).
* Generating a form template from a local DOCX file (#1362)
* Added a drop-down list with actions for a parent folder (#1367)
* Added a filter to the fileselector (#1385)
* The pdf format is now considered as fillable one instead of oform.
* Removed docxf to oform conversion.

### Mail module

* Fixed an error in recording auto-reply parameters in email services. Due to this error, a user accidentally did not receive emails if there was an overdue auto-reply.
* Fixed SQL requests for calculating counters of read messages in folders.
* Fixed SQL requests for working with emails in custom folders (moving/removing/adding tags).
* Fixed issue when the Mail Server data (connected domains and mailboxes) is not saved in the backup (Bug 33720).

### Control Panel

* Added the ability to restrict access rights to the application files for the Others group (Bug 61602).
* Fixed issue with redirect to the portal main page when opening Control Panel after a day on Ubuntu 22.10 (Bug 62076).
* Fixed retrieving data error when opening the backup page (Bug 63163).
* Fixed issue when backup with Mail is not performed after disabling and enabling encryption (added text about stopping services and the instruction to the Help Center) (Bug 64223).
* Fixed issue when features are not saved to the new tariff when setting a quota for the portal (Bug 65324).
* Edited sources build.

## Version 12.5.2

### General portal changes

* Fixed issue with Interface Theme and Contact Information arranging incorrectly upon reducing window width in People (Bug #62061).
* Fixed issue with an uploaded folder appearing only after reloading the page (Bug #61506).
* Fixed issue in CRM when address not appearing in the Invoice generated through the Invoice tab on the contact page (Bug #62050).
* Fixed issue in Calendar when an event from an attached ics file not synchronized via the CalDAV protocol (Bug #62048).
* Increased horizontal scroll in Desktop editors when connected to a portal (Bug #62153).
* Fixed issue with Show backup codes button would not working in People (Bug #62159).
* Fixed issue with downloading letters for LDAP users (Bug #58469).
* Fixed issue with installation/upgrade on Windows Server 2012.
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
