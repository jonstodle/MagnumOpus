<div><h1 style="display:inline"><img src="./MagnumOpus/Assets/Icon-64.png" alt="Wrench icon" style="height: .7em"/> Magnum Opus</h1></div>

Magnum Opus is a support tool specifically developed for use at Sykehuspartner HF. It covers a wide range of different tasks which are common at the Service Desk.

## Setup

### Executables

Magnum Opus is dependent on three exe files for some of the features. These files are as follows:

* [LockoutStatus.exe](https://www.microsoft.com/en-us/download/details.aspx?id=15201)
* [PSLoggedOn](https://technet.microsoft.com/en-us/sysinternals/pstools.aspx)
* rc - Often bundled with SCCM
* CmRcViewer - Often bundled with newer versions of SCCM (2012+)

Add these files to the `Executables/Files` folder. These files are bundled in the built exe file to ensure they're available when running the application

### Environment Variables

Some variables are specific to your environment. These are set by adding a file named `EnvironmentDefaults.json` to the root of the project. It has to match the `EnvironmentDefaults` class located in `SettingsService.Environment.cs`. The structure should match the following (without the C-style comments):

``` json
{
  "SplunkUrl": "https://splunk.internally.com/search?q=%60lockouts-for-user({0}%2C{1})%60", // Insert {0} where the domain should be inserted and {1} where the username should be inserted in the url
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

[![Icons8 icon](./MagnumOpus/Assets/Icons8.png)  
Icons by Icons8](https://icons8.com/c/auM6/Magnum%20Opus)