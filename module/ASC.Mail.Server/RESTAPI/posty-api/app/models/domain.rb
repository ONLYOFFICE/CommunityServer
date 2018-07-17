class Domain < ActiveRecord::Base
  self.table_name = "domain"
  self.primary_key = "domain"
 
  has_many :mailbox, primary_key: :domain, foreign_key: :domain
  
  validates :domain, :uniqueness => true
  validates :domain, :presence => true
  validates :domain, :format => { :with => /^[a-z0-9\-]{2,}\.[a-z0-9]{2,}$/, :message => "Please use a valid domain name" }
 
  def get_folder
    "/var/vmail/vmail1/" + self.domain
  end

  def clear_folder
    FileUtils.rm_rf get_folder
  end
  
end