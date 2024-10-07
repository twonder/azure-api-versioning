resource "azurerm_storage_account" "sa" {
  name                     = "invapibxdsa${replace(var.api_version, ".", "")}"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_app_service_plan" "asp" {
  name                = "inventory-api-asp-${replace(var.api_version, ".", "-")}"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_function_app" "fa" {
  name                       = "inventory-api-fa-${replace(var.api_version, ".", "-")}"
  location                   = var.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.asp.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  version                    = "~3"

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                   = "dotnet"
    "WEBSITE_RUN_FROM_PACKAGE"                   = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"   = azurerm_storage_account.sa.primary_connection_string
    "WEBSITE_CONTENTSHARE"                       = lower("inventory-api-fa-${replace(var.api_version, ".", "-")}")
  }

  site_config {
    dotnet_framework_version = "v6.0"
  }
}