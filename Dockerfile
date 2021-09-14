﻿FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["fanuc/fanuc.csproj", "fanuc/"]
RUN dotnet restore "fanuc/fanuc.csproj"
COPY . .
WORKDIR "/src/fanuc"
RUN dotnet build "fanuc.csproj" -c Release -o /app/build /nowarn:CS0618 /nowarn:CS8632 /nowarn:CS1998  -p:DefineConstants=LINUX64

FROM build AS publish
RUN dotnet publish "fanuc.csproj" -c Release -o /app/publish /nowarn:CS0618 /nowarn:CS8632 /nowarn:CS1998  -p:DefineConstants=LINUX64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "fanuc.dll"]
