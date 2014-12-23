DELETE FROM `mailbox` WHERE  
`username` LIKE '%safronov8.com' OR 
`username` LIKE '%katkov.com' OR 
`username` LIKE '%novozhenin8.com' OR 
`username` LIKE '%yudanov8.com' OR 
`username` LIKE '%s-tlmail.com';

DELETE FROM `domain` WHERE  
`domain` LIKE '%safronov8%' OR 
`domain` LIKE '%katkov%' OR 
`domain` LIKE '%novozhenin8%' OR 
`domain` LIKE '%yudanov8%' OR
`domain` LIKE '%s-tlmail%';

DELETE FROM `alias` WHERE  
`address` LIKE '%@safronov8%' OR 
`address` LIKE '%@katkov%' OR 
`address` LIKE '%@novozhenin8%' OR 
`address` LIKE '%@yudanov8%' OR 
`address` LIKE '%@s-tlmail%';