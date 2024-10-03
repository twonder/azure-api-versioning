output "function_app_name" {
  value = azurerm_function_app.fa.name
}

output "function_app_hostname" {
  value = azurerm_function_app.fa.default_hostname
}

output "api_management_gateway_url" {
  value = azurerm_api_management.apim.gateway_url
}

output "api_management_api_url" {
  value = "${azurerm_api_management.apim.gateway_url}/${azurerm_api_management_api.inventory_api.path}"
}