﻿#region copyright
//  Copyright (C) 2022 Auto Dark Mode
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion
using AutoDarkModeLib;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System.Power;
using AdmProperties = AutoDarkModeLib.Properties;

namespace AutoDarkModeApp.Pages
{
    /// <summary>
    /// Interaction logic for PageSwitchModes.xaml
    /// </summary>
    public partial class PageSwitchModes : Page
    {
        private readonly bool init = true;
        readonly AdmConfigBuilder builder = AdmConfigBuilder.Instance();

        public PageSwitchModes()
        {
            InitializeComponent();

            //ui init

            //deactivate some incompatible settings
            if (PowerManager.BatteryStatus == BatteryStatus.NotPresent)
            {
                CheckBoxBatteryDarkMode.IsEnabled = false;
            }

            if (builder.Config.GPUMonitoring.Enabled)
            {
                CheckBoxGPUMonitoring.IsChecked = true;
                StackPanelGPUMonitoring.Visibility = Visibility.Visible;
            }
            else
            {
                CheckBoxGPUMonitoring.IsChecked = false;
                StackPanelGPUMonitoring.Visibility = Visibility.Collapsed;
            }

            CheckBoxIdleTimer.IsChecked = builder.Config.IdleChecker.Enabled;
            if (builder.Config.IdleChecker.Enabled)
            {
                StackPanelIdleTimer.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanelIdleTimer.Visibility = Visibility.Collapsed;
            }

            CheckBoxAutoSwitchNotify.IsChecked = builder.Config.AutoSwitchNotify.Enabled;
            if (builder.Config.AutoSwitchNotify.Enabled)
            {
                StackPanelAutoSwitchNotify.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanelAutoSwitchNotify.Visibility = Visibility.Collapsed;
            }
            NumberBoxAutoSwitchNotifyGracePeriod.Value = Convert.ToDouble(builder.Config.AutoSwitchNotify.GracePeriodMinutes);

            ComboBoxGPUSamples.SelectedIndex = builder.Config.GPUMonitoring.Samples - 1;
            NumberBoxGPUThreshold.Value = Convert.ToDouble(builder.Config.GPUMonitoring.Threshold);

            NumberBoxIdleTimer.Value = Convert.ToDouble(builder.Config.IdleChecker.Threshold);

            CheckBoxBatteryDarkMode.IsChecked = builder.Config.Events.DarkThemeOnBattery;

            HotkeyTextboxForceDark.Text = builder.Config.Hotkeys.ForceDark ?? "";
            HotkeyTextboxForceLight.Text = builder.Config.Hotkeys.ForceLight ?? "";
            HotkeyTextboxNoForce.Text = builder.Config.Hotkeys.NoForce ?? "";
            HotkeyTextboxToggleAutomaticThemeSwitch.Text = builder.Config.Hotkeys.ToggleAutoThemeSwitch ?? "";
            HotkeyTextboxToggleTheme.Text = builder.Config.Hotkeys.ToggleTheme ?? "";
            HotkeyTextboxTogglePostpone.Text = builder.Config.Hotkeys.TogglePostpone ?? "";
            HotkeyCheckboxToggleAutomaticThemeSwitchNotification.IsChecked = builder.Config.Notifications.OnAutoThemeSwitching;
            HotkeyCheckboxTogglePostpone.IsChecked = builder.Config.Notifications.OnSkipNextSwitch;


            ToggleHotkeys.IsOn = builder.Config.Hotkeys.Enabled;
            TextBlockHotkeyEditHint.Visibility = ToggleHotkeys.IsOn ? Visibility.Visible : Visibility.Hidden;

            init = false;
        }

