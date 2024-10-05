# Inventory API

This project contains an Azure Function App that serves as an Inventory API, along with Terraform configurations for deploying the Function App and exposing it through Azure API Management.

## Project Structure

- `src/InventoryApi/`: Contains the Azure Function App code
- `infrastructure/`: Contains Terraform configuration files
- `tests/`: Contains unit tests for the Function App

## Setup

1. Install the [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#install-the-azure-functions-core-tools)
2. Install [Terraform](https://www.terraform.io/downloads.html)
3. Install the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

## Deployment

1. Navigate to the `infrastructure/` directory
2. Initialize Terraform: `terraform init`
3. Plan the deployment: `terraform plan`
4. Apply the configuration: `terraform apply`

## Testing

Run the unit tests using the following command:

```
dotnet test tests/InventoryApi.Tests/InventoryApi.Tests.csproj
```

## API Endpoints

- GET /api/items: Retrieve all items
- GET /api/items/{id}: Retrieve a specific item
- POST /api/items: Add a new item

The API is accessible through the Azure API Management gateway URL, which is output after running `terraform apply`.