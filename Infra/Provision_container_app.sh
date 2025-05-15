#!/bin/bash

resource_group=MerchStoreRG
location=northeurope
env_name=MerchStoreEnv
app_name=MerchStore
app_port=8080
image=ghcr.io/samarbetsorganisation/project9

az group create --location $location --name $resource_group

az containerapp env create --name $env_name --resource-group $resource_group --location $location

az containerapp create --name $app_name --resource-group $resource_group \
                       --image $image \
                       --environment $env_name \
                       --target-port $app_port \
                       --ingress external --query properties.configuration.ingress.fqdn