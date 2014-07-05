DROP TABLE auth_tokens;
CREATE TABLE auth_tokens (
						token VARCHAR(255) NOT NULL PRIMARY KEY,
						token_secret VARCHAR(255),
						associate_id VARCHAR(255),
						request_token BOOLEAN
					);
		