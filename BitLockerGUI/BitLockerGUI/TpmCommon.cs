using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitLockerGUI
{
    using System;
    using System.Management;

    namespace BitLocker
    {
        public static class TpmCommon
        {
            private static ManagementObject tpm;

            static TpmCommon()
            {
                try
                {
                    // Initialize the TPM object
                    var scope = new ManagementScope("\\\\.\\root\\CIMV2\\Security\\MicrosoftTpm");
                    var query = new SelectQuery("Win32_Tpm");

                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        var tpmDevices = searcher.Get();

                        // Get the first TPM device
                        foreach (ManagementObject device in tpmDevices)
                        {
                            tpm = device;
                            break;
                        }
                    }
                    if (tpm == null)
                    {
                        Console.WriteLine("No TPM device found on this system.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing TPM: {ex.Message}");
                }

            }
            public static uint TakeOwnership()
            {
                try
                {
                    UInt32 result = 1;
                    UInt32 retIsEnabled = IsEnabled(out bool isEnabled);
                    UInt32 retIsActive = IsActivated(out bool isActive);
                    UInt32 retIsAllowed = IsOwnershipAllowed(out bool isAllowed);

                    if (isEnabled && isActive && isAllowed)
                    {
                        ManagementBaseObject inParams = tpm.GetMethodParameters("TakeOwnership");
                        ManagementBaseObject outParams = tpm.InvokeMethod("TakeOwnership", inParams, null);
                        result = (uint)outParams["ReturnValue"];
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TakeOwnership: " + ex.Message);
                    return (uint)ex.HResult;
                }
            }
            public static uint ClearOwnership()
            {
                uint result = 255;
                try
                {
                    if (tpm == null)
                    {
                        return result;
                    }
                    UInt32 retIsEnabled = IsEnabled(out bool isEnabled);
                    UInt32 retIsActive = IsActivated(out bool isActive);
                    if (isEnabled && isActive)
                    {
                        ManagementBaseObject inParams = tpm.GetMethodParameters("Clear");
                        ManagementBaseObject outParams = tpm.InvokeMethod("Clear", inParams, null);
                        result = (uint)outParams["ReturnValue"];
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in Clear: " + ex.Message);
                    return (uint)ex.HResult;
                }
            }
            public static uint IsEnabled(out bool isEnable)
            {
                try
                {
                    ManagementBaseObject inParams = tpm.GetMethodParameters("IsEnabled");

                    ManagementBaseObject outParams = tpm.InvokeMethod("IsEnabled", inParams, null);
                    isEnable = (bool)outParams["IsEnabled"];
                    return (uint)outParams["ReturnValue"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsEnabled: " + ex.Message);
                    isEnable = false;
                    return (uint)ex.HResult;
                }
            }
            public static uint IsActivated(out bool isActive)
            {
                try
                {
                    ManagementBaseObject inParams = tpm.GetMethodParameters("IsActivated");

                    ManagementBaseObject outParams = tpm.InvokeMethod("IsActivated", inParams, null);
                    isActive = (bool)outParams["IsActivated"];
                    return (uint)outParams["ReturnValue"];

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsActivated: " + ex.Message);
                    isActive = false;
                    return (uint)ex.HResult;
                }
            }
            public static uint IsOwnershipAllowed(out bool isAllowed)
            {
                try
                {
                    ManagementBaseObject inParams = tpm.GetMethodParameters("IsOwnershipAllowed");

                    ManagementBaseObject outParams = tpm.InvokeMethod("IsOwnershipAllowed", inParams, null);
                    isAllowed = (bool)outParams["IsOwnershipAllowed"];
                    return (uint)outParams["ReturnValue"];

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsOwnershipAllowed: " + ex.Message);
                    isAllowed = false;
                    return (uint)ex.HResult;
                }
            }
        }
    }
}
