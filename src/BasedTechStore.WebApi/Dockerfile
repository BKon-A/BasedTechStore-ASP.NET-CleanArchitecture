FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS aspnet

WORKDIR /app
EXPOSE 80

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BasedTechStore.WebApi/BasedTechStore.WebApi.csproj", "BasedTechStore.WebApi/"]
COPY ["BasedTechStore.Infrastructure/BasedTechStore.Infrastructure.csproj", "BasedTechStore.Infrastructure/"]
COPY ["BasedTechStore.Application/BasedTechStore.Application.csproj", "BasedTechStore.Application/"]
COPY ["BasedTechStore.Domain/BasedTechStore.Domain.csproj", "BasedTechStore.Domain/"]
COPY ["BasedTechStore.Common/BasedTechStore.Common.csproj", "BasedTechStore.Common/"]

RUN dotnet restore "BasedTechStore.WebApi/BasedTechStore.WebApi.csproj"

COPY . .

WORKDIR "/src/BasedTechStore.WebApi"
RUN dotnet build "BasedTechStore.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasedTechStore.WebApi.csproj" -c Release -o /app/publish

FROM aspnet AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/data-protection-keys
RUN mkdir -p /app/uploads

# Set proper permissions
RUN chown -R www-data:www-data /app/data-protection-keys
RUN chown -R www-data:www-data /app/uploads

ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "BasedTechStore.WebApi.dll"]