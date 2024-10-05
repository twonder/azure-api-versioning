#!/bin/bash

# Deploy infrastructure
cd infra
terraform apply -auto-approve

# Get Function App name
function_app_name=$(terraform output -raw function_app_name)

# Build and publish the Function App
cd ../src/InventoryApi
dotnet publish -c Release

# Create a ZIP file of the published app
cd bin/Release/net8.0/publish
zip -r ../../../../../function-app.zip .
cd ../../../../..

# Deploy the ZIP file to the Function App
az functionapp deployment source config-zip -g rg-inventoryapi-dev -n $function_app_name --src function-app.zip

echo "Deployment completed successfully!"