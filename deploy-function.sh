#!/bin/bash

# Exit on any error
set -e

ARTIFACT_PATH="./bin/Release/net8.0/linux-x64/publish"

echo "Starting Function deployment process..."

cd ./src/InventoryApi

# Restore packages
dotnet restore

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean

# Build and publish
echo "Building application..."
dotnet build
dotnet publish \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -p:PublishReadyToRun=false

# Create a ZIP file of the published app
cd "$ARTIFACT_PATH"
zip -r ../../../../../function.zip .
cd ../../../../../

# Stop the function app before deployment
# echo "Stopping Function App..."
# az functionapp stop -g inventory-api-rg -n $FUNCTION_APP_NAME

# Deploy the ZIP file
echo "Deploying to Azure..."

# Get Function App name
export FUNCTION_APP_NAME=$(cd ../../infra && terraform output -raw function_api_app_name)
export RESOURCE_GROUP_NAME=$(cd ../../infra && terraform output -raw resource_group_name)

echo $FUNCTION_APP_NAME
echo $RESOURCE_GROUP_NAME

az functionapp deployment source config-zip \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$FUNCTION_APP_NAME" \
    --src "function.zip"

rm -f function.zip

# Start the function app
# echo "Starting Function App..."
# az functionapp start -g inventory-api-rg -n $FUNCTION_APP_NAME


echo "Deployment completed successfully!"