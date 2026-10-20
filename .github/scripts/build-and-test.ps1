param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-CheckedCommand {
    param([string]$FilePath, [string[]]$Arguments)
    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao executar '$FilePath': codigo $LASTEXITCODE."
    }
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repositoryRoot
try {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    $visualStudio = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
    if (-not $visualStudio) { throw 'Visual Studio com MSBuild nao encontrado.' }
    $msbuild = Join-Path $visualStudio 'MSBuild/Current/Bin/MSBuild.exe'
    $vstest = Join-Path $visualStudio 'Common7/IDE/Extensions/TestPlatform/vstest.console.exe'
    $nuget = (Get-Command nuget.exe -ErrorAction Stop).Source

    Invoke-CheckedCommand $msbuild @(
        'src/ConnectionFactory.sln', '/t:Restore,Rebuild',
        '/p:RestorePackagesConfig=true', "/p:Configuration=$Configuration", '/v:minimal', '/nologo'
    )

    $localDb = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    $localDbPath = if ($localDb) { $localDb.Source } else {
        Get-ChildItem "$env:ProgramFiles/Microsoft SQL Server/*/Tools/Binn/SqlLocalDB.exe" |
            Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
    if (-not $localDbPath) { throw 'SQL Server LocalDB nao encontrado; testes de banco sao obrigatorios.' }
    Invoke-CheckedCommand $localDbPath @('create', 'ConnectionFactoryCi', '-s')

    # Altera somente a configuracao gerada; o App.config do desenvolvedor e preservado.
    $testAssembly = "test/ConnectionFactoryTest/bin/$Configuration/ConnectionFactoryTest.dll"
    $testConfigPath = "$testAssembly.config"
    [xml]$testConfig = Get-Content -LiteralPath $testConfigPath -Raw
    foreach ($connectionName in @('teste', 'AppVeyor')) {
        $connection = $testConfig.configuration.connectionStrings.add |
            Where-Object { $_.name -eq $connectionName }
        if (-not $connection) { throw "Conexao de teste '$connectionName' nao encontrada." }
        $connection.connectionString = 'Data Source=(localdb)\ConnectionFactoryCi;Initial Catalog=tempdb;Integrated Security=True'
    }
    $testConfig.Save([IO.Path]::GetFullPath($testConfigPath))
    $testResults = Join-Path $repositoryRoot 'artifacts/test-results'
    New-Item -ItemType Directory -Path $testResults -Force | Out-Null
    Invoke-CheckedCommand $vstest @(
        $testAssembly, '/Logger:trx;LogFileName=tests.trx', "/ResultsDirectory:$testResults"
    )
    [xml]$trx = Get-Content (Join-Path $testResults 'tests.trx') -Raw
    $counters = $trx.TestRun.ResultSummary.Counters
    if ([int]$counters.total -lt 4 -or [int]$counters.passed -ne [int]$counters.total) {
        throw 'Todos os testes, incluindo banco e seguranca XML, precisam ser descobertos e aprovados.'
    }

    $packageDirectory = Join-Path $repositoryRoot 'artifacts/package'
    New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
    Invoke-CheckedCommand $nuget @(
        'pack', 'src/ConnectionFactory.nuspec', '-Properties', "Configuration=$Configuration",
        '-OutputDirectory', $packageDirectory, '-NonInteractive'
    )
    [xml]$manifest = Get-Content 'src/ConnectionFactory.nuspec' -Raw
    $version = [string]$manifest.package.metadata.version
    $packagePath = Join-Path $packageDirectory "ConnectionFactory.NET.$version.nupkg"
    if (-not (Test-Path $packagePath)) { throw 'Pacote esperado nao foi gerado.' }
    Copy-Item "src/bin/$Configuration/ConnectionFactory.dll" $packageDirectory
    Copy-Item "src/bin/$Configuration/ConnectionFactory.XML" $packageDirectory
    [string]$manifest.package.metadata.releaseNotes |
        Set-Content (Join-Path $packageDirectory 'RELEASE_NOTES.md') -Encoding utf8
    if ($env:GITHUB_OUTPUT) { "version=$version" | Add-Content $env:GITHUB_OUTPUT }
    Write-Host "Pacote $version validado com $($counters.passed) testes aprovados."
}
finally {
    Pop-Location
}
