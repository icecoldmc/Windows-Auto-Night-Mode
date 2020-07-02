﻿using Microsoft.Win32;
using System;
using System.Threading;
using AutoDarkModeApp;

namespace AutoDarkModeSvc.Handlers
{
    static class RegistryHandler
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Switches system applications theme
        /// </summary>
        /// <param name="theme">0 for dark, 1 for light theme</param>
        public static void SetAppsTheme(int theme)
        {
            var key = GetPersonalizeKey();
            key.SetValue("AppsUseLightTheme", theme, RegistryValueKind.DWord);
            key.Dispose();
        }

        /// <summary>
        /// Switches the system theme
        /// </summary>
        /// <param name="theme"><0 for dark, 1 for light theme</param>
        public static void SetSystemTheme(int theme)
        {
            var key = GetPersonalizeKey();
            key.SetValue("SystemUsesLightTheme", theme, RegistryValueKind.DWord);
            key.Dispose();
        }

        public static void SetEdgeTheme(int theme)
        {
            if (theme == (int)Theme.Dark)
            {
                theme = (int)Theme.Light;
            }
            else
            {
                theme = (int)Theme.Dark;
            }
            var edgeKey = GetEdgeKey();
            edgeKey.SetValue("Theme", theme, RegistryValueKind.DWord);
            edgeKey.Dispose();
        }

        public static bool EdgeUsesLightTheme()
        {
            //reverse dark and light for edge because Microsoft
            var key = GetEdgeKey();
            var value = key.GetValue("Theme");
            key.Dispose();
            if ((int)value == (int)Theme.Dark)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the taskbar color prevalence
        /// </summary>
        /// <param name="theme">0 for disabled, 1 for enabled</param>
        public static void SetColorPrevalence(int theme)
        {
            var key = GetPersonalizeKey();
            key.SetValue("ColorPrevalence", theme, RegistryValueKind.DWord);
            key.Dispose();
        }

        /// <summary>
        /// Checks if color prevalence is enabled
        /// </summary>
        /// <returns>true if enabled; false otherwise</returns>
        public static bool IsColorPrevalence()
        {
            var key = GetPersonalizeKey();
            var enabled = key.GetValue("ColorPrevalence").Equals(1);
            key.Dispose();
            return enabled;
        }

        /// <summary>
        /// Checks if system apps theme is light
        /// </summary>
        /// <returns>true if light; false if dark</returns>
        public static bool AppsUseLightTheme()
        {
            var key = GetPersonalizeKey();
            var enabled = key.GetValue("AppsUseLightTheme").Equals(1);
            key.Dispose();
            return enabled;
        }

        /// <summary>
        /// Checks if the system's theme is light
        /// </summary>
        /// <returns>true if light; false if dark</returns>
        public static bool SystemUsesLightTheme()
        {
            var key = GetPersonalizeKey();
            var enabled = key.GetValue("SystemUsesLightTheme").Equals(1);
            key.Dispose();
            return enabled;
        }

        /// <summary>
        /// Retrieves the operating system version
        /// </summary>
        /// <returns>operating system version string</returns>
        public static string GetOSversion()
        {
            var osVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
            return osVersion;
        }

        /// <summary>
        /// Gets the current user's personaliation registry key
        /// </summary>
        /// <returns>HKCU personalization RegistryKey</returns>
        private static RegistryKey GetPersonalizeKey()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
            return registryKey;
        }

        private static RegistryKey GetEdgeKey()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Main", true);
            return registryKey;
        }


        /// <summary>
        /// Adds the application to Windows autostart
        /// </summary>
        public static void AddAutoStart()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey.SetValue("AutoDarkMode", '\u0022' + Extensions.ExecutionPath + '\u0022');
                registryKey.Dispose();
            } catch (Exception ex)
            {
                Logger.Error(ex, "could not add AutoDarkModeSvc to autostart");
            }

        }

        /// <summary>
        /// Removes the application from Windows autostart
        /// </summary>
        public static void RemoveAutoStart()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey.DeleteValue("AutoDarkMode", false);
                registryKey.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "could not remove AutoDarkModeSvc from autostart");
            }

        }
        /// <summary>
        /// Changes the office theme
        /// </summary>
        /// <param name="themeValue">0 = colorful, 3 = grey, 4 = black, 5 = white</param>
        public static void OfficeTheme(byte themeValue)
        {
            string officeCommonKey = @"Software\Microsoft\Office\16.0\Common";

            //edit first registry key
            RegistryKey commonKey = Registry.CurrentUser.OpenSubKey(officeCommonKey, true);
            commonKey.SetValue("UI Theme", themeValue);

            //search for the second key and then change it
            RegistryKey identityKey = Registry.CurrentUser.OpenSubKey(officeCommonKey + @"\Roaming\Identities\", true);

            string msaSubkey = @"\Settings\1186\{00000000-0000-0000-0000-000000000000}\";
            string anonymousSubKey = msaSubkey + @"\PendingChanges";

            foreach (var v in identityKey.GetSubKeyNames())
            {
                //registry key for users logged in with msa
                if (!v.Equals("Anonymous"))
                {
                    try
                    {
                        RegistryKey settingsKey = identityKey.OpenSubKey(v + msaSubkey, true);
                        settingsKey.SetValue("Data", new byte[] { themeValue, 0, 0, 0 });
                    }
                    catch
                    {
                        var createdSettingsKey = identityKey.CreateSubKey(v + msaSubkey, true);
                        createdSettingsKey.SetValue("Data", new byte[] { themeValue, 0, 0, 0 });
                    }
                }
                //registry key for users without msa
                else
                {
                    try
                    {
                        RegistryKey settingsKey = identityKey.OpenSubKey(v + anonymousSubKey, true);
                        settingsKey.SetValue("Data", new byte[] { themeValue, 0, 0, 0 });
                    }
                    catch
                    {
                        var createdSettingsKey = identityKey.CreateSubKey(v + anonymousSubKey, true);
                        createdSettingsKey.SetValue("Data", new byte[] { themeValue, 0, 0, 0 });
                    }
                }
            }
        }
    }
}