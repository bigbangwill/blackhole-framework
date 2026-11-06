param(
    [Parameter(Mandatory = $true)][string]$PlayerDirectory,
    [Parameter(Mandatory = $true)][ValidateSet('Client', 'Server')][string]$Role,
    [Parameter(Mandatory = $true)][string]$CecilPath,
    [Parameter(Mandatory = $true)][string]$ReportPath,
    [switch]$Probe
)

$ErrorActionPreference = 'Stop'
[System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $CecilPath).Path) | Out-Null
$playerRoot = (Resolve-Path -LiteralPath $PlayerDirectory).Path
$oppositeRole = if ($Role -eq 'Client') { 'Server' } else { 'Client' }
$problems = [System.Collections.Generic.List[string]]::new()
$managed = [System.Collections.Generic.List[object]]::new()
$native = [System.Collections.Generic.List[string]]::new()
$manifestFields = @()
$manifestGetterFields = @()
$installerRegistrationTypes = @()
$expectedRuntime = "Blackhole.Networking.Infrastructure.$Role.Utp${Role}Runtime"
$runtimeFound = $false
$probeFound = $false

foreach ($file in Get-ChildItem -LiteralPath $playerRoot -Filter '*.dll' -Recurse) {
    $assembly = $null
    try { $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($file.FullName) }
    catch [System.BadImageFormatException] { $native.Add($file.FullName.Substring($playerRoot.Length + 1)); continue }
    try {
        $name = $assembly.Name.Name
        $references = @($assembly.MainModule.AssemblyReferences | ForEach-Object Name)
        $types = @($assembly.MainModule.GetTypes() | ForEach-Object FullName)
        foreach ($candidate in @($name) + $references) {
            if ($candidate -eq "Blackhole.Networking.Composition.$oppositeRole" -or $candidate -eq "Blackhole.Networking.Infrastructure.$oppositeRole") {
                $problems.Add("Opposite-role assembly or reference in ${name}: $candidate")
            }
            if ($candidate -like 'UnityEditor*' -or $candidate -like 'nunit*' -or ($candidate -like 'Blackhole.*' -and ($candidate -like '*.Editor' -or $candidate -like '*Tests' -or $candidate -like '*.TestSupport'))) {
                $problems.Add("Editor/test assembly or reference in ${name}: $candidate")
            }
        }
        foreach ($type in $types) {
            if ($type -like "Blackhole.Networking.Infrastructure.$oppositeRole.*" -or $type -like "Blackhole.Networking.Composition.$oppositeRole.*") {
                $problems.Add("Opposite-role implementation type in ${name}: $type")
            }
        }
        if ($types -contains $expectedRuntime) { $runtimeFound = $true }
        if ($name -eq 'Blackhole.Development.BuildIsolation') { $probeFound = $true }
        if ($name -eq 'Blackhole.ModuleSystem.Composition') {
            $manifest = $assembly.MainModule.Types | Where-Object FullName -eq 'Blackhole.ModuleSystem.Composition.PlatformModuleManifestSO'
            $manifestFields = @($manifest.Fields | ForEach-Object Name)
            $getter = $manifest.Methods | Where-Object Name -eq 'get_Installer'
            $manifestGetterFields = @($getter.Body.Instructions | Where-Object { $_.Operand -is [Mono.Cecil.FieldReference] } | ForEach-Object { $_.Operand.Name })
        }
        if ($name -eq "Blackhole.Networking.Composition.$Role") {
            $installerType = $assembly.MainModule.Types | Where-Object Name -eq "Networking${Role}Installer"
            $install = $installerType.Methods | Where-Object Name -eq 'InstallModule'
            $installerRegistrationTypes = @($install.Body.Instructions | Where-Object { $_.Operand -is [Mono.Cecil.GenericInstanceMethod] } | ForEach-Object { $_.Operand.GenericArguments } | ForEach-Object FullName)
        }
        $managed.Add([ordered]@{
            file = $file.FullName.Substring($playerRoot.Length + 1)
            name = $name
            sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            references = $references
            types = $types
        })
    }
    finally { $assembly.Dispose() }
}

if (-not $runtimeFound) { $problems.Add("Expected runtime type was absent: $expectedRuntime") }
$selectedField = '_' + $Role.ToLowerInvariant() + 'Installer'
$oppositeField = '_' + $oppositeRole.ToLowerInvariant() + 'Installer'
if ($manifestFields -notcontains $selectedField) { $problems.Add("Selected manifest field was absent: $selectedField") }
if ($manifestFields -contains $oppositeField) { $problems.Add("Opposite manifest field remains: $oppositeField") }
if ($manifestGetterFields -notcontains $selectedField -or $manifestGetterFields -contains $oppositeField) { $problems.Add('Compiled manifest getter does not select only the expected installer field') }
if ($installerRegistrationTypes -notcontains $expectedRuntime) { $problems.Add('Compiled installer does not register the expected transport runtime') }
if ($Probe -and -not $probeFound) { $problems.Add('Requested runtime audit probe was absent') }
if (-not $Probe -and $probeFound) { $problems.Add('Runtime audit probe leaked into an ordinary build') }

$report = [ordered]@{
    passed = $problems.Count -eq 0
    role = $Role
    directory = $playerRoot
    probe = [bool]$Probe
    expectedRuntime = $expectedRuntime
    manifestFields = $manifestFields
    manifestGetterFields = $manifestGetterFields
    installerRegistrationTypes = $installerRegistrationTypes
    errors = @($problems.ToArray())
    managedAssemblies = @($managed.ToArray())
    nativeLibraries = @($native.ToArray())
}
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $ReportPath -Encoding utf8
Write-Output "$Role player: $($managed.Count) managed assemblies inspected; $($problems.Count) isolation errors."
if ($problems.Count -ne 0) { $problems | Write-Output; exit 1 }
