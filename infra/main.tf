terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.00"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "inventory-api-rg"
  location = var.location
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}