from azure.identity import DefaultAzureCredential
from azure.mgmt.resource import ResourceManagementClient
from azure.monitor.query import LogsQueryClient
import os
from datetime import timedelta

# Authenticate
credential = DefaultAzureCredential()
subscription_id = "db6bec7e-09a1-4102-a4b4-9838df282aa9"

# List resources
resource_client = ResourceManagementClient(credential, subscription_id)
resources = resource_client.resources.list()

# Check usage
logs_client = LogsQueryClient(credential)
query = """
AzureActivity
| where TimeGenerated >= ago(30d)
| summarize count() by ResourceId
| where count_ == 0
"""

for resource in resources:
    response = logs_client.query_workspace(
        "03dbc802-f07f-46e7-9d7c-52e8eff02ffc", 
        query, 
        timespan=timedelta(days=30)
    )
    if not response.tables[0].rows:
        print(f"Resource {resource.name} is not being used.")

print("Done.")