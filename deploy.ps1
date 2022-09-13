[CmdletBinding()]
Param(
    [Parameter(
            Mandatory,
            HelpMessage = "Unique name for the deployed Azure Container Apps")]
    [Alias("n","Name")]
    [string] $unique_app_name,

    [Parameter(
            Mandatory,
            HelpMessage = "Azure Resource Group name")]
    [Alias("rg","ResourceGroup")]
    [string] $azure_resource_group_name,

    [Parameter(
            Mandatory,
            HelpMessage = "Azure Resource Group location")]
    [Alias("l","Location")]
    [string] $azure_resource_group_location
)

# Build project
dotnet publish ./src/Akka.ShoppingCart/Akka.ShoppingCart.csproj --configuration Release

# Login to Azure
">>> Logging in to Azure"
az login

# Flex bicep
">>> Provisioning Azure resources"
az deployment group create `
    --resource-group "$($azure_resource_group_name)" `
    --template-file 'flex/main.bicep' `
    --parameters location="$($azure_resource_group_location)" `
        appName="$($unique_app_name)" `
    --debug

# Webapp deploy
">>> Deploying Webapp"
az webapp deploy `
    --name "$($unique_app_name)" `
    --resource-group "$($azure_resource_group_name)" `
    --clean true `
    --restart true `
    --type zip `
    --src-path cluster.zip `
    --debug