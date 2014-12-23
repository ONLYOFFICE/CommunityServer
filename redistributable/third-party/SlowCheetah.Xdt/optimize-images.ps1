[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    $folderToOptimize
)

<#
.SYNOPSIS
    If nuget is in the tools
    folder then it will be downloaded there.
#>
function GetNuget(){
    [cmdletbinding()]
    param(
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\AzureJobs\tools\"),

        $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
    )
    process{
        $nugetDestPath = Join-Path -Path $toolsDir -ChildPath nuget.exe
        
        if(!(Test-Path $nugetDestPath)){
            'Downloading nuget.exe' | Write-Verbose
            # download nuget
            $webclient = New-Object System.Net.WebClient
            $webclient.DownloadFile($nugetDownloadUrl, $nugetDestPath)

            # double check that is was written to disk
            if(!(Test-Path $nugetDestPath)){
                throw 'unable to download nuget'
            }
        }

        # return the path of the file
        $nugetDestPath
    }
}

<#
.SYNOPSIS
    If the image optimizer in the .ools
    folder then it will be downloaded there.
#>
function GetImageOptimizer(){
    [cmdletbinding()]
    param(
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\AzureJobs\tools\")
    )
    process{
        
        if(!(Test-Path $toolsDir)){
            New-Item $toolsDir -ItemType Directory | Out-Null
        }

        $imgOptimizer = (Get-ChildItem -Path $toolsDir -Include 'ImageCompressor.Job.exe' -Recurse)

        if(!$imgOptimizer){
            'Downloading image optimizer to the .tools folder' | Write-Verbose
            # nuget install AzureImageOptimizer -Prerelease -OutputDirectory C:\temp\nuget\out\
            $cmdArgs = @('install','AzureImageOptimizer','-Prerelease','-OutputDirectory',(Resolve-Path $toolsDir).ToString())

            'Calling nuget to install image optimzer with the following args. [{0}]' -f ($cmdArgs -join ' ') | Write-Verbose
            &(GetNuget) $cmdArgs | Out-Null
        }

        $imgOptimizer = (Get-ChildItem -Path $toolsDir -Include 'ImageCompressor.Job.exe' -Recurse)
        if(!$imgOptimizer){ throw 'Image optimizer not found' }       

        # return the path to it
        if($imgOptimizer -is [Array]){
            # incase somehow more than one ImageCompressor.Job.exe shows up in that folder
            $imgOptimizer[0]
        }
        else{
            $imgOptimizer
        }
    }
}

function OptimizeImages(){
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,Position=0)]
        $folder
    )
    process{        
        [string]$imgOptExe = GetImageOptimizer

        [string]$folderToOptimize = (Resolve-path $folder)

        'Starting image optimizer on folder [{0}]' -f $folder | Write-Host
        # .\.tools\AzureImageOptimizer.0.0.10-beta\tools\ImageCompressor.Job.exe --folder M:\temp\images\opt\to-optimize
        $cmdArgs = @('--folder', $folderToOptimize)

        'Calling img optimizer with the following args [{0} {1}]' -f $imgOptExe, ($cmdArgs -join ' ') | Write-Host
        &$imgOptExe $cmdArgs

        'Images optimized' | Write-Host
    }
}

OptimizeImages -folder $folderToOptimize
