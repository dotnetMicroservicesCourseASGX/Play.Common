# Play.Common
Common libraries used by Play Economy services

## Create and publish package
```powershell
$owner="dotnetMicroservicesCourseASGX"
$gh_pat="[PAT HERE]"
$version="1.0.11"

dotnet pack src\Play.Common\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Common -o ..\packages

dotnet nuget push ..\packages\Play.Common.$version.nupkg --api-key $gh_pat --source "github"
```
