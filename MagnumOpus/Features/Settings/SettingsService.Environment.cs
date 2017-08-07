﻿using System.Collections.Generic;

namespace MagnumOpus.Settings
{
    public partial class SettingsService
    {
        // Variables specific to the current environment

        public string SplunkUrl
        {
            get => Get(_defaults.SplunkUrl);
            set => Set(value);
        }

        public string SCCMPath
        {
            get => Get(_defaults.SCCMPath);
            set => Set(value);
        }

        public string LogDirectoryPath
        {
            get => Get(_defaults.LogDirectoryPath);
            set => Set(value);
        }

        public IEnumerable<KeyValuePair<string, string>> ComputerCompanyOus
        {
            get => Get(_defaults.ComputerCompanyOus);
            set => Set(value);
        }



        private EnvironmentDefaults _defaults;
    }



    class EnvironmentDefaults
    {
        public EnvironmentDefaults(
            string splunkUrl,
            string sccmPath,
            string logDirectoryPath,
            IEnumerable<KeyValuePair<string, string>> computerCompanyOus)
        {
            SplunkUrl = splunkUrl;
            SCCMPath = sccmPath;
            LogDirectoryPath = logDirectoryPath;
            ComputerCompanyOus = computerCompanyOus;
        }



        public string SplunkUrl { get; private set; }
        public string SCCMPath { get; private set; }
        public string LogDirectoryPath { get; private set; }
        public IEnumerable<KeyValuePair<string,string>> ComputerCompanyOus { get; private set; }
    }
}
