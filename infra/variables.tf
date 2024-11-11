variable "api_version" {
  type = string
  description = "The version of the API"
  default = "v1.0.0"
}

variable "project_name" {
  type = string
  description = "The name of the project"
  default = "inventory"
}

variable "location" {
  type = string
  description = "The Azure region to deploy resources to"
  default = "centralus"
}

variable "resource_group_name" {
  type = string
  description = "The name of the resource group"
  default = "inventory-rg"
}