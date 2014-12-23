$LOAD_PATH.unshift(File.join(File.dirname(__FILE__), '..', 'app', 'api'))
$LOAD_PATH.unshift(File.join(File.dirname(__FILE__), '..', 'app', 'models'))
$LOAD_PATH.unshift(File.dirname(__FILE__))

require 'boot'
require 'uri'
require 'erb'

Bundler.require :default, ENV['RACK_ENV']

dbconfig = YAML.load(ERB.new(File.read(Dir.pwd + '/config/database.yml')).result)
ActiveRecord::Base.establish_connection(dbconfig[ENV['RACK_ENV']])

I18n.config.enforce_available_locales = false

Dir[File.expand_path('../../app/api/v*.rb', __FILE__)].each do |f|
  require f
end

Dir[File.expand_path('../../app/models/*.rb', __FILE__)].each do |f|
  require f
end

require 'helper'
require 'api'