[CmdletBinding(DefaultParameterSetName = 'None')]
Param()

Begin {
    Write-Verbose "Entering script Run-GitVersion.ps1"
}

Process {
    $parameter1 = """$PWD"""
    $parameter2 = " /output json"

    Write-Verbose "Running GitVersion.exe ..."
    $gitversionoutput = & "$PSScriptRoot\GitVersion.exe" $parameter1 $parameter2
    Write-Verbose "    Done"

    $jsonObj = "$gitversionoutput" | ConvertFrom-Json

    Write-Verbose "Writing variables ..."
    foreach ($property in $jsonObj.PSObject.Properties) {
        Write-Verbose "    GitVersion.$($property.Name): $($property.Value)"
        Write-Output "##vso[task.setvariable variable=GitVersion.$($property.Name);]$($property.Value)"
    }
    Write-Verbose "    Done"
}

End {
    Write-Verbose "Leaving script Run-GitVersion.ps1"
}