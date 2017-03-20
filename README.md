<div><h1 style="display:inline"><img src="./MagnumOpus/Assets/Icon-64.png" alt="Wrench icon" style="height: .7em"/> Magnum Opus</h1></div>

Magnum Opus is a support tool specifically developed for use at Sykehuspartner HF. It covers a wide range of different tasks which are common at the Service Desk.

## Setup

### Executables

Magnum Opus is dependent on three exe files for some of the features. These files are as follows:

* [LockoutStatus.exe](https://www.microsoft.com/en-us/download/details.aspx?id=15201)
* [PsExec and PSLoggedOn](https://technet.microsoft.com/en-us/sysinternals/pstools.aspx)

Add these files to the `Executables/Files` folder. These files are bundled in the built exe file to ensure they're available when running the application

### Environment Variables

Some variables are specific to your environment. These are set by adding a file named `EnvironmentDefaults.json` to the root of the project. It has to match the `EnvironmentDefaults` class located in `SettingsService.Environment.cs`. The structure should match the following (without the C-style comments):

``` json
{
  "SplunkUrl": "https://splunk.internally.com/search?msad-account={0}", // Insert {0} where the username should be inserted in the url
  "SCCMPath": "C:\\Program Files\\SCCM Tools\\SCCM Client Center\\SMSCliCtrV2.exe", // Path to SCCM client (https://sccmclictr.codeplex.com/)
  "LogDirectoryPath": "C:\\Logs\\Magnum Opus", // Where to put log files from the application
  "ComputerCompanyOus": [ // The key should be a part of the DistinguishedName of the DirectoryEntry which only occurs for each specific company, e.g.: "OU=CompanyName". The value is the name of the company, e.g.: "Company Name".
    {
      "Key": "OU=ALPHASOFT",
      "Value": "Alpha Software Industries"
    },
    {
      "Key": "OU=BETAINC",
      "Value": "Beta Incorporated"
    }
  ]
}
```

### Icon

[![Wrench icon](./MagnumOpus/Assets/Icon-128.png)  
Support by Icons8](https://icons8.com/web-app/21107/support)