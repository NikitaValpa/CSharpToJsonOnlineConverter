FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY ./ .
RUN dotnet publish /src/CSharpToJson.WebAsm/Server/CSharpToJson.WebAsm.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CSharpToJson.WebAsm.Server.dll"]