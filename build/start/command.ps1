$PSversionMajor = $PSVersionTable.PSVersion | sort-object major | ForEach-Object {$_.major}
$PSversionMinor = $PSVersionTable.PSVersion | sort-object minor | ForEach-Object {$_.minor}

if ($PSversionMajor -lt 7 -or $PSversionMinor -lt 2) {
    Write-Error "Powershell version must be greater than or equal to 7.2."
    exit
}

$WorkDir = "$(Split-Path -Parent $PSScriptRoot)\run";
$CommandName = "$($args[0])";

if ( $CommandName -eq "increase-service-timeout" ) {	
	$RegistryPath = 'HKLM:\SYSTEM\CurrentControlSet\Control'
	$Name         = 'ServicesPipeTimeout'
	$Value        = '90000'

	Write-Output  "Changing service start timeout from default value (30000 ms ) to $($Value) ms"
	
	New-ItemProperty -Path $RegistryPath -Name $Name -Value $Value -PropertyType DWORD -Force


    Write-Warning  "Please, restart the computer for the changes to take effect."
    exit		
}



#Write-Output  "Starting $($CommandName) services at time: $(Get-Date -Format HH:mm:ss)"
#Write-Output  ""

Get-ChildItem -Path $WorkDir -File | ForEach-Object -ThrottleLimit 20 -Parallel  {
    $ServiceName = "Onlyoffice$([System.IO.Path]::GetFileNameWithoutExtension($_))";

    switch ( $Using:CommandName )
    {
        "start" { Start-Service -InputObject $(Get-Service -Name "$ServiceName") }
        "stop" { Stop-Service -InputObject $(Get-Service -Name "$ServiceName") }
        "restart" { Restart-Service -InputObject $(Get-Service -Name "$ServiceName") }
    }

    if( $? )
    {
        Write-Output "$ServiceName $($Using:CommandName) service has been done"
    }
}

#Write-Output  ""
#Write-Output  "End $($CommandName) services at time: $(Get-Date -Format HH:mm:ss)"