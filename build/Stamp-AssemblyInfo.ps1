<#
.SYNOPSIS
    Writes the version GitVersion computed into Properties/AssemblyInfo.cs.

.DESCRIPTION
    This is a non SDK style project, so there is no <Version> property for MSBuild to pick up
    and the assembly attributes have to be edited directly.

    GitVersion's own /updateassemblyinfo is not used because the CLI restores the file when the
    process exits, which leaves nothing for the build step that follows to pick up.
#>
param(
    [Parameter(Mandatory = $true)][string] $AssemblySemVer,
    [Parameter(Mandatory = $true)][string] $AssemblySemFileVer,
    [Parameter(Mandatory = $true)][string] $InformationalVersion
)

$ErrorActionPreference = 'Stop'

$path = Join-Path $PSScriptRoot '..\Properties\AssemblyInfo.cs' | Resolve-Path
$content = Get-Content $path -Raw

# Anchored to the start of a line, so the commented out AssemblyVersion example just above the
# real attributes is left alone.
$content = $content -replace '(?m)^\[assembly: AssemblyVersion\("[^"]*"\)\]', "[assembly: AssemblyVersion(`"$AssemblySemVer`")]"
$content = $content -replace '(?m)^\[assembly: AssemblyFileVersion\("[^"]*"\)\]', "[assembly: AssemblyFileVersion(`"$AssemblySemFileVer`")]"

$informational = "[assembly: AssemblyInformationalVersion(`"$InformationalVersion`")]"
if ($content -match '(?m)^\[assembly: AssemblyInformationalVersion\("[^"]*"\)\]') {
    $content = $content -replace '(?m)^\[assembly: AssemblyInformationalVersion\("[^"]*"\)\]', $informational
} else {
    $content = $content.TrimEnd() + "`r`n" + $informational + "`r`n"
}

# Written this way rather than with Set-Content because -Encoding utf8 means UTF-8 with a BOM on
# Windows PowerShell and without one on PowerShell 7, and the file starts with a BOM already.
[System.IO.File]::WriteAllText($path, $content, (New-Object System.Text.UTF8Encoding($true)))

Write-Host "AssemblyVersion              $AssemblySemVer"
Write-Host "AssemblyFileVersion          $AssemblySemFileVer"
Write-Host "AssemblyInformationalVersion $InformationalVersion"
