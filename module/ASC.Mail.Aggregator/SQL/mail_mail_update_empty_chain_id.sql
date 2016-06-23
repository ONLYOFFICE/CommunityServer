Update `mail_mail` AS mm SET mm.mime_message_id = mm.id, mm.chain_id = mm.id
WHERE mm.chain_id is null;