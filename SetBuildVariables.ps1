# This file sets build variables to be used in Azure Pipelines

if ($Env:BUILD_SOURCEBRANCH -match "^refs\/tags\/v(\d[\.0-9]*)$") 
{
    $fileVersion = $matches[1]
    $packageVersion = $fileVersion
    # Internal deploy until tried in production
    $pushToNugetOrg = 'false'
}
elseif ($Env:BUILD_BUILDNUMBER -match "^\d+.(\d+)$") 
{
    $date = get-date
    $major = $date.Year
    $minor = $date.Month
    $build = $date.Day
    $revision = $matches[1]
    $fileVersion = "$major.$minor.$build.$revision"

	if ($Env:BUILD_SOURCEBRANCH -match "^refs\/heads\/(?:features\/)?(.*)$")
	{
		$branchName = $matches[1]
		$packageVersion = "$fileVersion-$branchName"
	}
	else
	{
		$packageVersion = "$fileVersion-ci"
	}
    $pushToNugetOrg = 'false'
}

Write-Output "Setting build variables..."
Write-Output "fileVersion=$fileVersion"
Write-Output "packageVersion=$packageVersion"
Write-Output "pushToNugetOrg=$pushToNugetOrg"
Write-Output "##vso[task.setvariable variable=fileVersion]$fileVersion"
Write-Output "##vso[task.setvariable variable=packageVersion]$packageVersion"
Write-Output "##vso[task.setvariable variable=pushToNugetOrg]$pushToNugetOrg"