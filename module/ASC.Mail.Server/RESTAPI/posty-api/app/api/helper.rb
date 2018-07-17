module Posty
  module APIHelper
    def attributes_for_keys(keys)
      attrs = {}
      keys.each do |key|
        attrs[key] = params[key] if params[key].present?
      end
      attrs
    end

    def ensure_entity(message='', &block)
      entity = block.call
      
      unless entity
        error!("unknown #{message}", 404)
      end
      
      entity
    end
    
    def return_on_success(entity, &block)
      success = block.call(entity)
      
      success ? entity : validation_error(entity.errors)
    end
    
    def authenticate!
      error!('Unauthorized. Invalid or expired token.', 401) unless current_session
    end
 
    def current_session
      auth_token = params[:auth_token] || env['HTTP_AUTH_TOKEN']
      @current_session ||= ApiKey.active.where(access_token: auth_token).first
    end
    
    def current_api_key
      ensure_entity('ApiKey') do
        ApiKey.find_by_access_token(params[:api_key])
      end
    end
        
    def current_domain
      @current_domain ||= ensure_entity('Domain') do
        begin
            Domain.find(params[:domain_name])
        rescue ActiveRecord::RecordNotFound
        end
      end
    end
    
    def current_mailbox
      ensure_entity('Mailbox') do
        begin
          current_domain.mailbox.find(complete_email_address(params[:mailbox_name], params[:domain_name]), :select => "username, name, local_part, storagebasedirectory, storagenode, maildir, quota, domain, created, modified, expired, active")
        rescue ActiveRecord::RecordNotFound
        end  
      end
    end

    def complete_email_address(user, domain)
      user + "@" + domain
    end
    
    def validation_error(errors)
      error!(errors, 400)
    end
    
    def current_domain_id_hash
      {"domain_id" => current_domain.domain}
    end
    
    def current_mailbox_id_hash
      {"mailbox_id" => current_user.username}
    end
    
  end
end