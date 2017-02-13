# Magnum Opus (Support Tool)

Magnum Opus is a support tool specifically developed for use at Sykehuspartner HF. It covers a wide range of different tasks which are common at the Service Desk.

## Setup

Some variables are specific to your environment. These can be set by adding a file named `SettingsService.Environment.cs` to the `SettingsService` folder and adding the following code:
```
using System.Collections.Generic;

namespace SupportTool.Services.SettingsServices
{
    public partial class SettingsService
    {
        // Variables specific to the current environment

        public string SplunkUrl // Insert {0} where the username should be inserted in the url
        {
            get => Get("");
            set => Set(value);
        }

        public IEnumerable<KeyValuePair<string, string>> ComputerCompanyOus // The key should be a part of the DistinguishedName of the DirectoryEntry which only occurs for each specific company, e.g.: "OU=CompanyName". The value is the name of the company, e.g.: "Company Name".
        {
            get => Get(new List<KeyValuePair<string, string>>());
            set => Set(value);
        }
    }
}
```