#!/bin/bash

# Exit on any error
set -e

echo "Starting deployment process..."

# Deploy infrastructure
cd infra
terraform apply -auto-approve

# Export the cosmos connection details to a json file
export COSMOS_DB_ENDPOINT=$(terraform output -raw cosmos_db_endpoint)
export COSMOS_DB_PRIMARY_KEY=$(terraform output -raw cosmos_db_primary_key)
export COSMOS_DB_NAME=$(terraform output -raw cosmos_db_name)
export COSMOS_CONTAINER_NAME=$(terraform output -raw cosmos_container_name)
# Get Function App name
export FUNCTION_APP_NAME=$(terraform output -raw function_app_name)
export RESOURCE_GROUP_NAME=$(terraform output -raw resource_group_name)

cd ..

# Create/update local.settings.json
echo "Updating local settings..."
jq -n \
  --arg endpoint "$COSMOS_DB_ENDPOINT" \
  --arg key "$COSMOS_DB_PRIMARY_KEY" \
  --arg dbName "$COSMOS_DB_NAME" \
  --arg containerName "$COSMOS_CONTAINER_NAME" \
  '{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDb:EndpointUrl": $endpoint,
    "CosmosDb:PrimaryKey": $key,
    "CosmosDb:DatabaseName": $dbName,
    "CosmosDb:ContainerName": $containerName
  }
}' > ./src/InventoryAPI/local.settings.json
