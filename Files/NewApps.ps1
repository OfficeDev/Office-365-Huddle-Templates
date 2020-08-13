param(
    [Parameter(Mandatory)]
    [String]$ResourceGroupSuffix
)

#define all apps names
$botAppName = "Huddle Bot";
$botWebAppName = "Huddle Bot Web App";
$metricWebAppName = "Huddle Metric Web App";
$graphConnectorAppName = "Huddle MS Graph Connector App";
$allAppsName = @($botAppName, $botWebAppName, $metricWebAppName, $graphConnectorAppName);

function Get-UniqueString ([string]$id, $length=13)
{
$hashArray = (new-object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
-join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
}


$PasswordCredential = @{PasswordCredential=@{DisplayName="devKey"}}

$org = Get-MgOrganization;

#remove exist apps
foreach ($appName in $allAppsName) {
    $apps = Get-MgApplication -Filter "displayName eq `'$appName`'";
    foreach ($app in $apps) {
        if ($app)
        {
            Remove-MgApplication -ApplicationId $app.Id;
            Write-Host "Remove app $appName"
        }
    }
}

#create bot app
Write-Host "Creating $botAppName"
$botApp = New-MgApplication -DisplayName $botAppName -SignInAudience "AzureADandPersonalMicrosoftAccount" -ImplicitGrantSettingEnableIdTokenIssuance
#Generate ClientId and ClientKey
$botAppPassword = Add-MgApplicationPassword -ApplicationId $botApp.Id -BodyParameter $PasswordCredential

#create bot web app
Write-Host "Creating $botWebAppName"
#generate RequiredResourceAccess attribute
$botWebAppResourceAccess = @(
    @{ResourceAppId="00000003-0000-0000-c000-000000000000"; ResourceAccess=@(
        @{Id="37f7f235-527c-4136-accd-4a02d197296e";Type="Scope"},
        @{Id="14dad69e-099b-42c9-810b-d002981feec1";Type="Scope"},
        @{Id="4e46008b-f24c-477d-8fff-7bb4ec7aafe0";Type="Scope"},
        @{Id="205e70e5-aba6-4c52-a976-6d2d46c48043";Type="Scope"},
        @{Id="485be79e-c497-4b35-9400-0e3fa7f2a5d4";Type="Scope"},
        @{Id="b340eb25-3456-403f-be2f-af7a0d370277";Type="Scope"}
    )}
);
$botWebRedirectUrl = "https://token.botframework.com/.auth/web/redirect"
$botWebApp = New-MgApplication -DisplayName $botWebAppName -RequiredResourceAccess $botWebAppResourceAccess -SignInAudience "AzureADandPersonalMicrosoftAccount" -WebRedirectUris $botWebRedirectUrl -ImplicitGrantSettingEnableIdTokenIssuance;
#Generate ClientId and ClientKey
$botWebAppPassword = Add-MgApplicationPassword -ApplicationId $botWebApp.Id -BodyParameter $PasswordCredential

#create metric web app
#$metricWebAppName = "Huddle Metric Web App Chad Dev";
Write-Host "Create $metricWebAppName"
#generate RequiredResourceAccess attribute
$metricWebAppResourceAccess = @(
    @{ResourceAppId="00000003-0000-0000-c000-000000000000"; ResourceAccess=@(@{Id="204e0828-b5ca-4ad8-b9f3-f32a958e7cc4";Type="Scope"},@{Id="4e46008b-f24c-477d-8fff-7bb4ec7aafe0";Type="Scope"},@{Id="5f8c59db-677d-491f-a6b8-5f174b11ec1d";Type="Scope"})},
    @{ResourceAppId="00000003-0000-0ff1-ce00-000000000000"; ResourceAccess=@(@{Id="9bff6588-13f2-4c48-bbf2-ddab62256b36";Type="Role"})},
    @{ResourceAppId="00000002-0000-0000-c000-000000000000"; ResourceAccess=@(@{Id="311a71cc-e848-46a1-bdf8-97ff7156d8e6";Type="Scope"},@{Id="5778995a-e1bf-45b8-affa-663a9f3f4d04";Type="Scope"})}
);

#new cer
$cert = New-SelfSignedCertificate -Type Custom -KeyExportPolicy Exportable -KeySpec Signature -Subject "CN=Huddle App-only Cert" -NotAfter (Get-Date).AddYears(20) -CertStoreLocation "cert:\CurrentUser\My" -KeyLength 2048
$keyCredential = @{}
$keyCredential.customKeyIdentifier = $cert.GetCertHash()
$keyCredential.keyId = [System.Guid]::NewGuid()
$keyCredential.type = "AsymmetricX509Cert"
$keyCredential.usage = "Verify"
$keyCredential.key = $cert.GetRawCertData()


$guid = (New-Guid).ToString();
$bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $guid)
$certBase64 = [System.Convert]::ToBase64String($bytes)
$certBase64|Out-File .\cert.txt

