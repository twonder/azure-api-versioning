# Cosmos DB Account
resource "azurerm_cosmosdb_account" "db" {
  name = "cosmosdb-${var.project_name}"
  location = var.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type = "Standard"
  kind = "GlobalDocumentDB"
  
  consistency_policy {
    consistency_level = "Session"
    max_interval_in_seconds = 5
    max_staleness_prefix = 100
  }

  geo_location {
    location = var.location
    failover_priority = 0
  }

  capabilities {
    name = "EnableServerless"
  }

  backup {
    type = "Periodic"
    interval_in_minutes = 240
    retention_in_hours = 8
  }

  tags = {
    Project = var.project_name
  }
}

# Cosmos DB Database
resource "azurerm_cosmosdb_sql_database" "database" {
  name = "${var.project_name}-db"
  resource_group_name = azurerm_resource_group.rg.name
  account_name = azurerm_cosmosdb_account.db.name
}

# Cosmos DB Container
resource "azurerm_cosmosdb_sql_container" "inventory" {
  name = "${var.project_name}-items"
  resource_group_name = azurerm_resource_group.rg.name
  account_name = azurerm_cosmosdb_account.db.name
  database_name = azurerm_cosmosdb_sql_database.database.name
  partition_key_paths = ["/partitionKey"]

  # Indexing policy optimized for your queries
  indexing_policy {
    indexing_mode = "consistent"
    
    included_path {
      path = "/inventoryDate/?"
    }
    
    excluded_path {
      path = "/*"
    }

    composite_index {
      index {
        path  = "/productId"
        order = "ascending"
      }
      index {
        path  = "/inventoryDate"
        order = "ascending"
      }
    }
  }

  # Enable TTL if needed
  default_ttl = 8760  # Uncomment and set value in seconds if you want TTL
}

resource "azurerm_cosmosdb_sql_container" "lease" {
  name = "${var.project_name}-leases"
  resource_group_name = azurerm_resource_group.rg.name
  account_name = azurerm_cosmosdb_account.db.name
  database_name = azurerm_cosmosdb_sql_database.database.name
  partition_key_paths = ["/id"]

  default_ttl = 1209600  # Never expire

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }
  }
}

# outputs.tf
output "cosmos_account_name" {
  value = azurerm_cosmosdb_account.db.name
}

output "cosmos_db_name" {
  value = azurerm_cosmosdb_sql_database.database.name
}

output "cosmos_container_name" {
  value = azurerm_cosmosdb_sql_container.inventory.name
}

output "cosmos_db_endpoint" {
  value = azurerm_cosmosdb_account.db.endpoint
}

output "cosmos_db_primary_key" {
  value = azurerm_cosmosdb_account.db.primary_key
  sensitive = true
}

