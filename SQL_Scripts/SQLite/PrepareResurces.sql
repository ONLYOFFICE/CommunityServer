delete from res_data where resourceType = 'file';
delete from res_files where projectname = 'Addons' and modulename in ('Organizer','Todo','QuickLinks');
delete from res_files where projectname = 'Files' and modulename = 'Editor';
delete from res_files where projectname = 'Community' and modulename = 'Photo';
delete from res_files where projectname in ('TeamLab Editors', 'Miscellaneous', 'Scanner','Tools','WebApps','Affiliate');
delete from res_data where fileid not in (select id from res_files);
delete from res_files where not exists(select fileid from res_data where fileid = res_files.id);

/* res_data нужно:
insert into res_cultures (title, value, available) values 
INSERT INTO `res_cultures` (`title`, `value`, `available`) VALUES 

insert into res_files (id, projectname, modulename, resname) values 
INSERT INTO `res_files` (`id`, `projectName`, `moduleName`, `resName`) VALUES 

begin transaction;
insert into res_data (fileid, title, culturetitle, textvalue) values 
INSERT INTO `res_data` (`fileid`, `title`, `cultureTitle`, `textValue`) VALUES 
commit transaction;

- добавить столбцы
- убрать `
- заменить \' на ''
- заменить \n') на ') (несколько проходов)
- заменить \r') на ') (несколько проходов)
- заменить   ) на   (2 пробела на 1, несколько проходов)
- заменить \\r\\n на \r\n (включить расширенный режим)
- заменить \\n на \r\n (включить расширенный режим) */