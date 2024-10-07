variable "api_version" {
  description = "The version of the API"
  default     = "v1.0.0"
}

variable "location" {
  description = "The Azure region to deploy resources to"
  default     = "Central US"
}

variable "resource_group_name" {
  description = "The name of the resource group"
  default     = "inventory-api-rg"
}