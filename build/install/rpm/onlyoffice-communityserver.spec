%global package_header_tag_summary "Business productivity tools."
%global package_header_tag_name onlyoffice-communityserver
%global package_header_tag_url http://onlyoffice.com/
%global package_header_tag_vendor Ascensio System SIA
%global package_header_tag_packager Ascensio System SIA <support@onlyoffice.com>
%global package_header_tag_requires mono-complete >= 6.8.0, nginx >= 1.9.5, mysql-server >= 5.7.0, wget, mono-webserver-hyperfastcgi, nodejs >= 12.0.0, redis >= 3.0.0, elasticsearch = 7.4.0-1, python36, ffmpeg, jq
%global package_section_description "Community Server is a free open-source collaborative system developed to manage documents, projects, customer relationship and emails, all in one place."
%global package_services god monoserve monoserveApiSystem onlyofficeSocketIO onlyofficeThumb onlyofficeTelegram onlyofficeBackup onlyofficeFeed onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog onlyofficeMailCleaner onlyofficeStorageMigrate onlyofficeStorageEncryption onlyofficeUrlShortener onlyofficeRadicale

%global package_sysname onlyoffice

%global nginx_user nginx
%global service service
%global semanage semanage
%global getenforce getenforce
%global nginx_conf_d /etc/nginx/conf.d

%include common.spec

