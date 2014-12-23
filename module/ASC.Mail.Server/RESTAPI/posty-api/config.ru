#\ -p 8081 -E production
require File.expand_path('../config/environment', __FILE__)

use Rack::Cors do
  allow do
    origins '*'
    resource '*', :headers => :any, :methods => [:put, :delete, :get, :post, :options]
  end
end

run Posty::API