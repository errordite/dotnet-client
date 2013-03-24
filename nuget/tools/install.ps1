param($installPath, $toolsPath, $package, $project)

foreach($configFileName in "web.config", "app.config")
{
	$configFilePath = Join-Path $project.FullName ("..\" + $configFileName)
	if (!(Test-Path $configFilePath)) {
		continue
	}
	
	$configFile = [xml](Get-Content($configFilePath))

	$errorditeNodes = $configFile.SelectNodes("/configuration/errordite")

	if ($errorditeNodes.Count -gt 1){
		#because the xml transform adds an errordite config section with a templated token, it will get added again if the settings
		#have changed.  In which case we just whip it out.
		Write-Host ("{0} Errordite nodes found - deleting all but first" -f $errorditeNodes.Count)
		$first = $TRUE
		foreach($e in $errorditeNodes){
			if (!$first){
				$e.ParentNode.RemoveChild($e)			
			}
			$first = $FALSE
		}
		$configFile.Save($configFilePath)
	}

    #if there are more than one errordite config sections, remove all but the first one
    $configNode = $configFile.SelectNodes("/configuration/configSections/section[@name='errordite']")

    if($configNode.Count -gt 1){
        Write-Host ("{0} Errordite config nodes found - deleting all but first" -f $configNode.Count)
        $first = $TRUE
        foreach($e in $configNode){
            if (!$first){
				$e.ParentNode.RemoveChild($e)			
			}
			$first = $FALSE
		}
        $configFile.Save($configFilePath)
    }
}


