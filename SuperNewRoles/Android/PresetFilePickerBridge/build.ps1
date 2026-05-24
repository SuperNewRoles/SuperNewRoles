[CmdletBinding()]
param(
    [string]$AndroidSdk = "",
    [string]$JavaHome = "",
    [string]$AndroidApi = "",
    [string]$BuildToolsVersion = "",
    [switch]$KeepBuildDir
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-FullPath([string]$Path) {
    return [System.IO.Path]::GetFullPath($Path)
}

function Get-VersionFromName([string]$Name) {
    $match = [regex]::Match($Name, "\d+(\.\d+)*")
    if (-not $match.Success) {
        return [version]"0.0"
    }

    try {
        return [version]$match.Value
    }
    catch {
        return [version]"0.0"
    }
}

function Find-AndroidSdk([string]$Override) {
    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($Override)) {
        $candidates += $Override
    }
    if (-not [string]::IsNullOrWhiteSpace($env:ANDROID_HOME)) {
        $candidates += $env:ANDROID_HOME
    }
    if (-not [string]::IsNullOrWhiteSpace($env:ANDROID_SDK_ROOT)) {
        $candidates += $env:ANDROID_SDK_ROOT
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return Resolve-FullPath $candidate
        }
    }

    throw "Android SDK was not found. Set ANDROID_HOME or pass -AndroidSdk."
}

function Find-AndroidJar([string]$SdkPath, [string]$Api) {
    $platformsDir = Join-Path $SdkPath "platforms"
    if (-not (Test-Path -LiteralPath $platformsDir)) {
        throw "Android SDK platforms directory was not found: $platformsDir"
    }

    if ([string]::IsNullOrWhiteSpace($Api)) {
        $preferredAndroidJar = Join-Path $platformsDir "android-35\android.jar"
        if (Test-Path -LiteralPath $preferredAndroidJar) {
            return Resolve-FullPath $preferredAndroidJar
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Api)) {
        $platformName = if ($Api.StartsWith("android-")) { $Api } else { "android-$Api" }
        $androidJar = Join-Path $platformsDir "$platformName\android.jar"
        if (Test-Path -LiteralPath $androidJar) {
            return Resolve-FullPath $androidJar
        }
        throw "android.jar was not found for $platformName."
    }

    $platform = Get-ChildItem -LiteralPath $platformsDir -Directory -Filter "android-*" |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName "android.jar") } |
        Sort-Object @{ Expression = { Get-VersionFromName $_.Name }; Descending = $true } |
        Select-Object -First 1

    if ($null -eq $platform) {
        throw "No Android platform with android.jar was found under $platformsDir."
    }

    return Resolve-FullPath (Join-Path $platform.FullName "android.jar")
}

function Find-D8([string]$SdkPath, [string]$Version) {
    $buildToolsDir = Join-Path $SdkPath "build-tools"
    if (-not (Test-Path -LiteralPath $buildToolsDir)) {
        throw "Android SDK build-tools directory was not found: $buildToolsDir"
    }

    if ([string]::IsNullOrWhiteSpace($Version)) {
        $preferredD8 = Join-Path $buildToolsDir "35.0.1\d8.bat"
        if (Test-Path -LiteralPath $preferredD8) {
            return Resolve-FullPath $preferredD8
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $d8 = Join-Path $buildToolsDir "$Version\d8.bat"
        if (Test-Path -LiteralPath $d8) {
            return Resolve-FullPath $d8
        }
        throw "d8.bat was not found for build-tools $Version."
    }

    $toolDir = Get-ChildItem -LiteralPath $buildToolsDir -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName "d8.bat") } |
        Sort-Object @{ Expression = { Get-VersionFromName $_.Name }; Descending = $true } |
        Select-Object -First 1

    if ($null -eq $toolDir) {
        throw "d8.bat was not found under $buildToolsDir."
    }

    return Resolve-FullPath (Join-Path $toolDir.FullName "d8.bat")
}

