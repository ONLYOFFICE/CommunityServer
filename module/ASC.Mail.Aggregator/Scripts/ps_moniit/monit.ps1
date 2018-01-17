<#
Information: 
    - Don't forget to setup once: Set-ExecutionPolicy RemoteSigned.
    - Don't forget to add config file(s) for service to monitor.
Example:

[service]
name=ASC.Mail.Aggregator

[memory]
threshold=50000000
cycles=3
action=restart

[cpu]
percent=90
cycles=3
action=restart

#>
#==============================================================================
# BEGIN USER-EDITABLE VARIABLES
#==============================================================================
# Log file time stamp:
$LogTime = Get-Date -format s;
# the log file directory
$LogPath = "D:\ASC\";
# the log file name
$LogName = "monit.log";
#==============================================================================
# END USER-EDITABLE VARIABLES
#==============================================================================
# import modules.
$myDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module -Name "$MyDir\modules\get-iniContent.psm1" -Verbose
Import-Module -Name "$MyDir\modules\out-iniFile.psm1" -Verbose

$LogFile = join-path $LogPath $LogName;

# create the log file if it doesn't already exist.
if (!(Test-Path $LogFile)) {
    New-Item -path $LogPath -type directory -force;
    New-Item $LogFile -type file;
}

# re-create the log file if it more then 100mb.
if((Get-Item $LogFile).length -gt 100mb)
{
    Remove-Item $LogPath -force;
    New-Item -path $LogPath -type directory -force;
    New-Item $LogFile -type file;
}

