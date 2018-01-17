class SpamTrainingQueue < ActiveRecord::Base
  self.table_name = "api_sa_training_queue"
  
  attr_accessible :url, :is_spam 
  
  validates :url, :presence => true
  validates :url, format: { with: URI.regexp }, if: Proc.new { |a| a.url.present? }
  
  validates_inclusion_of :is_spam, :in => [ true, false ]
end