        private void CheckBoxBatteryDarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxBatteryDarkMode.IsChecked.Value)
            {
                builder.Config.Events.DarkThemeOnBattery = true;
            }
            else
            {
                builder.Config.Events.DarkThemeOnBattery = false;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "CheckBoxBatteryDarkMode_Click");
            }
        }

        private void CheckBoxIdleTimer_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxIdleTimer.IsChecked.Value)
            {
                StackPanelIdleTimer.Visibility = Visibility.Visible;
                builder.Config.IdleChecker.Enabled = true;
            }
            else
            {
                StackPanelIdleTimer.Visibility = Visibility.Collapsed;
                builder.Config.IdleChecker.Enabled = false;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "CheckBoxIdleTimer_Click");
            }
        }

        private void ShowErrorMessage(Exception ex, string location)
        {
            string error = AdmProperties.Resources.ErrorMessageBox + $"\n\nError ocurred in: {location}" + ex.Source + "\n\n" + ex.Message;
            MsgBox msg = new(error, AdmProperties.Resources.errorOcurredTitle, "error", "yesno")
            {
                Owner = Window.GetWindow(this)
            };
            msg.ShowDialog();
            var result = msg.DialogResult;
            if (result == true)
            {
                string issueUri = @"https://github.com/Armin2208/Windows-Auto-Night-Mode/issues";
                Process.Start(new ProcessStartInfo(issueUri)
                {
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            return;
        }

        private void CheckBoxGPUMonitoring_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxGPUMonitoring.IsChecked.Value)
            {
                builder.Config.GPUMonitoring.Enabled = true;
                StackPanelGPUMonitoring.Visibility = Visibility.Visible;
            }
            else
            {
                builder.Config.GPUMonitoring.Enabled = false;
                StackPanelGPUMonitoring.Visibility = Visibility.Collapsed;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "CheckBoxGPUMonitoring_Click");
            }
        }

        private void NumberBoxGPUThreshold_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (!init)
            {
                if (double.IsNaN(NumberBoxGPUThreshold.Value)) //fixes crash when leaving box empty and clicking outside it
                    return;

                builder.Config.GPUMonitoring.Threshold = Convert.ToInt32(NumberBoxGPUThreshold.Value);

                try
                {
                    builder.Save();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex, "NumberBoxGPUThreshold_ValueChanged");
                }
            }
        }

        private void NumberBoxIdleTimer_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (!init)
            {
                if (double.IsNaN(NumberBoxIdleTimer.Value)) //fixes crash when leaving box empty and clicking outside it
                    return;

                builder.Config.IdleChecker.Threshold = Convert.ToInt32(NumberBoxIdleTimer.Value);
                try
                {
                    builder.Save();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex, "NumberBoxGPUThreshold_ValueChanged");
                }
            }
        }

        private void CheckBoxAutoSwitchNotify_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxAutoSwitchNotify.IsChecked.Value)
            {
                builder.Config.AutoSwitchNotify.Enabled = true;
                StackPanelAutoSwitchNotify.Visibility = Visibility.Visible;
            }
            else
            {
                builder.Config.AutoSwitchNotify.Enabled = false;
                StackPanelAutoSwitchNotify.Visibility = Visibility.Collapsed;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "CheckBoxAutoSwitchNotifyClick");
            }
        }

        private void NumberBoxAutoSwitchNotifyGracePeriod_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (!init)
            {
                if (double.IsNaN(NumberBoxAutoSwitchNotifyGracePeriod.Value)) //fixes crash when leaving box empty and clicking outside it
                    return;

                builder.Config.AutoSwitchNotify.GracePeriodMinutes = Convert.ToInt32(NumberBoxAutoSwitchNotifyGracePeriod.Value);
                try
                {
                    builder.Save();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex, "NumberBoxAutoSwitchNotifyGracePeriod_ValueChanged");
                }
            }
        }

        private void ComboBoxGPUSamples_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            builder.Config.GPUMonitoring.Samples = ComboBoxGPUSamples.SelectedIndex + 1;
            if (init) return;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "ComboBoxGPUSamples_DropDownClosed");
            }
        }

        private void ToggleHotkeys_Toggled(object sender, RoutedEventArgs e)
        {
            TextBlockHotkeyEditHint.Visibility = ToggleHotkeys.IsOn ? Visibility.Visible : Visibility.Hidden;
            if (ToggleHotkeys.IsOn) GridHotkeys.IsEnabled = false;
            else GridHotkeys.IsEnabled = true;
            if (init) return;
            builder.Config.Hotkeys.Enabled = ToggleHotkeys.IsOn;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "toggle_hotkeys");
            }
        }

        private void HotkeyTextboxNoForce_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.NoForce) return;
            builder.Config.Hotkeys.NoForce = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_noforce");
            }
        }

        private void HotkeyTextboxForceDark_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.ForceDark) return;
            builder.Config.Hotkeys.ForceDark = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_forcedark");
            }
        }

        private void HotkeyTextboxForceLight_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.ForceLight) return;
            builder.Config.Hotkeys.ForceLight = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_forcelight");
            }
        }

        private static string GetHotkeyString(KeyEventArgs e)
        {
            e.Handled = true;
            Key key = e.SystemKey == Key.None ? e.Key : e.SystemKey;
            string keyString = key.ToString();

            //Trace.WriteLine(e.SystemKey);
            Trace.WriteLine(keyString);

            if (keyString.Contains("Alt") || keyString.Contains("Shift") || keyString.Contains("Win") || keyString.Contains("Ctrl") || keyString.Contains("System"))
            {
                return null;
            }

            ModifierKeys modifiers = Keyboard.Modifiers;
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            {
                modifiers |= ModifierKeys.Windows;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                modifiers |= ModifierKeys.Alt;
            }

            string isShift = (modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? "Shift + " : "";
            string isCtrl = (modifiers & ModifierKeys.Control) == ModifierKeys.Control ? "Ctrl + " : "";
            string isWin = (modifiers & ModifierKeys.Windows) == ModifierKeys.Windows ? "LWin + " : "";
            string isAlt = (modifiers & ModifierKeys.Alt) == ModifierKeys.Alt ? "Alt + " : "";
            string modifiersString = $"{isCtrl}{isShift}{isAlt}{isWin}";
            return modifiersString.Length > 0 ? $"{modifiersString}{key}" : null;
        }

        private void HotkeyTextToggleAutomaticThemeSwitch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.ToggleAutoThemeSwitch) return;
            builder.Config.Hotkeys.ToggleAutoThemeSwitch = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_toggletheme");
            }
        }

        private void HotkeyCheckboxToggleAutomaticThemeSwitchNotification_Click(object sender, RoutedEventArgs e)
        {
            if (HotkeyCheckboxToggleAutomaticThemeSwitchNotification.IsChecked.Value)
            {
                builder.Config.Notifications.OnAutoThemeSwitching = true;
            }
            else
            {
                builder.Config.Notifications.OnAutoThemeSwitching = false;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "HotkeyCheckboxToggleAutomaticThemeSwitchNotification_Click");
            }
        }

        private void HotkeyTextboxToggleTheme_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.ToggleTheme) return;
            builder.Config.Hotkeys.ToggleTheme = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_toggletheme");
            }
        }

        private void HotkeyTextboxTogglePostpone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string hotkeyString = GetHotkeyString(e);
            if (sender is TextBox tb)
            {
                tb.Text = hotkeyString;
            }
            if (hotkeyString == builder.Config.Hotkeys.TogglePostpone) return;
            builder.Config.Hotkeys.TogglePostpone = hotkeyString;
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "hotkeybox_togglepostpone");
            }
        }

        private void HotkeyCheckboxTogglePostpone_Click(object sender, RoutedEventArgs e)
        {
            if (HotkeyCheckboxTogglePostpone.IsChecked.Value)
            {
                builder.Config.Notifications.OnSkipNextSwitch = true;
            }
            else
            {
                builder.Config.Notifications.OnSkipNextSwitch  = false;
            }
            try
            {
                builder.Save();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex, "HotkeyCheckboxToggleAutomaticThemeSwitchNotification_Click");
            }
        }
    }
}
