# Api

```
# Generate a PAT with read:packages scope.
dotnet nuget add source --username whatever --password "<PAT>" --store-password-in-clear-text --name github "https://nuget.pkg.github.com/strelka-skaut/index.json"
```

## EF

### Install

```
dotnet tool install dotnet-ef --global
```

### Add Migration

```
dotnet ef migrations add <name> --project Data --startup-project Api
```
