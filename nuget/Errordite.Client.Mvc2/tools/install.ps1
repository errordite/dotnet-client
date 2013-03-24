param($installPath, $toolsPath, $package, $project)

$configFilePath = Join-Path $project.FullName "..\web.config"
$configFile = [xml](Get-Content($configFilePath))

$errorditeNodes = $configFile.SelectNodes("/configuration/errordite")

if ($errorditeNodes.Count -gt 1){
	#because the xml transform adds an errordite config section with a templated token, it will get added again if the settings
	#have changed.  In which case we just whip it out.
	Write-Host "{0} Errordite nodes found - deleting all but first" -f $errorditeNodes.Count
	$first = $TRUE
	foreach($e in $errorditeNodes){
		if (!$first){
			$e.ParentNode.RemoveChild($e)			
		}
		$first = $FALSE
	}
	$configFile.Save($configFilePath)
}
