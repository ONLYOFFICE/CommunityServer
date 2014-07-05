Introduction
Storage cleaner is a tool for removing obsolete entries from the data storage. Resources of message (e.g. attachments) are stored in the data storage. While message deletion - its resources are only marking as deleted but aren't removing from disk storage physically, because of performance reason. Storage cleaner later removes such unnecessary resources.

Storage cleaner can be installed as a Windows service.

Processing obsolete files occurs in two stages:
a. certain number of legacy resources placed by chunks (configurable attribute "tasks_gen_chunks_count" in the "storage_cleaner" section) into a special "garbage" database table (tasks for storage cleaner working threads)
b. tasks from "garbage" database table selected by chunks (configurable attribute "tasks_chunck_size" in the "storage_cleaner" section) distribute to worker threads and become physically removed from the file storage

Configuration
"nlog" section configures logging. Detailed documentation can be found on the developer's site - http://nlog-project.org/
"storage_cleaner" section configures storage cleaner:
  "max_threads" attribute - number of worker threads that will physically remove obsolete resources;
  "tasks_chunck_size" attribute - the number of rows selected from "garbage" database table at one database query (reduces the number of queries to the database);
  "tasks_gen_chunks_count" attribute - number of jobs added to the "garbage" database table at one database query (reduces the number of queries to the database);
  "db_lock_name" attribute - localized name of the named database lock;
  "db_lock_timeout" attribute - time in seconds for named database lock;
  "watchdog_timeout" attribute - time in seconds after which the task from "garbage" database table can be processed again (protection against worker thread hang out).