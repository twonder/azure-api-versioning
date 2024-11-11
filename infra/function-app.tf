# Add Application Insights
resource "azurerm_application_insights" "appinsights" {
  name                = "appi-${var.project_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location           = azurerm_resource_group.rg.location
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.logs.id

  # Recommended for better cost management
  retention_in_days   = 30
}

# Add Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "logs" {
  name                = "log-${var.project_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location           = azurerm_resource_group.rg.location
  sku                = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_storage_account" "sa" {
  name = "invapibxdsa${replace(var.api_version, ".", "")}"
  resource_group_name = azurerm_resource_group.rg.name
  location = var.location
  account_tier = "Standard"
  account_replication_type = "LRS"
}

# App Service Plan
resource "azurerm_service_plan" "sp" {
  name = "${var.project_name}-api-plan-${replace(var.api_version, ".", "-")}"
  resource_group_name = azurerm_resource_group.rg.name
  location = var.location
  os_type = "Linux"  # Changed to Linux
  sku_name = "Y1" # Consumption plan
}

resource "azurerm_linux_function_app" "function" {  # Changed to linux_function_app
  name = "${var.project_name}-api-fa-${replace(var.api_version, ".", "-")}"
  resource_group_name = azurerm_resource_group.rg.name
  location = var.location
  storage_account_name = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  service_plan_id = azurerm_service_plan.sp.id

  site_config {
    application_stack {
      dotnet_version = "8.0"  # .NET Core version
      use_dotnet_isolated_runtime = true
    }

    cors {
      allowed_origins = ["*"]  # Configure as needed
    }

    application_insights_connection_string = azurerm_application_insights.appinsights.connection_string
    application_insights_key = azurerm_application_insights.appinsights.instrumentation_key
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet-isolated"
    "WEBSITE_RUN_FROM_PACKAGE" = "1"
    "FUNCTIONS_EXTENSION_VERSION" = "~4"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appinsights.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.appinsights.instrumentation_key
    # "ASPNETCORE_ENVIRONMENT" = "Production"
    # "AzureWebJobsStorage" = azurerm_storage_account.sa.primary_connection_string
    # "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa.primary_connection_string
    # "WEBSITE_CONTENTSHARE" = lower("${var.project_name}-api-fa-${replace(var.api_version, ".", "-")}")
  }

  identity {
    type = "SystemAssigned"
  }
}

output "function_app_name" {
  value = azurerm_linux_function_app.function.name
}

output "function_app_hostname" {
  value = azurerm_linux_function_app.function.default_hostname
}
