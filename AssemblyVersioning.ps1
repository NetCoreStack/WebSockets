param (
	[Parameter(Mandatory=$True)]
	[string]$rootDir,
	[Parameter(Mandatory=$True)]
	[string]$fileNamePattern,
	[Parameter(Mandatory=$True)]
	[string]$major,
	[Parameter(Mandatory=$True)]
	[string]$minor
)

Write-Verbose 'Entering assemblyversioning.ps1'

### First, mount the assembly version ###

#Calculate today's julian date

function Get-JulianDate {
	#Calculate today's julian date
	$Year = get-date -format yy
	$JulianYear = $Year.Substring(1)
	$DayOfYear = (Get-Date).DayofYear
	$JulianDate = $JulianYear + "{0:D3}" -f $DayOfYear
	$JulianDate
	return
}

$build = Get-JulianDate

$buildNumberFromVso = $($env:BUILD_BUILDNUMBER)

$revision = $buildNumberFromVso.Split(".")[1]

# The Assembly Version

$assemblyVersion = "$major.$minor.$build.$revision"

Write-Host "The version this build will generate is $assemblyVersion"

$assemblyVersionString = "AssemblyVersion(""$assemblyVersion"")"
$assemblyFileVersionString = "AssemblyFileVersion(""$assemblyVersion"")"

$assemblyInfoFiles = Get-ChildItem -Path $rootDir -Filter $fileNamePattern -Recurse
$fileCount = $assemblyInfoFiles.Count

Write-Host ""
Write-Host "Started writing the $fileNamePattern files..."
Write-Host ""

foreach($file in $assemblyInfoFiles){
	$fullFilePath = Join-Path $file.Directory $file.Name

	Write-Host "Editing $fullFilePath"

	Add-Content -Path $fullFilePath -Value "`n[assembly: $assemblyVersionString]"
	Add-Content -Path $fullFilePath -Value "[assembly: $assemblyFileVersionString]"
}

Write-Host ""
Write-Host "Finished editing $fileCount AssemblyInfo files."