$metricWebRedirectUrl = "https://huddle-metric-$ResourceGroupSuffix.azurewebsites.net/"
$metricWebApp = New-MgApplication -DisplayName $metricWebAppName -RequiredResourceAccess $metricWebAppResourceAccess -SignInAudience "AzureADMyOrg" -WebRedirectUris $metricWebRedirectUrl -KeyCredentials @($keyCredential) -ImplicitGrantSettingEnableIdTokenIssuance
#Generate ClientId and ClientKey
$metricWebAppPassword = Add-MgApplicationPassword -ApplicationId $metricWebApp.Id -BodyParameter $PasswordCredential

#create metric web app
Write-Host "Create $graphConnectorAppName"
#generate RequiredResourceAccess attribute
$graphConnectorAppResourceAccess = @(
    @{ResourceAppId="00000003-0000-0000-c000-000000000000"; ResourceAccess=@(@{Id="a154be20-db9c-4678-8ab7-66f6cc099a59";Type="Scope"},@{Id="4e46008b-f24c-477d-8fff-7bb4ec7aafe0";Type="Scope"},@{Id="e1fe6dd8-ba31-4d61-89e7-88639da4683d";Type="Scope"})}
);
$grpahConnectorWebRedirectUrl = "https://logic-apis-westus.consent.azure-apim.net/redirect"
$graphConnectorApp = New-MgApplication -DisplayName $graphConnectorAppName -RequiredResourceAccess $graphConnectorAppResourceAccess -SignInAudience "AzureADMyOrg" -WebRedirectUris $grpahConnectorWebRedirectUrl -ImplicitGrantSettingEnableIdTokenIssuance
#Generate ClientId and ClientKey
$graphConnectorAppPassword = Add-MgApplicationPassword -ApplicationId $graphConnectorApp.Id -BodyParameter $PasswordCredential

Write-Host "Tenant Id: $($org.Id)" -ForegroundColor Green

Write-Host "Microsoft App Id: $($botApp.AppId)" -ForegroundColor Green
Write-Host "Microsoft App Password: $($botAppPassword.SecretText)" -ForegroundColor Green

Write-Host "Bot Client Id: $($botWebApp.AppId)" -ForegroundColor Green
Write-Host "Bot Client Secret: $($botWebAppPassword.SecretText)" -ForegroundColor Green

Write-Host "Metric Client Id: $($metricWebApp.AppId)" -ForegroundColor Green
Write-Host "Metric Client Secret: $($metricWebAppPassword.SecretText)" -ForegroundColor Green

Write-Host "Graph Client Id: $($graphConnectorApp.AppId)" -ForegroundColor Green
Write-Host "Graph Client Secret: $($graphConnectorAppPassword.SecretText)" -ForegroundColor Green

Write-Host "Certificate Pfx Base64: $certBase64" -ForegroundColor Green
Write-Host "Certificate Pfx Password: $guid" -ForegroundColor Green


