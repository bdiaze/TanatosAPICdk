REM dotnet tool install --global dotnet-reportgenerator-globaltool

if exist coverage rmdir /s /q coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:Html -filefilters:"-*.g.cs;-*Program.cs" -classfilters:"-Microsoft.AspNetCore.OpenApi.Generated;-System.Runtime.CompilerServices"
start coverage/report/index.html