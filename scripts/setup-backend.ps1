$ErrorActionPreference = "Stop"
$slnPath = "src/backend"
$srcPath = "src/backend/src"

# Create Solution
Write-Host "Creating Solution..."
dotnet new sln -n TroLiKOC -o $slnPath

# Create API
Write-Host "Creating API..."
dotnet new webapi -n TroLiKOC.API -o "$srcPath/TroLiKOC.API"
dotnet sln "$slnPath/TroLiKOC.sln" add "$srcPath/TroLiKOC.API/TroLiKOC.API.csproj"

# Create SharedKernel
Write-Host "Creating SharedKernel..."
dotnet new classlib -n TroLiKOC.SharedKernel -o "$srcPath/TroLiKOC.SharedKernel"
dotnet sln "$slnPath/TroLiKOC.sln" add "$srcPath/TroLiKOC.SharedKernel/TroLiKOC.SharedKernel.csproj"

# Create JobOrchestrator
Write-Host "Creating JobOrchestrator..."
dotnet new classlib -n TroLiKOC.JobOrchestrator -o "$srcPath/TroLiKOC.JobOrchestrator"
dotnet sln "$slnPath/TroLiKOC.sln" add "$srcPath/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj"

# Create Modules
$modules = @("Identity", "Wallet", "Subscription", "Jobs", "Payment")
foreach ($module in $modules) {
    Write-Host "Creating Module: $module..."
    dotnet new classlib -n "TroLiKOC.Modules.$module" -o "$srcPath/Modules/TroLiKOC.Modules.$module"
    dotnet sln "$slnPath/TroLiKOC.sln" add "$srcPath/Modules/TroLiKOC.Modules.$module/TroLiKOC.Modules.$module.csproj"
}

# Add References (SharedKernel to everyone)
Write-Host "Adding References..."
dotnet add "$srcPath/TroLiKOC.API/TroLiKOC.API.csproj" reference "$srcPath/TroLiKOC.SharedKernel/TroLiKOC.SharedKernel.csproj"
dotnet add "$srcPath/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj" reference "$srcPath/TroLiKOC.SharedKernel/TroLiKOC.SharedKernel.csproj"

foreach ($module in $modules) {
    dotnet add "$srcPath/Modules/TroLiKOC.Modules.$module/TroLiKOC.Modules.$module.csproj" reference "$srcPath/TroLiKOC.SharedKernel/TroLiKOC.SharedKernel.csproj"
    # Add Module reference to API
    dotnet add "$srcPath/TroLiKOC.API/TroLiKOC.API.csproj" reference "$srcPath/Modules/TroLiKOC.Modules.$module/TroLiKOC.Modules.$module.csproj"
}

# Add JobOrchestrator reference to API
dotnet add "$srcPath/TroLiKOC.API/TroLiKOC.API.csproj" reference "$srcPath/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj"

# Add cross-module references needed for JobOrchestrator
dotnet add "$srcPath/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj" reference "$srcPath/Modules/TroLiKOC.Modules.Subscription/TroLiKOC.Modules.Subscription.csproj"
dotnet add "$srcPath/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj" reference "$srcPath/Modules/TroLiKOC.Modules.Wallet/TroLiKOC.Modules.Wallet.csproj"

Write-Host "Solution setup complete!"
