#!/bin/bash

# Variables
resource_group=MerchStoreRG
location=northeurope
env_name=MerchStoreEnv
app_name=merchstore
app_port=8080
image=ghcr.io/samarbetsorganisation/project9
workspace_name=MerchStoreWorkspace

# Step 1: Create Resource Group
az group create --location $location --name $resource_group

# Step 2: Create Log Analytics Workspace
az monitor log-analytics workspace create \
    --resource-group $resource_group \
    --workspace-name $workspace_name \
    --location $location

# Step 3: Get Workspace ID
workspace_id=$(az monitor log-analytics workspace show \
    --resource-group $resource_group \
    --workspace-name $workspace_name \
    --query customerId \
    --output tsv)

# Step 4: Create Container App Environment
az containerapp env create \
    --name $env_name \
    --resource-group $resource_group \
    --location $location \
    --logs-workspace-id $workspace_id

# Step 5: Deploy the Container App
az containerapp create --name $app_name --resource-group $resource_group \
                       --image $image \
                       --environment $env_name \
                       --target-port $app_port \
                       --ingress external --query properties.configuration.ingress.fqdn