class GlobalVars < ActiveRecord::Base
  self.table_name = "global_vars"
  self.primary_key = "variable"
  
  validates :variable, :uniqueness => true
  validates :variable, :presence => true
end