# load config files.
$configsDir = "$myDir\config\"
if ((Test-Path $configsDir)) {
    $configFiles = Get-ChildItem $configsDir -force
    if($configFiles -ne $null)
    {
        foreach($configFile in $configFiles)
        {
            $configPath = join-path $configsDir $configFile.Name
            $fileContent = Get-IniContent $configPath;
            if($fileContent -ne $null)
            {
                # $str = $fileContent | Out-String;
                # Write-Host $str -ForegroundColor red;
                if($fileContent["service"] -ne $null -And `
                   $fileContent["service"]["name"] -ne $null -And `
                   $fileContent["memory"] -ne $null -And `
                   $fileContent["memory"]["threshold"] -ne $null -And `
                   $fileContent["memory"]["cycles"] -ne $null -And `
                   $fileContent["memory"]["action"] -ne $null -And `
                   $fileContent["cpu"] -ne $null -And `
                   $fileContent["cpu"]["percent"] -ne $null -And `
                   $fileContent["cpu"]["cycles"] -ne $null -And `
                   $fileContent["cpu"]["action"] -ne $null)
                {
                    $Configs += @($fileContent);
                }
            }
        }
    }
}

if($Configs -eq $null)
{
    "$LogTime no any valid config in path {0}" -f $configsDir >> $LogFile;
    return;
}

# check services for thresholds.
foreach($Config in $Configs)
{
    "--------------------------------------------------------------" >> $LogFile;

    $name = $Config["service"]["name"];
    $mem_threshold = $Config["memory"]["threshold"];
    $mem_threshold_cycles = $Config["memory"]["cycles"];
    $mem_threshold_action = $Config["memory"]["action"];
    $cpu_threshold = $Config["cpu"]["percent"];
    $cpu_threshold_cycles = $Config["cpu"]["cycles"];
    $cpu_threshold_action = $Config["cpu"]["action"];

    @("$LogTime Service config:`r`n"
    "`tname={0}`r`n"
    "`tmem_threshold={1}`tmem_threshold_cycles={2}`tmem_threshold_action={3}`r`n"
    "`tcpu_threshold={4}`tcpu_threshold_cycles={5}`tcpu_threshold_action={6}") -f `
        $name, `
        $mem_threshold, `
        $mem_threshold_cycles, `
        $mem_threshold_action, `
        $cpu_threshold, `
        $cpu_threshold_cycles, `
        $cpu_threshold_action >> $LogFile;

    $Service = Get-Service -name "$name";

    if($Service -ne $null)
    {
        $ServiceName = $Service.Name;
        "$LogTime Service '{0}' Status '{1}'" -f $ServiceName, $Service.Status >> $LogFile
        
        if($Service.Status -eq "Stopped")
        {
            "$LogTime Start service '{0}'." -f $ServiceName >> $LogFile;
            Start-Service $ServiceName;
            "$LogTime Service '{0}' is started." -f $ServiceName >> $LogFile;
        }
        
        $id = gwmi Win32_Service -Filter "Name LIKE '$name'" | select -expand ProcessId
        
        "$LogTime Process Id = {0}" -f $id >> $LogFile;
        
        $Process = Get-Process -Id $id;

        if($Process -ne $null)
        {
            $wsm = $Process.WS /1024 /1024;
            $th = $mem_threshold /1024 /1024;
            
            # Format the numbers so they are easier to read in mb units and round up to a
            # decimal places.
            
            $Fwsm = $("{0:0.000}" -f $wsm);
            $Fth = $("{0:0.000}" -f $th);

            "$LogTime Process WorkingSet is {0} mb and threshold is {1} mb" -f $Fwsm, $Fth >> $LogFile; 

            $serviceCheckFile = join-path "$myDir\checks\" "$name.ini";
            
            if($Process.WS -gt $mem_threshold)
            {
                "$LogTime Process exceeded memory threshold." >> $LogFile;
                if ((Test-Path $serviceCheckFile)) {
                   #"$LogTime mem_cycle exists on '{0}'." -f $serviceCheckFile >> $LogFile;
                   $serviceCheck = Get-IniContent $serviceCheckFile;
                }
                
                if($serviceCheck -eq $null)
                {
                    #"$LogTime mem_cycle not exists on '{0}'." -f $serviceCheckFile >> $LogFile;                  
                    $memory = @{"last_cycle" = 0};
                    $serviceCheck = @{"memory" = $memory};
                }

                # "$LogTime mem_cycle old = {0}" -f $serviceCheck["memory"]["last_cycle"] >> $LogFile;
                
                ([int]$serviceCheck["memory"]["last_cycle"])++;
                
                # "$LogTime mem_cycle new = {0}" -f $serviceCheck["memory"]["last_cycle"] >> $LogFile;
                
                "$LogTime Process current memory cycle = {0} and limit memory threshold cycle = {1}." `
                    -f $serviceCheck["memory"]["last_cycle"], `
                    $mem_threshold_cycles >> $LogFile;
                
                if($serviceCheck["memory"]["last_cycle"] -eq $mem_threshold_cycles)
                {
                    "$LogTime Process memory threshold cycle limit exceeded." >> $LogFile;
                    
                    if($mem_threshold_action -eq "restart")
                    {
                        "$LogTime Service '{0}' restart." -f $ServiceName >> $LogFile;
                        Restart-Service $ServiceName; 
                        "$LogTime Service '{0}' is restarted." -f $ServiceName >> $LogFile;
                        
                        # remove stored check if exists
                        if ((Test-Path $serviceCheckFile)) {
                            Remove-Item $serviceCheckFile -force;
                        }
                    }
                    else
                    {
                        "$LogTime Unknown action on memory threshold = {0}." -f $mem_threshold_action >> $LogFile;
                    }
                }
                else
                {
                    "$LogTime Save mem_cycle to '{0}'." -f $serviceCheckFile >> $LogFile;
                    Out-IniFile -InputObject $serviceCheck -FilePath $serviceCheckFile -force;
                }
            }
            else
            {
                $delta = $("{0:0.000}" -f ($th - $wsm));
                "$LogTime Service '{0}' still have {1} mb left." -f $ServiceName, $delta >> $LogFile;

                # remove stored check if exists
                if ((Test-Path $serviceCheckFile)) {
                   Remove-Item $serviceCheckFile -force;
                }
            }
        }
        else
        {
            "$LogTime Process by ServiceName {0} could not be found." -f $ServiceName >> $LogFile;
        }
    }
    else
    {
        "$LogTime Could not find any services matching to name {0}" -f $name >> $LogFile;
    }
    
    "--------------------------------------------------------------" >> $LogFile;
}


"==========================================================================" >> $LogFile;