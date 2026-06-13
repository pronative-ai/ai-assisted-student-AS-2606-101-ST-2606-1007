# Install pre-commit hooks
$hooksDir = Join-Path (Get-Location) ".git\hooks"
$sourceDir = Join-Path (Get-Location) "hooks"

Get-ChildItem -Path $sourceDir | ForEach-Object {
  $dest = Join-Path $hooksDir $_.Name
  Copy-Item -Path $_.FullName -Destination $dest -Force
  Write-Output "Installed hook: $($_.Name)"
}
