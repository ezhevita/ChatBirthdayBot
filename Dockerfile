FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["ChatBirthdayBot/ChatBirthdayBot.csproj", "ChatBirthdayBot/"]
RUN dotnet restore "ChatBirthdayBot/ChatBirthdayBot.csproj"
COPY . .
WORKDIR "/src/ChatBirthdayBot"
RUN dotnet build "ChatBirthdayBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatBirthdayBot.csproj" -c Release -r linux-musl-x64 --self-contained false -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatBirthdayBot.dll"]
