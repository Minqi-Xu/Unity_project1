param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.4.5f1/Editor')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repo 'Unity6-project1'
$output = Join-Path $project 'Temp/EnemyChecks'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$compiler = Join-Path $UnityEditor 'Data/DotNetSdkRoslyn/csc.dll'
$netReferences = @(Get-ChildItem (Join-Path $UnityEditor 'Data/NetStandard/ref/2.1.0') -Filter *.dll)
$netReferences += @(Get-ChildItem (Join-Path $UnityEditor 'Data/NetStandard/compat/2.1.0/shims/netfx') -Filter *.dll)
$unityReferences = @(Get-ChildItem (Join-Path $UnityEditor 'Data/Managed/UnityEngine') -Filter *.dll)
$uiReferences = @('Unity.TextMeshPro.dll', 'UnityEngine.UI.dll') | ForEach-Object {
    $dependency = Join-Path $project ('Library/ScriptAssemblies/' + $_)
    if (-not (Test-Path -LiteralPath $dependency)) { throw "Missing $dependency. Open this project in Unity once to import packages." }
    Get-Item -LiteralPath $dependency
}
$references = @($netReferences; $unityReferences; $uiReferences) | Sort-Object FullName -Unique | ForEach-Object { '-r:"' + $_.FullName + '"' }
$sources = Get-ChildItem (Join-Path $project 'Assets/Scripts') -Recurse -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
$runtimeRsp = Join-Path $output 'runtime.rsp'
@('-nologo'; '-target:library'; '-nostdlib+'; '-langversion:9'; ('-out:"' + $output + '/EnemyRuntime.dll"'); $references; $sources) | Set-Content $runtimeRsp
& dotnet $compiler ('@' + $runtimeRsp)
if ($LASTEXITCODE -ne 0) { throw 'Runtime compilation failed' }
$editorSources = Get-ChildItem (Join-Path $project 'Assets/Editor') -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
$editorRsp = Join-Path $output 'editor.rsp'
@('-nologo'; '-target:library'; '-nostdlib+'; '-langversion:9'; '-define:UNITY_EDITOR'; ('-out:"' + $output + '/EnemyEditor.dll"'); $references; $sources; $editorSources) | Set-Content $editorRsp
& dotnet $compiler ('@' + $editorRsp)
if ($LASTEXITCODE -ne 0) { throw 'Editor compilation failed' }
$core = Join-Path $UnityEditor 'Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
$checksRsp = Join-Path $output 'checks.rsp'
@('-nologo'; '-target:exe'; '-nostdlib+'; '-langversion:9'; ('-out:"' + $output + '/EnemyScheduleChecks.dll"'); $references;
    ('"' + $project + '/Assets/Scripts/GameScene/EnemyDefinition.cs"');
    ('"' + $project + '/Assets/Scripts/GameScene/EnemyMovementBalance.cs"'); ('"' + $PSScriptRoot + '/EnemyScheduleChecks.cs"')) | Set-Content $checksRsp
& dotnet $compiler ('@' + $checksRsp)
if ($LASTEXITCODE -ne 0) { throw 'Schedule check compilation failed' }
Copy-Item -LiteralPath $core -Destination $output -Force
'{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' | Set-Content (Join-Path $output 'EnemyScheduleChecks.runtimeconfig.json')
& dotnet (Join-Path $output 'EnemyScheduleChecks.dll')
if ($LASTEXITCODE -ne 0) { throw 'Schedule checks failed' }
Write-Output 'PASS: all runtime scripts and editor smoke tests compile.'
