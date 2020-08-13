param(
    [Parameter(Mandatory)]
    [String]$resourceGroupName,
    [Parameter(Mandatory)]
    [String]$appPath,
    [Parameter(Mandatory)]
    [String]$resourceTemplate
)

#Connect Azure
#Connect-AzAccount

Write-Host "Creating resource group: $resourceGroupName";
if (Get-AzResourceGroup -Name $resourceGroupName -ErrorAction:SilentlyContinue){
    Write-Host "Resource group $resourceGroupName already exists."
    exit
}
else {
    $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location 'westus'
}

Write-Host "Creating LUIS.Authoring"
#$luis = New-AzCognitiveServicesAccount -ResourceGroupName $resourceGroupName -Name 'huddleLuisAuthoring' -Type 'LUIS.Authoring' -SkuName 'F0' -Location 'westus'
$result = New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $resourceTemplate 
$resourceGroupUnique = $result.Outputs.uniqueString.Value
$luisAuthoringName = $result.Outputs.luisAuthoringName.Value
if ([string]::IsNullOrEmpty($resourceGroupUnique)){
    Write-Host "Create resource failed" -ForegroundColor Red
    exit;
}

$luisKeys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -Name $luisAuthoringName
$appName = "huddle-" + (New-Guid).ToString();
$key = $luisKeys.Key1
$file = $appPath

Write-Host "Creating $appName"
$luisApp = Invoke-Expression -Command "luis import application --appName `"$appName`" --authoringKey `"$key`" --subsciptionKey `"$key`" --region 'westus' --in `"$file`" --wait" | ConvertFrom-Json;

Write-Host "Training $appName"
#$luisTrain = 
Invoke-Expression -Command "luis train version --appId `"$($luisApp.Id)`" --region 'westus' --authoringKey `"$key`" --versionId `"0.1`" --wait"

Write-Host "Publishing $appName"
#$luisPublish =
Invoke-Expression -Command "luis publish version --appId `"$($luisApp.Id)`" --region 'westus' --authoringKey `"$key`" --versionId '0.1' --wait" | ConvertFrom-Json;

Write-Host "LUIS App Id: $($luisApp.Id)" -ForegroundColor Green
#Write-Host "LUIS App Key: $key" -ForegroundColor Green
Write-Host "ResourceGroup Suffix: $resourceGroupUnique" -ForegroundColor Green