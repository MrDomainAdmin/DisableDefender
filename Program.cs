using System;
using Microsoft.Win32;
using System.Linq;
using System.Security.Principal;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;

namespace DisableDefender
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var list = new Dictionary<string, string>();

            //Add all other entries into this list using "list.Add(@"path","value for enabled")
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Real - Time Protection!DisableIOAVProtection", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Real - Time Protection!DisableOnAccessProtection", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Real - Time Protection!DisableRawWriteNotification", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Real - Time Protection!DisableRealtimeMonitoring", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Real - Time Protection!DisableScanOnRealtimeEnable", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender!DisableAntiSpyware", "1");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Spynet!SpynetReporting", "0");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\Spynet!SubmitSamplesConsent", "2");
                list.Add(@"HKLM\Software\Policies\Microsoft\Windows Defender\UX Configuration!Notification_Suppress", "1");
                list.Add(@"HKLM\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications!DisableNotifications", "1");
            
        
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if(!isElevated)
            {
                Console.WriteLine("Please execute in an elevated context");
                return;
            }

            if (args == null || args.Length == 0)
            {
                SetValues(list);
                Console.WriteLine("Defender Disabled - Calling GPUpdate");
                UpdateGPO();
                Console.WriteLine("Please reboot for changes to take effect.");


            }
            else
            {
                if (args[0].ToLower() == "--clean")
                {
                        SetNull(list);
                    
                    var gpo = new GP.ComputerGroupPolicyObject();
                    using( var machine = gpo.GetRootRegistryKey(GP.GroupPolicySection.Machine))
                    {
                        //Removes the folders created by the GPO
                        machine.DeleteSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real - Time Protection", false);
                        machine.DeleteSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", false);
                    }

                    gpo.Save();

                    Console.WriteLine("GPO Removed. Calling GPUpdate...");
                    UpdateGPO();
                    Console.WriteLine("Please reboot for changes to take effect.");
                }
            }
           
        }

        public static void SetValues(Dictionary<string, string> list)
        {
            foreach(var pair in list)
            {
                GP.ComputerGroupPolicyObject.SetPolicySetting(pair.Key, pair.Value, RegistryValueKind.DWord);
            }
        }
        public static void SetNull(Dictionary<string, string> list)
        {
            foreach (var pair in list)
            {
                try
                {
                    GP.ComputerGroupPolicyObject.SetPolicySetting(pair.Key,null, RegistryValueKind.Unknown);
                }
                catch (Exception ex)
                {
                    //on exception
                }
            }
        }

        private static void UpdateGPO()
        {
            try
            {
                Process proc = new Process();
                ProcessStartInfo procStartInfo = new ProcessStartInfo(@"cmd.exe", "/c" + "gpupdate/force");
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.LoadUserProfile = true;
                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                //on exception
            }
        }
    }
}
