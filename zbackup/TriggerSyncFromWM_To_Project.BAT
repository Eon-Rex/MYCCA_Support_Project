ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\AcxiomDesign" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\AcxiomDesign" /e /mir /np /tee /log:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\App_Code" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\App_Code" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\App_Start" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\App_Start" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\BussinessLayer" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\BussinessLayer" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\Controllers" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\Controllers" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\DataAccessLayer" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\DataAccessLayer" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\EmailTemplates" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\EmailTemplates" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\ExcelTemplate" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\ExcelTemplate" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\Models" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\Models" /e /mir /np /tee /log+:backup_log.txt
ROBOCOPY "C:\Web & Mobile\Latest Code\CCAF\CCAF Web\CCAF\Views" "C:\Projects\CCAF\Portal-UAT\CCAF Web\CCAF\Views" /e /mir /np /tee /log+:backup_log.txt
pause