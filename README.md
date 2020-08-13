---
page_type: sample
products:
- office
- office-teams
- office-365
languages:
- csharp
- javascript
- typescript
- html
description: "Huddle developer templates built on the Microsoft Teams platform help drive quality and care outcomes by enabling collaboration."
urlFragment: ms-teams-huddle
extensions:
  contentType: samples
  createdDate: "2/22/2018 10:58:13 AM"
---

# Huddle

Improving quality of care depends on many things – process, patient care, and research planning. Huddle developer templates built on the Microsoft Teams platform, help drive quality and care outcomes by enabling collaboration with more effective huddle teams. With the huddle sample solutions, you can: 
* Measure and visualize impactful best practices across your organization. 
* Identify patient care issues and potential causes. 
* Share ideas with your health team using natural conversations. 

**Table of content**

[Foreword](#foreword)

[Enable and Create Microsoft Teams](#enable-and-create-microsoft-teams)

* [Enable Microsoft Teams feature](#enable-microsoft-teams-feature)
* [Create Teams](#create-teams)
* [Update Each Team](#update-each-team)

[Import and publish LUIS App](#import-and-publish-luis-app)

[Create SharePoint Site and Lists](#create-sharepoint-site-and-lists)

* [Create a site collection](#create-a-site-collection)
* [Provision Lists](#provision-lists)

[Create App Registrations in AAD](#create-app-registrations-in-aad)
* [Register App in AAD](#register-app-in-aad)
* [Consent Permissions](#consent-permissions)

[Deploy Azure Components with ARM Template](#deploy-azure-components-with-arm-template)

* [GitHub Authorize](#github-authorize)
* [Deploy Azure Components](#deploy-azure-components)
* [Handle Errors](#handle-errors)

[Follow-up Steps](#follow-up-steps)

* [Customize and Configure the Bot](#customize-and-configure-the-bot)
* [Authorize Planner API Connection](#authorize-planner-api-connection)
* [Authorize Teams API Connection](#authorize-teams-api-connection)
* [Authorize Microsoft Graph API Connection](#authorize-microsoft-graph-api-connection)

[Configure Teams App](#configure-teams-app)

* [Create Teams App Package and Side-load It](#create-teams-app-package-and-side-load-it)
* [Add Metric Input Tab](#add-metric-input-tab)
* [Add Idea Board Tab](#add-idea-board-tab)
* [Start Conversation with The Bot](#start-conversation-with-the-bot)

## Foreword

This document will guide you to deploy the solution to your environment.

First, an Azure AAD is required to register the app registrations. In this document, the Azure AAD will be called "Huddle AAD", and an account in Huddle AAD will be called Huddle work account.

* All app registrations should be created in the Huddle AAD. 
* Bot/Luis/Microsoft App should be registered with a Huddle work account. This account should have permission to deploy resource and create resource group in Azure.

* SharePoint lists should be created on SharePoint associating with Huddle AAD.

* Following Powershell Modules are installed
   * [MicrosoftTeams](https://www.powershellgallery.com/packages/MicrosoftTeams)
   * [ImportExcel](https://github.com/dfinke/ImportExcel)
   * [Microsoft.Graph](https://github.com/microsoftgraph/msgraph-sdk-powershell)
   * [SharePointPnPPowerShellOnline](https://docs.microsoft.com/en-us/powershell/sharepoint/sharepoint-pnp/sharepoint-pnp-cmdlets?view=sharepoint-ps)
   * [Azure](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)

   > Notes: 
   >
   > Run PowerShell as Administrator, and execute the commands below to install required modules:
   > ```powershell
   > Install-Module MicrosoftTeams
   > Install-Module ImportExcel
   > Install-module Microsoft.Graph
   > Install-Module SharePointPnPPowerShellOnline
   > Install-Module Az
   > ```
   >

* Required [NodeJS](https://nodejs.org/en/) on your local environment. Node.js >= 8.5 and npm installed on your machine, then use:
```cmd
npm install -g luis-apis
```

An Azure Subscription is required to deploy the Azure components. We will use the [ARM Template](azuredeploy.json) to deploy these Azure components automatically. 

Please download files in `/Files` folder to your computer.

To learn more about the huddle solutions in Microsoft Teams and Microsoft O365, visit the [Microsoft developer blog](https://dev.office.com/blogs/new-templates-to-create-huddle-solutions-in-microsoft-teams-and-office-365).

## Enable and Create Microsoft Teams

### Enable Microsoft Teams feature

Please follow [Teams org-wide settings in the Microsoft Teams admin center](https://docs.microsoft.com/en-us/microsoftteams/enable-features-office-365#teams-org-wide-settings-in-the-microsoft-teams-admin-center).

Make sure the following options are turned on:

* Allow third-party apps

* Allow interaction with custom apps

  ![](Images/ms-teams-configure.png)

### Create Teams

In this section, we will connect to Microsoft Teams in PowerShell with a Huddle AAD account, and execute some PowerShell scripts to create teams from an Excel file.

> Note: after you finish this section, teams will be created right away. But their owners and members will take up to an hour to show in Teams. Refer to [Add-TeamUser](https://docs.microsoft.com/en-us/powershell/module/teams/add-teamuser?view=teams-ps) for more details.

1. First, let open and edit `/Files/Teams.xlsx`. Input the teams and related information.

   > Note: 
   >
   > * AccessType:
   >   * Private: Private teams can only be joined if the team owner adds you to them. They also won't show up in your teams gallery.
   >   * Public: public teams are visible to everyone from the teams gallery and you can join them without getting approval from the team owner.
   > * Owners and Members:
   >   * Please use UPN (User Principle Name) instead of email.
   >   * Use ";" to separate multi-users. 
   >   * The Huddle work account used to connect to Microsoft Teams will be added as the owner of each team automatically, no matter it is in the owners column or not.

2. Open PowerShell Console

3. Navigate to the `/Files` folder in PowerShell

   ```powershell
   cd <Path to Files folder> # For example: cd "c:\Users\Admin\Desktop\Huddle\Files\"
   ```

4. Connect to Microsoft Teams with a Huddle AAD account.

   ```
   Connect-MicrosoftTeams
   ```

5. Execute the commands below which reads data from the Excel file and create teams:

   ```powershell
   .\NewTeams.ps1 -excelPath .\Teams.xlsx
   ```

### Update Each Team

For each team you created, please active the default planer and create 4 buckets:

1. Open <https://www.office.com>, sign in.

   ![](Images/ms-planner-01.png)

   Click **Planner**.

   Click **New plan** and then click **Add to an existing Office 365 Group**. Create a plan with the same name in the previously created group.

   ![](Images/ms-planner-02.png)


2. Create the following buckets:
   * New Idea
   * In Progress
   * Completed
   * Shareable

## Import and publish LUIS App

1. Open PowerShell Console and navigate to the `/Files` folder in PowerShell

2. Connect to Microsoft Azure with a Huddle work account.

   ```PowerShell
   Connect-AzAccount
   ```
3. Run the following script in the PowerShell console. This script will create a resource group in Azure, then import, train, and publish LUIS App. Replace \<resource group name\> with the resource group name you expect. If the execution is successful, LUIS App Id and ResourceGroup Suffix will be returned. Remember these two values

   ```PowerShell
   #Replace <resource group name> with the resource group name you expect.
   .\DeployLuis.ps1 -appPath .\LUISApp.json -resourceTemplate .\LUISTemplate.json -resourceGroup <resource group name>
   ```

## Create SharePoint Site and Lists

### Create a site collection

1. Open a web browser and go to SharePoint Administration Center.

   `https://<YourTenant>-admin.sharepoint.com/_layouts/15/online/SiteCollections.aspx`

2. Click **New** -> **Private Site Collection**.

   ![](Images/sp-01.png) 

3. Fill in the form:

   ![](Images/sp-02.png)

   * In the **Title** field, enter site title. 
   * In the **Web Site Address** field, enter hospital site URL.
   * **Select a language**: English
   * In the **Template Selection** section, select **Team Site** as **site template.**
   * Choose a  **Time Zone**.
   * **Administrator** should be the alias of the individual you want to have full administrator rights on this site. 
   * Leave **Server Resource Quota** at 300. (This value can be adjusted later if needed)

4. Click **OK**.

5. When the site is created, use Huddle AAD account to log in to the site just created.

6. Click the gear in the upper right corner of the page, **Shared with...**, **Invite people**. Add the users you want to use. And click **Save**

   ![](Images/sp-04.png)

7. Copy aside the URL of the site collection. It will be used as the value of **Base SP Site Url** parameter of the ARM Template.

###  Provision Lists

1. Install SharePointPnPPowerShellOnline module, if you have not installed it. 

   Please follow: <https://msdn.microsoft.com/en-us/pnp_powershell/pnp-powershell-overview#installation>

2. Open Power Shell, then execute the command below to connect to the site you just created:

   ```powershell
   Connect-PnPOnline -Url https://<Tenant>.sharepoint.com/sites/<Site> -Credentials (Get-Credential)
   ```

   > Note: Please replace `<Tenant>` and `<Site>`.

3. Login in with an admin account.

   ![](Images/sp-03.png)

4. Navigate to `/Files` folder in PowerShell, then execute the following command:

   ```powershell
   Apply-PnPProvisioningTemplate -Path PnPProvisioningTemplate.xml
   ```

   > Notes: The Following sample data was created in the Categories list
   >   * Safety/Quality
   >   * Access
   >   * Experience
   >   * Finance
   >   * People

## Create App Registrations in AAD 

### Register App in AAD
1. Open PowerShell Console and navigate to the `/Files` folder in PowerShell

2. Connect to Microsoft Azure with a Huddle AAD account.

   ```PowerShell
   Connect-Graph -Scopes Directory.AccessAsUser.All
   ```
3. Run the following script in the PowerShell console. This script will create the following 4 Applications in AAD. The names of these 4 Applications are defined at the top of [NewApps.ps1](./Files/NewApps.ps1).
   * Huddle Bot
   * Huddle Bot Web App
   * Huddle Metric Web App
   * Huddle MS Graph Connector App

   ```PowerShell
   .\NewApps.ps1 -ResourceGroupSuffix <your resourceGroupSuffix value>
   ```
   > Notes:
   > The resourceGroupSuffix value is generated in **Import and publish LUIS App** section

4. After the script runs successfully, it will return the following data, remember these data
   * Tenant Id
   * Microsoft App Id
   * Microsoft App Password
   * Bot Client Id
   * Bot Client Secret
   * Metric Client Id
   * Metric Client Secret
   * Graph Client Id
   * Graph Client Secret
   * Certificate Pfx Base64
   * Certificate Pfx Password

### Consent Permissions
1. Log in to [Azure Portal](https://portal.azure.com) with Huddle AAD account. 

2. Find **Huddle Bot Web App** just created in App registrations.

3. In **API Permission** tab, consent the permission

   ![](./Images/app-registration-consent.png)

4. Follow step 2 and step 3, consent the permission for **Huddle Metric Web App** and **Huddle MS Graph Connector App**

## Deploy Azure Components with ARM Template

### GitHub Authorize

1. Generate Token

   - Open [https://github.com/settings/tokens](https://github.com/settings/tokens) in your web browser.

   - Sign into your GitHub account where you forked this repository.

   - Click **Generate new token**.

   - Enter a value in the **Token description** text box.

   - Select the following s (your selections should match the screenshot below):

     - repo (all) -> repo:status, repo_deployment, public_repo, repo:invite, security_events
     - admin:repo_hook -> read:repo_hook

     ![](Images/github-new-personal-access-token.png)

   - Click **Generate token**.

   - Copy the token.

2. Add the GitHub Token to Azure in the Azure Resource Explorer

   - Open [https://resources.azure.com/providers/Microsoft.Web/sourcecontrols/GitHub](https://resources.azure.com/providers/Microsoft.Web/sourcecontrols/GitHub) in your web browser.

   - Log in with your Azure account.

   - Selected the correct Azure subscription.

   - Select **Read/Write** mode.

   - Click **Edit**.

   - Paste the token into the **token parameter**.

     ![](Images/update-github-token-in-azure-resource-explorer.png)

   - Click **PUT**.

### Deploy Azure Components

1. Fork this repository to your GitHub account.

2. Click the Deploy to Azure Button:

   [![Deploy to Azure](https://camo.githubusercontent.com/9285dd3998997a0835869065bb15e5d500475034/687474703a2f2f617a7572656465706c6f792e6e65742f6465706c6f79627574746f6e2e706e67)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FOfficeDev%2FOffice-365-Huddle-Templates%2Fmaster%2Fazuredeploy.json)

3. Fill in the values on the deployment page:

   ![](Images/azure-deploy.png)

   You have collected most of the values in previous steps. For the rest parameters:

   * **Resource group**: select to created resource group in previous step. 
   * **Bot Name**: the name of the bot, will be used as Display Name of Bot Registration.
   * **Global Team**: the name of the global team that is one of the Teams created earlier. A webjob will move shared tasks in other team planners to the shared bucket of Global Team
   * **Source Code Repository**:  use the URL of the repository you just created -`https://github.com/<YourAccount>/Office-365-Huddle-Templates`
   * **Source Code Branch**: master
   * **Source code Manual Integration**: false
   * Check **I agree to the terms and conditions stated above**.

    > **Tips:**
    >
    > You can click **Edit Parameters** on top of the template after filled all parameters.
    >
    > ![](Images/azure-deploy-edit-parameters.png)
    >
    > Then click **Download** to save parameters on your local computer in case of deployment failure.
    >
    > ![](Images/azure-deploy-download.png)

4. Click **Purchase**.

### Handle Errors

If the deployment started, but failed as below - one or two errors of sourcecontrols resources:

![](Images/azure-deploy-error.png)

Please **Redeploy** with the same parameters and to the same resource group.

## Follow-up Steps

### Customize and Configure the Bot

1. On Azure Portal, find the Resource Group you just created. Navigate to the Bot Channels Registration you created.

   ![](Images/bot-27.png)

2. Upload an icon:

   * Click **Settings**.

     ![](Images/bot-31.png) 

   * Upload `/Files/HuddleBotIcon.png` as the **Icon**. 

   * Click **Save**.

### Authorize Planner API Connection

1. Navigate to **planner** in the resource group you just created.

   ![](Images/planner-api-connection-01.png)

2. Click the **planner** API Connection.

   ![](Images/planner-api-connection-02.png)

3. Click **Edit API connection**.

   ![](Images/planner-api-connection-03.png)


4. Click **Authorize**.

   Pick up or input the Huddle AAD account. The user account should be in every team.

   Sign in the account.

5. Click **Save** at the bottom.

### Authorize Teams API Connection

Follow the similar steps in the previous chapter to authorize the **teams** API Connection.

![](Images/teams-api-connection.png)

### Authorize Microsoft Graph API Connection

Follow the similar steps in the previous chapter to authorize the **microsoft-graph** API. 

![](Images/ms-graph-connection.png)

### Add Prediction Resource in LUIS App
1. Open [LUIS](https://luis.ai) site and log in with Huddle work account

![](Images/luis-config-01.png)

2. Navigate to **My apps** tab

3. Select the Subscription and Resource deployed in the previous step (the suffix of this resource has the same suffix as the luis app deployed in the previous step)

![](Images/luis-config-02.png)

4. Click on the listed app

5. Select **MANAGE** tab on the top

![](Images/luis-config-03.png)

6. Select **Azure Resources** tab on the left

![](Images/luis-config-04.png)

7. Click **Add prediction resource** button

![](Images/luis-config-05.png)

8. Select the Subscription and Resource deployed in the previous step (the suffix of this resource has the same suffix as the luis app deployed in the previous step)

![](Images/luis-config-06.png)

9. Click **Done** button to save changes.

## Configure Teams App

### Create Teams App Package and Side-load It

1. Open `/Files/TeamsAppPackage/manifest.json` with a text editor.

2. Replace the following 2 placeholders with the corresponding values you got in previous guides:

   * `<MicrosoftAppId>`: Microsoft App Id generated in previous step.
   
   * `<MetricWebAppDomain>`: the domain of the Metric Web App

     ![](Images/ms-teams-02.png)

3. Save the changes.

4. Zip the files in `/Files/TeamsAppPackage` folder.

   ![](Images/ms-teams-03.png)

   Name it HuddleTeamsApp.zip.

5. Right-click a team in Microsoft Teams, then click Manage team.

   ![](Images/ms-teams-04.png)

6. Click the **Apps** tab.

   ![](Images/ms-teams-05.png)

7. Then click **Upload a custom app**.

8. Select the *HuddleTeamsApp.zip*.

9. Click **Add** button.

   ![](Images/ms-teams-15.png)

### Add Metric Input Tab

1. Click a team.

   ![](Images/ms-teams-06.png)        

2. Click **+**

   ![](Images/ms-teams-07.png)

3. Click **Huddle App**.

   ![](Images/ms-teams-08.png)

4. Click **Save**.

### Add Idea Board Tab

1. Click a team.

   ![](Images/ms-teams-06.png)        

2. Click **+**

   ![](Images/ms-teams-11.png)

3. Click **Planner**.

4. Choose **Use an existing plan**, then select the plan which has the same name as the team.

   ![](Images/ms-teams-12.png)

5. Click **Save**.

   ![](Images/ms-teams-13.png)

6. Click the dropdown icon, then click **Rename**. 

   ![](Images/ms-teams-14.png)

   Input: Idea Board

7. Click **Save**. 

### Start Conversation with The Bot

Follow the step below to start 1:1 conversation with the Bot in Microsoft Teams

1. Find the URL of Microsoft Teams Channel of the Bot, 

   ![](Images/bot-16.png)

   Then open it in your browser:

   ![](Images/bot-22.png)

2. Click **Open Microsoft Teams**.

Another way to start 1:1 talk is using the **MicrosoftAppId** of the Bot:

![](Images/bot-23.png)



**Copyright (c) 2018 Microsoft. All rights reserved.**
