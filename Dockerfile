FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["cronograma_atividades_backend.csproj", "./"]
RUN dotnet restore "cronograma_atividades_backend.csproj" -a $TARGETARCH
COPY . .
RUN dotnet publish "cronograma_atividades_backend.csproj" -c Release -o /app/publish -a $TARGETARCH --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cronograma_atividades_backend.dll"]