%changelog
* %{package_header_tag_date} %{package_header_tag_vendor} <support@onlyoffice.com> %{package_header_tag_version}-%{package_header_tag_release}
- Added two-factor authentication using the authentication applications for code generation;
- Added the page with the list of links for mobile and desktop editor installations.
- Added the possibility to create administrators for the Mail module;
- Removed SMTP settings from the CRM module and added them to SaaS installations;
- Added the possibility to connect third-party providers for static data storage and CDN;
- Added the possibility to backup/restore from the local folder;
- Added the possibility to backup/restore using mysqldump (for server installations only).
- Replaced sphinx with Elasticsearch, adding the possibility to search by content in documents, mail and chat messages;
- Added analytics to the periodic SaaS mail messages;
- Added the generation of the unique machinekey when installing the Docker containers using script; the key is stored in the /app/onlyoffice/CommunityServer/data/.private hidden folder;
- Enabled caching using Redis;
- The thumbnail generation is done via a new service;
- Added Bearer authentication;
- Most icons are replaced with SVG format images to enhance their display on high DPI screens;
- Removed lots of outdated limitations for mobile devices.
- Added sorting of files in all sections, including Shared with me and Project documents;
- Added descending sorting by modification date by default;
- The search and sorting results are taken into account when opening images for preview;
- Added the filtering by media file formats.
- Added the possibility to share documents for commenting only;
- Added the possibility to share DOCX files for form filling only;
- Added the shortening of the link to the people outside portal in the sharing dialog window by user request only;
- Removed the possibility to rename files and add comments for guests.
- Added support for the new request signing to the document editing service (with JWT in request body);
- Updated the file/folder selection and moving using the mouse;
- Added the viewers for video and audio files;
- Added the display of the date in the file name when opening not the latest file version for viewing;
- When sending the file by mail from the Documents module, a draft is created with the file attached.
- Added the possibility to view AVI, MPEG and MPG files;
- If the portal has HTTPS enabled, the address of the editors is also checked for the presense of the secure HTTPS connection alternative.
- Added the possibility to Save as the documents to the portal from editors;
- Added the possibility to insert the images from the document manager to the editors;
- Added the search by the document title and content;
- Enhanced the checks of the connection to the editors settings;
- Added the possibility to upload the license file after the support and updates period end.
- When creating a milestone the project manager is selected by default instead of the first person on the list;
- Added the loader when performing the group actions;
- Added a separate filter for the No milestone tasks;
- Added the milestone icon and deadline date for the tasks with no deadline belonging to a milestone;
- Fixed the display of the number of project team in the common list;
- Replaced the autosuggest input with the advancedSelector for the project creation dialog;
- The order of tasks/milestones is now saved when creating a project.
- Added the report generation using document builder service;
- Added the setting to select the report storage place;
- Added the page with the list of the generated reports.
- Added the possibility to edit the documents when editing discussions;
- Added the possibility to attach documents when creating discussions;
- Updated the dialog with file selection to attach it to the task or discussion.
- The list export uses a separate thread with progress display.
- Updated the user avatar generation;
- Optimized the work with user list for a large number of users;
- Added the wizard for the mass user import;
- Updated the loading and parsing of CSV file, added the possibility to map the user fields manually;
- Updated the user editing interface available before adding them to the portal;
- Added the counters of the users ready for importing and import progress;
- Added the possibility to import files previously exported from various sources (Google, Yahoo or Thunderbird) as a CSV file;
- Added the possibility to create the email address at the domain connected to the Mail Server;
- Added the possibility to set the temporary password for portal access manually.
- Added a separate page to create, edit and move/delete the user folders;
- Added the link for faster user folder creation;
- Added the modal window to create/edit user folders with the possibility to select their nesting within each other;
- Added drag'n'drop for mail messages and folders;
- Added the new Move to button to move mail messages to folders;
- Added the possibility to quickly select several mail messages using the mouse;
- Added the indication for the unread mail messages in the user folders;
- When the folder is deleted all the mail messages from it will be moved to the trash.
- Changed the tags popup, now it is optimized for tablet devices.
- Added the new user filters/rules option for faster actions with mail messages;
- Added the new page for filters display, where it is possible to change the filter order, enable and disable them, apply to existing messages, edit/delete the filters;
- Added the new page for filter creation and editing, where it is possible to add filter name and trigger rules and actions, list the messages fitting the filter, apply the filter to existing messages.
- Added the request for delivery/read notification option when creating a mail message.
- Updated the file selection dialog when attaching the file to the mail message;
- Server code refactoring and optimization;
- Replaced the Log4net logging system with NLog;
- Replaced the sanitizer with the third-party DOMPurify XSS sanitizer;
- Fixed a lot of bugs.
- Code refactoring and optimization to speed up the Mail Server settings page opening;
- Redesigned modal windows and lists in them;
- Redesigned the wizard for domain connection in installations (four steps instead of five);
- Added the possibility to connect third-party clients;
- Added new Connection settings context menu option to show common connections settings;
- Added new Change password context menu option to change password by administrator;
- Added the possibility for the user to change the password and see the connection settings for the Mail Server mailbox;
- Added the possibility to send the mail messages when creating boxes on Mail Server and when changing passwords;
- Added the warning when the server uses self-signed certificates.
- Added permanent Fail2Ban ignoreip section for docker containers and its gateway;
- Added the possibility to create sieve rules for users inside /var/vmail/sieve/;
- Added new extension to sieve: sieve_extensions -> editheader;
- Exposed port 4190 for ManageSieve service;
- Added the possibility to run an external script from the /var/vmail/external.sh or /app/onlyoffice/MailServer/data/external.sh path;
- Changed the default FIRST_DOMAIN parameter;
- Replaced the hard-coded password used for the first mailbox with a random one;
- Removed old useless descriptions for iRedMail administrator console path;
- Added backticks to the 'CREATE DATABASE' commands;
- Added the possibility to send mail messages from alias_domain, alias_address and full alias;
- Added the imapsync command;
- Added the new Python scripts allowing to create mailboxes, change password and run imapsync batch in /usr/src/iRedMail/tools/scripts folder inside the onlyoffice-mail-server container;
- Added the python-pip and installation requirements.
- Fixed the bug with mailbox not being removed when using third level domain (issue: wrong domain regex);
- Fixed the bug with the Cannot load 1024-bit DH parameters from file /etc/pki/tls/dhparams.pem warning;
- Fixed the bug with the No such file or directory being shown in many cases;
- Fixed the bug with the ECHO_INFO: command not found warning.
- Redesigned the calendar;
- Added the possibility to create, delete, edit tasks from the list and from the calendar grid;
- Updated Ical.Net to the latest version;
- Added the possibility to sync new calendars via CalDAV protocol (for the calendars from My calendars section added after update to Community Server version 10.0 only);
- Fixed the problem with daylight saving.
- Removed the possibility to set a status;
- Fixed the problem with reconnections of several chat tabs open;
- Added the reconnection when the connection is lost, when exiting the sleep mode;
- Added the restoring of the previously opened chat tabs;
- Added the restoring and display of the unread messages when opening the chat;
- In web version only opened chat tabs are marked read, this is done to allow reading the unread messages on other devices.
- Added more fields mapped for the users loaded via LDAP: user photo, birthday, contacts, primary phone number;
- Added the setting to autosync LDAP on schedule;
- Added the possibility to give administrator rights to the user group at the portal via LDAP;
- Updated the rules for LDAP users.



