module Posty
  class API_v1 < Grape::API
    version 'v1', :using => :path, :vendor => 'posty'
    before { authenticate! }
    
    resource :api_keys do
      desc "Returns all available API Keys"
      get do
        ApiKey.all
      end
      
      desc "Creates a new API Key"
      post do
        ApiKey.create(attributes_for_keys [ :expires_at ])
      end
      
      segment '/:api_key' do
        desc "Returns the information to the specified api_key"
        get do
          current_api_key
        end
        
        desc "Update the specified api_key"
        put do
          return_on_success(current_api_key) do |api_key|
            api_key.update_attributes(attributes_for_keys [ :active, :expires_at ])
          end
        end
      
        desc "Delete the given token"
        delete do
          current_api_key.destroy
        end
      end
    end
    
    resource :spam do  
      resource :training do
        desc "Creates a new training task"
        post do
          SpamTrainingQueue.create(attributes_for_keys [ :url, :is_spam ])
        end
      end
    end

    resource :version do
      desc "Returns current mailserver version"
      get do
        GlobalVars.find("VERSION", :select => "value")
      end
    end
    
    resource :domains do

      desc "Returns all available domains"
      get do
        Domain.all
      end
      
      segment '/:domain_name', requirements: {domain_name: /[a-z0-9\-]{2,}\.[a-z0-9]{2,}/} do 
        desc "Returns the information to the specified domain"
        get do
            current_domain
        end
      
        desc "Delete the specified domain"
        delete do
          current_domain.clear_folder
        end
        
        resource :mailboxes do
          desc "Returns all available mailboxes"
          get do
            current_domain.mailbox.select("username, name, local_part, storagebasedirectory, storagenode, maildir, quota, domain, created, modified, expired, active")
          end
          
          segment '/:mailbox_name', requirements: {mailbox_name: /[a-z0-9\-\.]+/} do 
            desc "Returns the information to the specified mailbox"
            get do
              current_mailbox
            end
            
            desc "Delete the specified mailbox's maildir folder"
            delete do
              current_mailbox.clear_folder
            end
          end
        end
      end
    end
  end
end