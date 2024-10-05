resource "azurerm_api_management" "apim" {
  name                = "inventory-apim"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  publisher_name      = "Your Company"
  publisher_email     = "your-email@example.com"

  sku_name = "Developer_1"
}

resource "azurerm_api_management_api" "inventory_api" {
  name                = "inventory-api"
  resource_group_name = azurerm_resource_group.rg.name
  api_management_name = azurerm_api_management.apim.name
  revision            = "1"
  display_name        = "Inventory API"
  path                = "inventory"
  protocols           = ["https"]

  import {
    content_format = "swagger-json"
    content_value  = jsonencode({
      swagger = "2.0"
      info = {
        version = var.api_version
        title   = "Inventory API"
      }
      host = azurerm_function_app.fa.default_hostname
      basePath = "/api"
      schemes = ["https"]
      paths = {
        "/items" = {
          get = {
            responses = {
              "200" = {
                description = "Successful response"
              }
            }
          }
          post = {
            responses = {
              "200" = {
                description = "Successful response"
              }
            }
          }
        }
        "/items/{id}" = {
          get = {
            parameters = [
              {
                name = "id"
                in = "path"
                required = true
                type = "integer"
              }
            ]
            responses = {
              "200" = {
                description = "Successful response"
              }
            }
          }
        }
      }
    })
  }
}

resource "azurerm_api_management_api_policy" "inventory_api_policy" {
  api_name            = azurerm_api_management_api.inventory_api.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_resource_group.rg.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service base-url="https://${azurerm_function_app.fa.default_hostname}/api" />
  </inbound>
</policies>
XML
}