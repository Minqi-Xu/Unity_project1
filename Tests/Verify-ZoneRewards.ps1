param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.4.5f1/Editor')
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Verify-EnemySystem.ps1') -UnityEditor $UnityEditor
$repo = Split-Path $PSScriptRoot -Parent
$output = Join-Path $repo 'Unity6-project1/Temp/ZoneChecks'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$compiler = Join-Path $UnityEditor 'Data/DotNetSdkRoslyn/csc.dll'
$rsp = Join-Path $output 'checks.rsp'
$references = Get-ChildItem (Join-Path $UnityEditor 'Data/NetStandard/ref/2.1.0') -Filter *.dll | ForEach-Object { '-r:"' + $_.FullName + '"' }
@('-nologo'; '-target:exe'; '-nostdlib+'; '-langversion:9'; ('-out:"' + $output + '/ZoneStayChecks.dll"'); $references;
    ('"' + $repo + '/Unity6-project1/Assets/Scripts/GameScene/ZoneStayProgress.cs"');
    ('"' + $repo + '/Unity6-project1/Assets/Scripts/GameScene/ZoneTravelDistance.cs"');
    ('"' + $repo + '/Unity6-project1/Assets/Scripts/GameScene/ZoneNavigationGeometry.cs"');
    ('"' + $PSScriptRoot + '/ZoneStayChecks.cs"')) | Set-Content $rsp
& dotnet $compiler ('@' + $rsp)
if ($LASTEXITCODE -ne 0) { throw 'Zone check compilation failed' }
'{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' | Set-Content (Join-Path $output 'ZoneStayChecks.runtimeconfig.json')
& dotnet (Join-Path $output 'ZoneStayChecks.dll')
if ($LASTEXITCODE -ne 0) { throw 'Zone checks failed' }
