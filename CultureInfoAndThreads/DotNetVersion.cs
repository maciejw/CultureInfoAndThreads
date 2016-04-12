using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;

namespace CultureInfoAndThreads
{
    public static class DotNetVersion
    {

        public static void ShowVersionHeader()
        {
            var assembly = typeof(CultureInfo).Assembly;
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine("Version info:");
            Console.WriteLine($"{version}");
        }
        public static void Show45or451FromRegistry()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                var ndpValue = ndpKey.GetValue("Release") as int?;
                if (ndpValue.HasValue)
                {
                    Console.WriteLine("Registry version: " + CheckFor45DotVersion(ndpValue.Value));
                }
                else
                {
                    Console.WriteLine("Registry version 4.5 or later is not detected.");
                }
            }
            Console.WriteLine();
        }
        public static void ShowVersionFromEnvironment()
        {
            Console.WriteLine($"Environment version: {Environment.Version}");
            Console.WriteLine();

        }
        public static void ShowUpdates()
        {
            Console.WriteLine("Updates:");
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates"))
            {
                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework") || baseKeyName.StartsWith("KB") || baseKeyName.Contains(".NETFramework"))
                    {
                        using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            string name = (string)updateKey.GetValue("PackageName", "");
                            Console.WriteLine($"{baseKeyName}  {name}");
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                using (RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
                                {
                                    name = (string)kbKey.GetValue("PackageName", "");
                                    Console.WriteLine($"  {kbKeyName}  {name}");

                                    if (kbKey.SubKeyCount > 0)
                                    {
                                        foreach (string sbKeyName in kbKey.GetSubKeyNames())
                                        {
                                            using (RegistryKey sbSubKey = kbKey.OpenSubKey(sbKeyName))
                                            {
                                                name = (string)sbSubKey.GetValue("PackageName", "");
                                                if (name == "")
                                                    name = (string)sbSubKey.GetValue("Description", "");
                                                Console.WriteLine($"    {sbKeyName}  {name}");

                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
    }
}
