class CreateApiKeys < ActiveRecord::Migration
  def self.up
    create_table :api_keys do |t|
      t.string :access_token,      null: false
      t.boolean :active,           null: false, default: true
      t.datetime :expires_at
      t.timestamps
    end
  end

  def self.down
    drop_table :api_keys
  end
end
