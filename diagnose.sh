#!/bin/bash

# Variables
RESOURCE_GROUP="inventory-api-rg"
FUNCTION_APP_NAME="inventory-api-fa-v1-0-0"


echo "Checking Function App status..."
az functionapp show \
    --resource-group $RESOURCE_GROUP \
    --name $FUNCTION_APP_NAME \
    --query "{State:state,RuntimeVersion:siteConfig.netFrameworkVersion}" \
    --output table

echo "Checking application settings..."
az functionapp config appsettings list \
    --resource-group $RESOURCE_GROUP \
    --name $FUNCTION_APP_NAME \
    --output table

echo "Checking Function App logs..."
az functionapp log deployment list \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP

# az functionapp logs tail \
#     --resource-group $RESOURCE_GROUP \
#     --name $FUNCTION_APP_NAME

# Optional: Restart the Function App
echo "Would you like to restart the Function App? (y/n)"
read -r answer
if [ "$answer" = "y" ]; then
    echo "Restarting Function App..."
    az functionapp restart \
        --resource-group $RESOURCE_GROUP \
        --name $FUNCTION_APP_NAME
fi