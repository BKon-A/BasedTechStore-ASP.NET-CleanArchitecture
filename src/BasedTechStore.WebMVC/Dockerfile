FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS aspnet
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/BasedTechStore.WebMVC/BasedTechStore.WebMVC.csproj", "src/BasedTechStore.WebMVC/"]
COPY ["src/BasedTechStore.Infrastructure/BasedTechStore.Infrastructure.csproj", "src/BasedTechStore.Infrastructure/"]
COPY ["src/BasedTechStore.Application/BasedTechStore.Application.csproj", "src/BasedTechStore.Application/"]
COPY ["src/BasedTechStore.Domain/BasedTechStore.Domain.csproj", "src/BasedTechStore.Domain/"]
COPY ["src/BasedTechStore.Common/BasedTechStore.Common.csproj", "src/BasedTechStore.Common/"]

RUN dotnet restore "src/BasedTechStore.WebMVC/BasedTechStore.WebMVC.csproj"

COPY . .

WORKDIR "/src/src/BasedTechStore.WebMVC"
RUN dotnet build "BasedTechStore.WebMVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasedTechStore.WebMVC.csproj" -c Release -o /app/publish

FROM aspnet AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "BasedTechStore.WebMVC.dll"]