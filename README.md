# SoftwarePioniere.Tools.EventStorePassword

dotnet global tool to change event store password

## Installation

```
dotnet tool install --global SoftwarePioniere.Tools.EventStorePassword
```

## Usage

```
dotnet eventstore-password 

dotnet eventstore-password --help
```

## Development

```
dotnet pack
dotnet tool install --global --add-source ./nupkg SoftwarePioniere.Tools.EventStorePassword
dotnet tool uninstall -g SoftwarePioniere.Tools.EventStorePassword
```

## Publish

```
del .\nupkg\*.*
dotnet pack -c Release
dotnet nuget push .\nupkg\SoftwarePioniere.Tools.EventStorePassword.1.0.0.nupkg -s https://api.nuget.org/v3/index.json -k api-key
```
