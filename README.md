# SoftwarePioniere.Tools.EventStorePassword

dotnet global tool to change event store password

## Installation

```
dotnet tool install --global SoftwarePioniere.Tools.EventStorePassword
```

## Usage

```
eventstore-password 

eventstore-password --help
```

## Development

```
dotnet tool install -g GitVersion.Tool

dotnet pack
dotnet tool install --global --add-source ./nupkg SoftwarePioniere.Tools.EventStorePassword
dotnet tool uninstall -g SoftwarePioniere.Tools.EventStorePassword


```

## Deployment

```
$j=(ConvertFrom-Json -InputObject  ([System.string]::Concat((dotnet-gitversion))))

$vv=$j.NugetVersionV2
$asv=$j.AssemblySemVer

dotnet pack -c Release -p:Version=$vv -p:AssemblyVersion=$asv -p:FileVersion=$asv

$pkg=(Get-ChildItem -Path .\nupkg\ -Filter "*$vv.nupkg").FullName
dotnet nuget push $pkg -s https://api.nuget.org/v3/index.json -k $env:NUGET_API_KEY
```
