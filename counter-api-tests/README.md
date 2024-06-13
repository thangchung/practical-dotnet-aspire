# Get starting

## Prerequisite

```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## Actions

```powershell
dotnet test --settings tests.runsettings
```

```powershell
reportgenerator `
	-reports:".\**\TestResults\**\coverage.cobertura.xml" `
	-targetdir:"coverage" `
	-reporttypes:Html
```

```powershell
.\coverage\index.htm
```

## Refs

- https://github.com/thangchung/setup-dotnet-test-projects/tree/main/src/Services/PeopleService/DNP.PeopleService.Tests
- https://knowyourtoolset.com/2024/01/coverage-reports/
