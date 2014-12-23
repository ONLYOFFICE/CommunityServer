task :environment do
  require File.expand_path('../config/environment', __FILE__)
end

namespace :api_key do
  desc "Creates a new api key"
  task :generate => :environment do
    ApiKey.create(:expires_at => Time.now + 100.years)
  end
end

namespace :db do
  desc "Migrate the database through scripts in db/migrate"
  task(:migrate => :environment) do
    ActiveRecord::Migrator.migrate('db/migrate', ENV["VERSION"] ? ENV["VERSION"].to_i : nil )
    Rake::Task["db:schema:dump"].invoke if ActiveRecord::Base.schema_format == :ruby
  end

  namespace :migrate do
    desc 'Runs the "down" for a given migration VERSION'
    task(:down => :environment) do
      ActiveRecord::Migrator.down('db/migrate', ENV["VERSION"] ? ENV["VERSION"].to_i : nil )
      Rake::Task["db:schema:dump"].invoke if ActiveRecord::Base.schema_format == :ruby
    end

    desc 'Runs the "up" for a given migration VERSION'
    task(:up => :environment) do
      ActiveRecord::Migrator.up('db/migrate', ENV["VERSION"] ? ENV["VERSION"].to_i : nil )
      Rake::Task["db:schema:dump"].invoke if ActiveRecord::Base.schema_format == :ruby
    end
    
    desc "Rollbacks the database one migration and re migrate up"
    task(:redo => :environment) do
      ActiveRecord::Migrator.rollback('db/migrate', 1 )
      ActiveRecord::Migrator.up('db/migrate', nil )
      Rake::Task["db:schema:dump"].invoke if ActiveRecord::Base.schema_format == :ruby
    end
  end
  
  namespace :schema do
    task :dump => :environment do
      require 'active_record/schema_dumper'
      File.open(ENV['SCHEMA'] || "db/schema.rb", "w") do |file|
        ActiveRecord::SchemaDumper.dump(ActiveRecord::Base.connection, file)
      end
    end
  end
end