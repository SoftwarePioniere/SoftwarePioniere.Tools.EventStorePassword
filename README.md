# SoftwarePioniere.Tools.EventStorePassword

dotnet global tool to change event store password

## Install Tool

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
dotnet pack
dotnet tool install --global --add-source ./nupkg SoftwarePioniere.Tools.EventStorePassword
dotnet tool uninstall -g SoftwarePioniere.Tools.EventStorePassword
```
