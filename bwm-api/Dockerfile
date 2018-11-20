FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["bwm-api.csproj", "bwm-api/"]
RUN dotnet restore "bwm-api/bwm-api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "bwm-api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "bwm-api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "bwm-api.dll"]