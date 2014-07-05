-- res_authors
CREATE TABLE IF NOT EXISTS "res_authors" (
  "login" varchar(150) NOT NULL COLLATE NOCASE,
  "password" varchar(50) NOT NULL COLLATE NOCASE,
  "isAdmin" INTEGER NOT NULL DEFAULT 0,
  "online" INTEGER NOT NULL DEFAULT 0,
  "lastVisit" datetime DEFAULT NULL,
  PRIMARY KEY ("login")
);


-- res_authorsfile
CREATE TABLE IF NOT EXISTS "res_authorsfile" (
  "authorLogin" varchar(50) NOT NULL COLLATE NOCASE,
  "fileid" INTEGER NOT NULL,
  "writeAccess" INTEGER DEFAULT NULL,
  PRIMARY KEY ("authorLogin","fileid")
);
CREATE INDEX IF NOT EXISTS "res_authorsfile_res_authorsfile_FK2" ON "res_authorsfile" ("fileid");


-- res_authorslang
CREATE TABLE IF NOT EXISTS "res_authorslang" (
  "authorLogin" varchar(50) NOT NULL COLLATE NOCASE,
  "cultureTitle" varchar(20) NOT NULL COLLATE NOCASE,
  PRIMARY KEY ("authorLogin","cultureTitle")
);
CREATE INDEX IF NOT EXISTS "res_authorslang_res_authorslang_FK2" ON "res_authorslang" ("cultureTitle");


-- res_cultures
CREATE TABLE IF NOT EXISTS "res_cultures" (
  "title" varchar(120) NOT NULL COLLATE NOCASE,
  "value" varchar(120) NOT NULL COLLATE NOCASE,
  "available" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("title")
);


-- res_data
CREATE TABLE IF NOT EXISTS "res_data" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "fileid" INTEGER NOT NULL,
  "title" varchar(120) NOT NULL COLLATE NOCASE,
  "cultureTitle" varchar(20) NOT NULL COLLATE NOCASE,
  "textValue" text COLLATE NOCASE,
  "description" text COLLATE NOCASE,
  "timeChanges" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "resourceType" varchar(20) DEFAULT NULL COLLATE NOCASE,
  "flag" INTEGER NOT NULL DEFAULT 0,
  "link" varchar(120) DEFAULT NULL COLLATE NOCASE,
  "authorLogin" varchar(50) NOT NULL DEFAULT 'Console' COLLATE NOCASE
);
CREATE UNIQUE INDEX IF NOT EXISTS "res_data_id" ON "res_data" ("id");
CREATE INDEX IF NOT EXISTS "res_data_dateIndex" ON "res_data" ("timeChanges");
CREATE INDEX IF NOT EXISTS "res_data_resources_FK2" ON "res_data" ("cultureTitle");


-- res_files
CREATE TABLE IF NOT EXISTS "res_files" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "projectName" varchar(50) NOT NULL COLLATE NOCASE,
  "moduleName" varchar(50) NOT NULL COLLATE NOCASE,
  "resName" varchar(50) NOT NULL COLLATE NOCASE,
  "isLock" INTEGER NOT NULL DEFAULT 0,
  "lastUpdate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS "res_files_index1" ON "res_files" ("resName");


-- res_reserve
CREATE TABLE IF NOT EXISTS "res_reserve" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "fileid" INTEGER NOT NULL,
  "title" varchar(120) NOT NULL COLLATE NOCASE,
  "cultureTitle" varchar(20) NOT NULL COLLATE NOCASE,
  "textValue" text COLLATE NOCASE,
  "flag" INTEGER NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX IF NOT EXISTS "res_reserve_id" ON "res_reserve" ("id");
CREATE INDEX IF NOT EXISTS "res_reserve_resources_FK2" ON "res_reserve" ("cultureTitle");



