# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

COPY . /source

WORKDIR /source/MerchStore.WebUI

ARG TARGETARCH
# Set DOTNET_ARCH based on the value of TARGETARCH
RUN if [ "$TARGETARCH" = "amd64" ]; then export DOTNET_ARCH="x64"; else export DOTNET_ARCH="$TARGETARCH"; fi && \
    dotnet publish -a ${DOTNET_ARCH:-x64} --use-current-runtime --self-contained false -o /app

################################################################################

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app .

USER $APP_UID

ENTRYPOINT ["dotnet", "MerchStore.WebUI.dll"]