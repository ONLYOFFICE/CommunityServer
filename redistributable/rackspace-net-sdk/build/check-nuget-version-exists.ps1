param (
  [string]$version
)

try {
  $url = "http://nuget.org/packages/Rackspace/$version"
  $status = (Invoke-WebRequest $url).StatusCode
} catch {
  $status = $_.Exception.Response.StatusCode.Value__
}

$shouldPublishToNuGet = $status -eq '404'
Write-Host $shouldPublishToNuGet
