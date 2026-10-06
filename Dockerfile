FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/JARemodeling.Web/JARemodeling.Web.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /app/publish \
    -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render Free = 512 MB. Prefer workstation GC and cap the heap.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_GCServer=0 \
    DOTNET_GCHeapHardLimit=268435456 \
    DOTNET_ThreadPool_ForceMinWorkerThreads=2 \
    DOTNET_ThreadPool_ForceMaxWorkerThreads=8

EXPOSE 8080
ENTRYPOINT ["/bin/sh", "-c", "dotnet JARemodeling.Web.dll --urls http://0.0.0.0:${PORT:-8080}"]
