
Ftp Server implementation thoughts.

Root folder for ftp server
        Subfolder
                Subfolder
                
                _Config.xml


        _Config.xml
//--------------------------

_Config.xml specifies User access and permissions to specified folder.
It also defines virtual folders. Virtual folder is alias name to folder in some other location.

Root folder for ftp server         
         Subfolder

        _Config.xml
        (
        <permissionInheritace inherit=true>
        <permission userName="test" permissions="Read|Write ...">
        <virtualFolder name="vFolder" path="d:\ftpRoot2\">
        )              

For client is reported

        ftp://xxx/Subfolder
        ftp://xxx/vFolder     - there client is mapped to "d:\ftpRoot2\", _Config.xml is getted from there if exists, otherwise
                                parent foldr config.xml is used.


Permissions in _Config.xml must be inheritable from parent folder if _Config.xml in subFolder ins't available, permissions are automatically inhereted.

Permissions checking order:
        From ROOT to end calculate permission what will stay (inheriting, overriding, ...).
