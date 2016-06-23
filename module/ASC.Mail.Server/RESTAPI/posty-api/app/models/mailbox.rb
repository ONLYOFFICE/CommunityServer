class Mailbox < ActiveRecord::Base
  self.table_name = "mailbox"
  self.primary_key = "username"
  
  belongs_to :domain, primary_key: :domain, foreign_key: :domain
  
  validates :username, :format => { :with => /^[a-z0-9\-\.]+$/, :message => "Please use a valid user name" }
  validates :username, :presence => true
  validates :domain, :presence => true 

  def get_folder
    self.storagebasedirectory + "/" + self.storagenode + "/" + self.maildir
  end

  def clear_folder
    FileUtils.rm_rf get_folder
  end
  
end