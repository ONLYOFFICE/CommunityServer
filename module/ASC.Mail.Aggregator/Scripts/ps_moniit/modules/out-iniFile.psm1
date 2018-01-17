Function Out-IniFile {  
    <#  
    .Synopsis  
        Write hash content to INI file  
          
    .Description  
        Write hash content to INI file  
          
    .Notes  
        Author        : Oliver Lipkau <oliver@lipkau.net>  
        Blog        : http://oliver.lipkau.net/blog/  
        Source        : https://github.com/lipkau/PsIni 
                      http://gallery.technet.microsoft.com/scriptcenter/ea40c1ef-c856-434b-b8fb-ebd7a76e8d91 
        Version        : 1.0 - 2010/03/12 - Initial release  
                      1.1 - 2012/04/19 - Bugfix/Added example to help (Thx Ingmar Verheij)  
                      1.2 - 2014/12/11 - Improved handling for missing output file (Thx SLDR) 
          
        #Requires -Version 2.0  
          
    .Inputs  
        System.String  
        System.Collections.Hashtable  
          
    .Outputs  
        System.IO.FileSystemInfo  
          
    .Parameter Append  
        Adds the output to the end of an existing file, instead of replacing the file contents.  
          
    .Parameter InputObject  
        Specifies the Hashtable to be written to the file. Enter a variable that contains the objects or type a command or expression that gets the objects.  
  
    .Parameter FilePath  
        Specifies the path to the output file.  
       
     .Parameter Encoding  
        Specifies the type of character encoding used in the file. Valid values are "Unicode", "UTF7",  
         "UTF8", "UTF32", "ASCII", "BigEndianUnicode", "Default", and "OEM". "Unicode" is the default.  
          
        "Default" uses the encoding of the system's current ANSI code page.   
          
        "OEM" uses the current original equipment manufacturer code page identifier for the operating   
        system.  
       
     .Parameter Force  
        Allows the cmdlet to overwrite an existing read-only file. Even using the Force parameter, the cmdlet cannot override security restrictions.  
          
     .Parameter PassThru  
        Passes an object representing the location to the pipeline. By default, this cmdlet does not generate any output.  
                  
    .Example  
        Out-IniFile $IniVar "C:\myinifile.ini"  
        -----------  
        Description  
        Saves the content of the $IniVar Hashtable to the INI File c:\myinifile.ini  
          
    .Example  
        $IniVar | Out-IniFile "C:\myinifile.ini" -Force  
        -----------  
        Description  
        Saves the content of the $IniVar Hashtable to the INI File c:\myinifile.ini and overwrites the file if it is already present  
          
    .Example  
        $file = Out-IniFile $IniVar "C:\myinifile.ini" -PassThru  
        -----------  
        Description  
        Saves the content of the $IniVar Hashtable to the INI File c:\myinifile.ini and saves the file into $file  
  
    .Example  
        $Category1 = @{“Key1”=”Value1”;”Key2”=”Value2”}  
    $Category2 = @{“Key1”=”Value1”;”Key2”=”Value2”}  
    $NewINIContent = @{“Category1”=$Category1;”Category2”=$Category2}  
    Out-IniFile -InputObject $NewINIContent -FilePath "C:\MyNewFile.INI"  
        -----------  
        Description  
        Creating a custom Hashtable and saving it to C:\MyNewFile.INI  
    .Link  
        Get-IniContent  
    #>  
      
    [CmdletBinding()]  
    Param(  
        [switch]$Append,  
          
        [ValidateSet("Unicode","UTF7","UTF8","UTF32","ASCII","BigEndianUnicode","Default","OEM")]  
        [Parameter()]  
        [string]$Encoding = "Unicode",  
 
          
        [ValidateNotNullOrEmpty()]  
        [ValidatePattern('^([a-zA-Z]\:)?.+\.ini$')]  
        [Parameter(Mandatory=$True)]  
        [string]$FilePath,  
          
        [switch]$Force,  
          
        [ValidateNotNullOrEmpty()]  
        [Parameter(ValueFromPipeline=$True,Mandatory=$True)]  
        [Hashtable]$InputObject,  
          
        [switch]$Passthru  
    )  
      
    Begin  
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function started"}  
          
    Process  
    {  
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Writing to file: $Filepath"  
          
        if ($append) {$outfile = Get-Item $FilePath}  
        else {$outFile = New-Item -ItemType file -Path $Filepath -Force:$Force}  
        if (!($outFile)) {Throw "Could not create File"}  
        foreach ($i in $InputObject.keys)  
        {  
            if (!($($InputObject[$i].GetType().Name) -eq "Hashtable"))  
            {  
                #No Sections  
                Write-Verbose "$($MyInvocation.MyCommand.Name):: Writing key: $i"  
                Add-Content -Path $outFile -Value "$i=$($InputObject[$i])" -Encoding $Encoding  
            } else {  
                #Sections  
                Write-Verbose "$($MyInvocation.MyCommand.Name):: Writing Section: [$i]"  
                Add-Content -Path $outFile -Value "[$i]" -Encoding $Encoding  
                Foreach ($j in $($InputObject[$i].keys | Sort-Object))  
                {  
                    if ($j -match "^Comment[\d]+") {  
                        Write-Verbose "$($MyInvocation.MyCommand.Name):: Writing comment: $j"  
                        Add-Content -Path $outFile -Value "$($InputObject[$i][$j])" -Encoding $Encoding  
                    } else {  
                        Write-Verbose "$($MyInvocation.MyCommand.Name):: Writing key: $j"  
                        Add-Content -Path $outFile -Value "$j=$($InputObject[$i][$j])" -Encoding $Encoding  
                    }  
                      
                }  
                Add-Content -Path $outFile -Value "" -Encoding $Encoding  
            }  
        }  
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Finished Writing to file: $path"  
        if ($PassThru) {Return $outFile}  
    }  
          
    End  
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function ended"}  
} 