function Find-JavaTool([string]$ToolName, [string]$OverrideJavaHome) {
    $homes = @()
    if (-not [string]::IsNullOrWhiteSpace($OverrideJavaHome)) {
        $homes += $OverrideJavaHome
    }
    if (-not [string]::IsNullOrWhiteSpace($env:JAVA_HOME)) {
        $homes += $env:JAVA_HOME
    }

    foreach ($home in $homes) {
        $tool = Join-Path $home "bin\$ToolName.exe"
        if (Test-Path -LiteralPath $tool) {
            return Resolve-FullPath $tool
        }
    }

    $command = Get-Command "$ToolName.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return Resolve-FullPath $command.Source
    }

    $fallback = Join-Path ${env:ProgramFiles} "Java\jdk-17\bin\$ToolName.exe"
    if (Test-Path -LiteralPath $fallback) {
        return Resolve-FullPath $fallback
    }

    throw "$ToolName.exe was not found. Set JAVA_HOME or pass -JavaHome."
}

function Get-JavacCompatibilityArgs([string]$JavacPath) {
    $versionText = (& $JavacPath -version 2>&1 | Out-String).Trim()
    $match = [regex]::Match($versionText, "javac\s+(\d+)(?:\.(\d+))?")
    if ($match.Success) {
        $major = [int]$match.Groups[1].Value
        if ($major -eq 1 -and $match.Groups[2].Success) {
            $major = [int]$match.Groups[2].Value
        }

        if ($major -ge 9) {
            return @("--release", "8")
        }
    }

    return @("-source", "1.8", "-target", "1.8")
}

function Invoke-Tool([string]$ToolPath, [string[]]$Arguments) {
    & $ToolPath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$ToolPath failed with exit code $LASTEXITCODE."
    }
}

$scriptDir = Resolve-FullPath $PSScriptRoot
$repoRoot = Resolve-FullPath (Join-Path $scriptDir "..\..\..")
$outputJar = Join-Path $repoRoot "SuperNewRoles\Resources\AndroidPresetFilePickerBridge.jar"
$buildDir = Resolve-FullPath (Join-Path $scriptDir "build")
$classesDir = Join-Path $buildDir "classes"
$dexDir = Join-Path $buildDir "dex"

if (-not $buildDir.StartsWith($scriptDir, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean a build directory outside the bridge directory: $buildDir"
}

$sdkPath = Find-AndroidSdk $AndroidSdk
$androidJar = Find-AndroidJar $sdkPath $AndroidApi
$d8 = Find-D8 $sdkPath $BuildToolsVersion
$javac = Find-JavaTool "javac" $JavaHome
$jar = Find-JavaTool "jar" $JavaHome

Write-Host "Android SDK : $sdkPath"
Write-Host "android.jar : $androidJar"
Write-Host "d8          : $d8"
Write-Host "javac       : $javac"
Write-Host "jar         : $jar"
Write-Host "Output jar  : $outputJar"

Remove-Item -LiteralPath $buildDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $classesDir, $dexDir | Out-Null

$sourceFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $scriptDir "src") -Recurse -Filter "*.java"
    Get-ChildItem -LiteralPath (Join-Path $scriptDir "stubs") -Recurse -Filter "*.java"
) | ForEach-Object { $_.FullName }

if ($sourceFiles.Count -eq 0) {
    throw "No Java source files were found."
}

$javacArgs = @("-encoding", "UTF-8", "-Xlint:-options") + (Get-JavacCompatibilityArgs $javac) + @("-classpath", $androidJar, "-d", $classesDir) + $sourceFiles
Invoke-Tool $javac $javacArgs

$unityStubClassDir = Join-Path $classesDir "com\unity3d"
Remove-Item -LiteralPath $unityStubClassDir -Recurse -Force -ErrorAction SilentlyContinue

$classFiles = @(Get-ChildItem -LiteralPath $classesDir -Recurse -Filter "*.class" | ForEach-Object { $_.FullName })
if ($classFiles.Count -eq 0) {
    throw "No compiled bridge .class files were found."
}

$d8Args = @("--classpath", $androidJar, "--output", $dexDir) + $classFiles
Invoke-Tool $d8 $d8Args

$classesDex = Join-Path $dexDir "classes.dex"
if (-not (Test-Path -LiteralPath $classesDex)) {
    throw "d8 did not produce classes.dex."
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputJar) | Out-Null
Push-Location $dexDir
try {
    Invoke-Tool $jar @("cf", $outputJar, "classes.dex")
}
finally {
    Pop-Location
}

$jarEntries = & $jar tf $outputJar
if ($LASTEXITCODE -ne 0) {
    throw "Failed to inspect generated jar."
}
if ($jarEntries -notcontains "classes.dex") {
    throw "Generated jar does not contain classes.dex."
}

if (-not $KeepBuildDir) {
    Remove-Item -LiteralPath $buildDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Built Android preset bridge jar successfully."
