%global package_header_tag_summary "Business productivity tools."
%global package_header_tag_name onlyoffice-communityserver
%global package_header_tag_url http://onlyoffice.com/
%global package_header_tag_vendor Ascensio System SIA
%global package_header_tag_packager Ascensio System SIA <support@onlyoffice.com>
%global package_header_tag_requires mono-complete >= 6.8.0, nginx >= 1.9.5, mysql-server >= 5.7.0, wget, mono-webserver-hyperfastcgi, nodejs >= 12.0.0, redis >= 3.0.0, elasticsearch = 7.16.3-1, python3 >= 3.6, ffmpeg, jq, dotnet-sdk-7.0
%global package_section_description "Community Server is a free open-source collaborative system developed to manage documents, projects, customer relationship and emails, all in one place."
%global package_services god monoserve monoserveApiSystem onlyofficeSocketIO onlyofficeThumb onlyofficeTelegram onlyofficeBackup onlyofficeFeed onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog onlyofficeMailCleaner onlyofficeMailImap onlyofficeStorageMigrate onlyofficeStorageEncryption onlyofficeUrlShortener onlyofficeWebDav onlyofficeFilesTrashCleaner onlyofficeRadicale onlyofficeThumbnailBuilder onlyofficeSsoAuth

%global package_sysname onlyoffice

%global nginx_user nginx
%global service service
%global semanage semanage
%global getenforce getenforce
%global nginx_conf_d /etc/nginx/conf.d

%include common.spec

%changelog
* %{package_header_tag_date} %{package_header_tag_vendor} <support@onlyoffice.com> %{package_header_tag_version}-%{package_header_tag_release}
- Added the ability to receive portal notifications via Telegram;
- Added the ability to make an addon (Mail, Chat, Calendar) a default portal page;
- Added the additional license check when replacing ONLYOFFICE in the About this program window;
- Added the vsyscall check to the installation scripts when installing Mail Server on Debian with kernel 4.18.0 and later;
- Updated copirights in the source files;
- Added the ability to connect the Mail.ru, VK and Yandex applications for authorization;
- Added new icons and texts in the welcome placeholders of the empty modules;
- Added icons to context menus;
- Added two scrolling areas on the page: navigation and content;
- Reworked the mechanics for displaying messages on the authorization page;
- Updated Elasticsearch to v.7.4. Added the possibility to rebuild the index.
- Updated Mono to v.6.8, updated builds, improved performance;
- Added the file encryption at rest feature for server versions;
- When requesting the password recovery, it's not possible to check if an email address is used on the portal;
- It is prohibited to specify the current password when changing a password;
- Updated minimal password length to 8 characters;
- The password complexity check is now performed in the client-side browser;
- Added password hashing with the PBKDF2 algorithm when transmitting passwords to the server;
- Added the Sign in to domain option on the authorization page for the portals where the LDAP Authentication is enabled;
- Added a request for subscription to newsletters in the free Community version installation wizard;
- Removed the link to the forum;
- Added data cleaning before restoring backup;
- When the license expires, Document Server updates are blocked in the installation;
- Portals with an expired license and Enterprise portals with a free default license are not blocked;
- In the free Community version, a block with a proposal to install Enterprise Edition is added;
- The Payment page redirects to the Control Panel (if it is installed);
- Added the `-it`, `--installation_type` parameter to the installation scripts. The possible values are `GROUPS | WORKSPACE | WORKSPACE_ENTERPRISE`.
- Changed the note about the mention in the comment pop-up window depending on the ability to provide access to the document;
- Added the support for the .webp images;
- Using a new MailMerge API for the editors;
- Opening the default portal page when clicking to the logo in the upper left corner of the editors interface;
- Added the ability to connect kDrive via WebDav;
- Added the Favorites section, added the ability to add files to favorites via context menu;
- Added the Recent section;
- Added translations for the EasyBib and WordPress plugins;
- Added the ability to provide the Custom Filter access rights for spreadsheets;
- Replaced an empty tab with a static page containing the logo when creating a new file;
- If the DocuSign service is not connected, the instructions are displayed when clicking the Sign with DocuSign context menu option;
- Added a new Private Room section;
- If an administrator disabled the Allow users to connect third-party storages option, they also cannot connect third-party clouds;
- Added a separate section for quick access to admin settings on the left panel;
- Added links to download desktop and mobile apps.
- Added empty files for creation in Japanese (ja-JP).
- Added the Make a VoIP call action in the contact context menu;
- Removed the unnecessary Quantity field when creating a new invoice item;
- Added the ability to enter decimal fractions with two decimal places in the Discount field when creating an invoice.
- Added a new scroll mechanism for the screen width greater than 1200 px.
- Improved the Calendar API security;
- Reworked the authorization method for the CalDAV calendars;
- Reworked the Radicale plugins for the new version 3.0.