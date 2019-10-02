Param (
	[string]$NewName
)

if (!$NewName){
	Write-Host "NewName parameter should be specified: rename.ps1 -NewName YOUR_NAME"
	exit
}

$OldName = "MobileTemplate"
	
Write-Host "NewName = $NewName"

Write-Host "Updating files content"
$exclude = @("*jpg*", "*png*", "*ps1*")
$files = Get-ChildItem . *.* -recurse -exclude $exclude | where { !$_.PSisContainer }

ForEach ($file in $files){
	$content = Get-Content $($file.FullName) -Raw
	$content -replace $OldName,$NewName | Set-Content $file 
}

Write-Host "Updating file names"

Function RenameFile($fileName)
{
	if ($fileName){
		$file = Get-Item $fileName
		if ($file -and $file.Name.Contains($OldName)){
			$newFileName = $file.Name -replace $OldName,$NewName
			Rename-Item $file -NewName $newFileName
		}
	}
}

Function RenameFilesInDir($dirName)
{	
	$files = Get-ChildItem $dirName | where { $_.FullName.Contains($OldName)}
	ForEach ($file in $files) 
	{ 
		if ($file.PSisContainer){
			RenameFilesInDir $file.FullName
		}

		RenameFile $file.FullName		
	}
} 

RenameFilesInDir .

Write-Host "Done"