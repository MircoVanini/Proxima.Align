param(
    [string]$VsixPath = (
        Join-Path $PSScriptRoot '..\src\Proxima.Align\bin\Release\net8.0-windows8.0\Proxima.Align.vsix'
    )
)

$ErrorActionPreference = 'Stop'

$expectedId = 'Proxima.Align.02d9493a-1406-4d2a-aa3b-2d686783003e'
$versionPropsPath = Join-Path $PSScriptRoot '..\Version.props'
[xml]$versionProps = Get-Content -LiteralPath $versionPropsPath -Raw
$projectVersion = [System.Version]$versionProps.Project.PropertyGroup.Version
$expectedVersion = [System.Version]::new(
    $projectVersion.Major,
    $projectVersion.Minor,
    $projectVersion.Build,
    0
).ToString()
$requiredFiles = @(
    'Assets/icon.png',
    'Assets/preview.png',
    'LICENSE.txt',
    'RELEASE-NOTES.txt'
)

if (-not (Test-Path -LiteralPath $VsixPath -PathType Leaf)) {
    throw "VSIX not found: $VsixPath"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $VsixPath))

try {
    $manifestEntry = $archive.GetEntry('extension.vsixmanifest')
    if ($null -eq $manifestEntry) {
        throw 'extension.vsixmanifest is missing from the VSIX.'
    }

    $reader = [System.IO.StreamReader]::new($manifestEntry.Open())
    try {
        [xml]$manifest = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }

    $namespace = [System.Xml.XmlNamespaceManager]::new($manifest.NameTable)
    $namespace.AddNamespace('v', 'http://schemas.microsoft.com/developer/vsx-schema/2011')
    $identity = $manifest.SelectSingleNode('//v:Identity', $namespace)
    $metadata = $manifest.SelectSingleNode('//v:Metadata', $namespace)

    if ($identity.Id -ne $expectedId) {
        throw "Unexpected VSIX ID: $($identity.Id)"
    }

    if ($identity.Version -ne $expectedVersion) {
        throw "Unexpected VSIX version: $($identity.Version)"
    }

    if ($metadata.Preview -and $metadata.Preview.ToLowerInvariant() -ne 'false') {
        throw 'The VSIX is still marked as Preview.'
    }

    foreach ($file in $requiredFiles) {
        if ($null -eq $archive.GetEntry($file)) {
            throw "Required package file is missing: $file"
        }
    }
}
finally {
    $archive.Dispose()
}

Write-Host "Verified Marketplace-ready VSIX: $VsixPath"
