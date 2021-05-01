#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SampleApi3/SampleApi3.csproj", "SampleApi3/"]
RUN dotnet restore "SampleApi3/SampleApi3.csproj"
COPY . .
WORKDIR "/src/SampleApi3"
RUN dotnet build "SampleApi3.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleApi3.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleApi3.dll"]