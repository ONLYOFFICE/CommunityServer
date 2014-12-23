require 'grape-swagger'

module Posty
  class API < Grape::API
    prefix 'api'
    format :json
    rescue_from :all

    helpers APIHelper
    
    mount ::Posty::API_v1

    add_swagger_documentation :mount_path => "v1/swagger_doc", :api_version => "v1"
  end
end