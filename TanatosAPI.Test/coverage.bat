REM dotnet tool install --global dotnet-reportgenerator-globaltool

if exist coverage rmdir /s /q coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/Program.cs,**/*.g.cs" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]Microsoft.AspNetCore.OpenApi.Generated.*,[*]System.Runtime.CompilerServices.*"
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:Html
start coverage/report/index.html