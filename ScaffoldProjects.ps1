$solutionName = "BasedTechStore"
$projects = @(
    @{ Name = "$solutionName.Domain"; Type = "classlib" },
    @{ Name = "$solutionName.Application"; Type = "classlib" },
    @{ Name = "$solutionName.Infrastructure"; Type = "classlib" },
    @{ Name = "$solutionName.Web"; Type = "web" }
    @{ Name = "$solutionName.Tests"; Type = "xunit" }
)

foreach ($project in $projects) 
{
    if(Test-Path $project.Name)
    {
        Write-Host "Project $project.Name already exists"
        continue
    }
    $projectName = $project.Name
    $projectType = $project.Type

    Write-Host "Creating project $projectName of type $projectType"
    dotnet new $projectType -n $projectName

    Write-Host "Adding project $projectName to $solutionName solution"
    dotnet sln add $projectName/$projectName.csproj
}

Write-Host "Configuration refereces between projects"
$references = @(
    @{ From = "$solutionName.Application"; To = "$solutionName.Domain" },
    @{ From = "$solutionName.Infrastructure"; To = "$solutionName.Domain" },
    @{ From = "$solutionName.Web"; To = "$solutionName.Application" },
    @{ From = "$solutionName.Web"; To = "$solutionName.Infrastructure" },
    @{ From = "$solutionName.Tests"; To = "$solutionName.Application" }
)

foreach ($reference in $references) {
    $fromProject = $reference.From
    $toProject = $reference.To

    Write-Host "Adding reference: $fromProject -> $toProject..."
    dotnet add "$fromProject/$fromProject.csproj" reference "$toProject/$toProject.csproj"
}

Write-Host "All projects and references configured successfully!"
