param(
    [Parameter(Mandatory)]
    [String]$excelPath
)

$currentPath = Get-Location
Import-Module MicrosoftTeams
function Coalesce($a, $b) { 
    if ($a -ne $null) { $a } else { $b } 
}

Write-Host $currentPath\$excelPath

$index = 0;
$splitOption = [System.StringSplitOptions]::RemoveEmptyEntries
$teams = Import-Excel -Path "$currentPath\$excelPath" -DataOnly
$count = Coalesce $teams.Count 1

Foreach($team in $teams) {
    $accessType = Coalesce $team.AccessType "Private"
    $owners = (Coalesce $team.Owners "").Split(';', $splitOption)
    $members = (Coalesce $team.Members "").Split(';', $splitOption)

    Write-Progress -Activity "Creating Teams" -Status 'Progress->' -PercentComplete ($index * 100 / $count) -CurrentOperation ("Creating Team " + $team.Name)
    $t = New-Team -DisplayName $team.Name -Visibility $accessType

    Write-Progress -Activity "Creating Teams" -Status 'Progress->' -PercentComplete (($index + 0.5) * 100 / $count) -CurrentOperation ("Adding owners and members to " + $team.Name)
    Foreach ($owner in $owners) {
        if ($owner -eq $connection.Account.Id) { continue }
        Try {
            Add-TeamUser -GroupId $t.GroupId -User $owner -Role Owner
        }
        Catch {
            $ErrorMessage = $_.Exception.Message
            Write-Host "Could not add $owner to $($team.Name) as owner: $ErrorMessage"
        }
    }

    Foreach ($member in $members) {
        if ($member -eq $connection.Account.Id) { continue }
        Try {
            Add-TeamUser -GroupId $t.GroupId -User $member -Role Member
        }
        Catch {
            $ErrorMessage = $_.Exception.Message
            Write-Host "Could not add $member to $($team.Name) as member: $ErrorMessage"
        }
    }
    $index++
}