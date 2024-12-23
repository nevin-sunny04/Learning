using System;
using System.Management;
using System.Collections.Generic;
using System.Threading;

namespace BitLocker
{
    public class BitLockerCommon : IDisposable
    {
        public readonly string COMMON_NUMERIC_KEY_FRIENDLY_NAME = "OpenText";
        public readonly uint NumericalPassword = 3;
        public readonly uint Passphrase = 8;
        public string DriveLetter { get; set; }
        public string DeviceID { get; set; }
        private ManagementObject classInstance;
        public BitLockerCommon(string driveLetter)
        {
            DriveLetter = CleanDriveLetter(driveLetter);
            DeviceID = GetDeviceIdFromDriveLetter(DriveLetter);
            string devicePath = $"Win32_EncryptableVolume.DeviceID='{DeviceID}'";
            classInstance = new ManagementObject("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", devicePath, null);
        }
        public void Dispose()
        {
            if (classInstance != null)
            {
                classInstance.Dispose();
            }
        }
        public string CleanDriveLetter(string driveLetter)
        {
            int iIdx = driveLetter.IndexOf("\\");
            if (iIdx > 0)
            {
                return driveLetter.Substring(0, iIdx);
            }
            return driveLetter;
        }
        private string GetDeviceIdFromDriveLetter(string driveLetter)
        {
            try
            {
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                        "SELECT * FROM Win32_EncryptableVolume");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string currentDriveLetter = (string)queryObj["DriveLetter"];
                    if (string.Compare(driveLetter, currentDriveLetter, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return (string)queryObj["DeviceID"];
                    }
                }
            }
            catch (ManagementException ex)
            {
                Console.WriteLine($"Error retrieving DeviceID for drive {driveLetter}: {ex.Message}");
            }
            return null; // Return null if no matching drive letter is found
        }
        public BLDrive BitLockerAPI_getBLDriveFromDriveLetter(string DriveLetter)
        {
            Console.WriteLine("Enter BitLockerAPI_getBLDriveFromDriveLetter");
            int iIdx = -1;
            iIdx = DriveLetter.IndexOf("\\");
            if (iIdx > 0) DriveLetter = DriveLetter.Substring(0, iIdx);//get rid of backslash
            BLDrive bldrive = new BLDrive();
            List<string> items = new List<string>();
            try
            {
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                        "SELECT * FROM Win32_EncryptableVolume");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string s = (string)queryObj["DriveLetter"];
                    if (string.Compare(DriveLetter, s) == 0)
                    {
                        int EncryptionMethod = -1;
                        string SelfEncryptionMethod = string.Empty;

                        bldrive.DriveLetter = s;
                        bldrive.DeviceID = (string)queryObj["DeviceID"];
                        bldrive.ProtectionStatus = (BLDrive.BLProtectionStatus)Convert.ToInt32(queryObj["ProtectionStatus"].ToString());

                        try
                        {
                            bldrive.VolumeType = (BLDrive.BLVolumeType)Convert.ToInt32(queryObj["VolumeType"].ToString());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error in BitLockerAPI_getBLDriveFromDriveLetter->VolumeType = {0}", e.HResult.ToString());
                        }

                        bldrive.ConversionStatus = BLDrive.BLConversionStatus.FullyDecrypted;
                        UInt32 retConvStatus = 0;
                        BitLockerAPI_GetConversionStatus(out retConvStatus, out bldrive.EncryptionPercentage);
                        if ((retConvStatus >= 0) && (retConvStatus <= 5)) bldrive.ConversionStatus = (BLDrive.BLConversionStatus)Convert.ToInt32(retConvStatus);

                        bldrive.AutoUnlockEnabled = false;
                        BitLockerAPI_IsAutoUnlockEnabled(out bldrive.AutoUnlockEnabled);

                        BitLockerAPI_GetLockStatus(out bldrive.LockStatus);

                        int ret = BitLockerAPI_GetEncryptionMethod(out EncryptionMethod, out SelfEncryptionMethod);
                        if (SelfEncryptionMethod == string.Empty)
                        {
                            bldrive.EncryptionMethod = (BLDrive.BLEncryptionMethod)EncryptionMethod;
                        }

                        string[] KeyProtectors;
                        UInt32 iret = BitLockerAPI_GetKeyProtectors(out KeyProtectors);
                        if (iret == 0)
                        {
                            for (int i = 0; i < KeyProtectors.Length; i++)
                            {
                                UInt32 KeyProtectorType;
                                string FriendlyName;
                                BitLockerAPI_GetKeyProtectorFriendlyName(KeyProtectors[i], out FriendlyName);
                                BitLockerAPI_GetKeyProtectorType(KeyProtectors[i], out KeyProtectorType);
                                if (KeyProtectorType == NumericalPassword)
                                {
                                    bldrive.KeyProtectorNumericalKey = KeyProtectors[i];
                                    bldrive.KeyProtectorNumericalKeyFriendlyName = FriendlyName;
                                }
                                else if (KeyProtectorType == Passphrase)
                                {
                                    bldrive.KeyProtectorPassPhrase = KeyProtectors[i];
                                    bldrive.KeyProtectorPassPhraseFriendlyName = FriendlyName;
                                }
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_getBLDriveFromDriveLetter = {0}", ex.HResult.ToString());
            }

            return bldrive;
        }
        public BLDrive BitLockerAPI_getBLDriveFromDeviceID(string DeviceID)
        {
            Console.WriteLine("Enter BitLockerAPI_getBLDriveFromDriveDosName");
            BLDrive bldrive = new BLDrive();
            List<string> items = new List<string>();
            try
            {
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                        "SELECT * FROM Win32_EncryptableVolume");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string s = (string)queryObj["DeviceID"];
                    if (string.Compare(DeviceID, s) == 0)
                    {
                        int EncryptionMethod = -1;
                        string SelfEncryptionMethod = string.Empty;

                        bldrive.DriveLetter = (string)queryObj["DriveLetter"];
                        bldrive.DeviceID = (string)queryObj["DeviceID"];
                        bldrive.ProtectionStatus = (BLDrive.BLProtectionStatus)Convert.ToInt32(queryObj["ProtectionStatus"].ToString());
                        bldrive.VolumeType = (BLDrive.BLVolumeType)Convert.ToInt32(queryObj["VolumeType"].ToString());

                        bldrive.ConversionStatus = BLDrive.BLConversionStatus.FullyDecrypted;
                        UInt32 retConvStatus = 0;
                        BitLockerAPI_GetConversionStatus(out retConvStatus, out bldrive.EncryptionPercentage);
                        if ((retConvStatus >= 0) && (retConvStatus <= 5)) bldrive.ConversionStatus = (BLDrive.BLConversionStatus)Convert.ToInt32(retConvStatus);

                        BitLockerAPI_GetLockStatus(out bldrive.LockStatus);

                        bldrive.AutoUnlockEnabled = false;
                        BitLockerAPI_IsAutoUnlockEnabled(out bldrive.AutoUnlockEnabled);

                        int ret = BitLockerAPI_GetEncryptionMethod(out EncryptionMethod, out SelfEncryptionMethod);
                        if (SelfEncryptionMethod == string.Empty)
                        {
                            bldrive.EncryptionMethod = (BLDrive.BLEncryptionMethod)EncryptionMethod;
                        }

                        string[] KeyProtectors;
                        UInt32 iret = BitLockerAPI_GetKeyProtectors(out KeyProtectors);
                        if (iret == 0)
                        {
                            for (int i = 0; i < KeyProtectors.Length; i++)
                            {
                                UInt32 KeyProtectorType;
                                string FriendlyName;
                                BitLockerAPI_GetKeyProtectorFriendlyName(KeyProtectors[i], out FriendlyName);
                                BitLockerAPI_GetKeyProtectorType(KeyProtectors[i], out KeyProtectorType);
                                if (KeyProtectorType == NumericalPassword)
                                {
                                    bldrive.KeyProtectorNumericalKey = KeyProtectors[i];
                                    bldrive.KeyProtectorNumericalKeyFriendlyName = FriendlyName;
                                }
                                else if (KeyProtectorType == Passphrase)
                                {
                                    bldrive.KeyProtectorPassPhrase = KeyProtectors[i];
                                    bldrive.KeyProtectorPassPhraseFriendlyName = FriendlyName;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_getBLDriveFromDeviceID = {0}", ex.HResult.ToString());
            }

            return bldrive;
        }
        public List<BLDrive> BitLockerAPI_getBLDrives()
        {
            Console.WriteLine("Enter BitLockerAPI_getBLDrives");
            List<BLDrive> bldrives = new List<BLDrive>();
            try
            {
                BLDrive bldrive = new BLDrive();

                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                        "SELECT * FROM Win32_EncryptableVolume");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    int EncryptionMethod = -1;
                    string SelfEncryptionMethod = string.Empty;

                    bldrive.DriveLetter = (string)queryObj["DriveLetter"];
                    bldrive.DeviceID = (string)queryObj["DeviceID"];
                    bldrive.ProtectionStatus = (BLDrive.BLProtectionStatus)Convert.ToInt32(queryObj["ProtectionStatus"].ToString());
                    bldrive.VolumeType = (BLDrive.BLVolumeType)Convert.ToInt32(queryObj["VolumeType"].ToString());

                    int ret = BitLockerAPI_GetEncryptionMethod(out EncryptionMethod, out SelfEncryptionMethod);
                    if (SelfEncryptionMethod == string.Empty)
                    {
                        bldrive.EncryptionMethod = (BLDrive.BLEncryptionMethod)EncryptionMethod;
                    }

                    string[] KeyProtectors;
                    UInt32 iret = BitLockerAPI_GetKeyProtectors(out KeyProtectors);
                    if (iret == 0)
                    {
                        for (int i = 0; i < KeyProtectors.Length; i++)
                        {
                            UInt32 KeyProtectorType;
                            string FriendlyName;
                            BitLockerAPI_GetKeyProtectorFriendlyName(KeyProtectors[i], out FriendlyName);
                            BitLockerAPI_GetKeyProtectorType(KeyProtectors[i], out KeyProtectorType);
                            if (KeyProtectorType == NumericalPassword)
                            {
                                bldrive.KeyProtectorNumericalKey = KeyProtectors[i];
                                bldrive.KeyProtectorNumericalKeyFriendlyName = FriendlyName;
                            }
                            else if (KeyProtectorType == Passphrase)
                            {
                                bldrive.KeyProtectorPassPhrase = KeyProtectors[i];
                                bldrive.KeyProtectorPassPhraseFriendlyName = FriendlyName;
                            }
                        }
                    }

                    bldrive.ConversionStatus = BLDrive.BLConversionStatus.FullyDecrypted;
                    UInt32 retConvStatus = 0;
                    BitLockerAPI_GetConversionStatus(out retConvStatus, out bldrive.EncryptionPercentage);
                    if ((retConvStatus >= 0) && (retConvStatus <= 5)) bldrive.ConversionStatus = (BLDrive.BLConversionStatus)Convert.ToInt32(retConvStatus);

                    bldrive.AutoUnlockEnabled = false;
                    BitLockerAPI_IsAutoUnlockEnabled(out bldrive.AutoUnlockEnabled);

                    BitLockerAPI_GetLockStatus(out bldrive.LockStatus);

                    bldrives.Add(bldrive);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_getBLDrives = {0}", ex.HResult.ToString());
            }

            return bldrives;
        }
        public UInt32 BitLockerAPI_PrepForEncryptionPassPhrase(string sFriendlyName, string sPassPhrase, out string sVolumeKeyProtectorID)
        {
            Console.WriteLine("Enter BitLockerAPI_PrepForEncryptionPassPhrase");
            string volkey;
            string returnvalue;
            UInt32 ret = 0;
            try
            {
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithPassPhrase");

                if (!string.IsNullOrEmpty(sFriendlyName))
                    inParams["FriendlyName"] = sFriendlyName;
                inParams["PassPhrase"] = sPassPhrase;

                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithPassPhrase", inParams, null);
                returnvalue = outParams["ReturnValue"].ToString();
                volkey = outParams["VolumeKeyProtectorID"].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_PrepForEncryptionPassPhrase = {0}", ex.HResult.ToString());
                sVolumeKeyProtectorID = string.Empty;
                return (UInt32)ex.HResult;
            }
            sVolumeKeyProtectorID = volkey;
            ret = Convert.ToUInt32(returnvalue);
            return ret;
        }
        public UInt32 BitLockerAPI_PrepForEncryptionNumericalKey(string sFriendlyName, out string sVolumeKeyProtectorID)
        {
            Console.WriteLine("Enter BitLockerAPI_PrepForEncryptionNumericalKey");
            string volkey;
            string returnvalue;
            UInt32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithNumericalPassword");

                Thread.Sleep(2000);

                inParams["FriendlyName"] = sFriendlyName;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithNumericalPassword", inParams, null);

                Thread.Sleep(2000);

                returnvalue = outParams["ReturnValue"].ToString();
                volkey = outParams["VolumeKeyProtectorID"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_PrepForEncryptionNumericalKey = {0}", ex.HResult.ToString());
                sVolumeKeyProtectorID = string.Empty;
                return (UInt32)ex.HResult;
            }
            sVolumeKeyProtectorID = volkey;
            ret = Convert.ToUInt32(returnvalue);
            return ret;
        }
        public UInt32 BitLockerAPI_ProtectKeyWithExternalKey(string sFriendlyName, string sExternalKey, out string sVolumeKeyProtectorID)
        {
            Console.WriteLine("Enter BitLockerAPI_ProtectKeyWithExternalKey");
            UInt32 ret = 1;
            string volkey = string.Empty;
            try
            {
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithExternalKey");

                // Add the input parameters.
                if (!String.IsNullOrEmpty(sExternalKey)) inParams["ExternalKey"] = sExternalKey;
                if (!String.IsNullOrEmpty(sFriendlyName)) inParams["FriendlyName"] = sFriendlyName;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithExternalKey", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
                volkey = outParams["VolumeKeyProtectorID"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_ProtectKeyWithExternalKey = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            sVolumeKeyProtectorID = volkey;
            return ret;
        }
        public UInt32 BitLockerAPI_GetKeyProtectorNumericalPassword(string sVolumeKeyProtectorID, out string NumericalPassword)
        {
            string numericalPassword = string.Empty;
            Console.WriteLine("Enter BitLockerAPI_GetKeyProtectorNumericalPassword");
            uint ret;
            try
            {
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetKeyProtectorNumericalPassword");

                // Add the input parameters.
                if (!String.IsNullOrEmpty(sVolumeKeyProtectorID)) inParams["VolumeKeyProtectorID"] = sVolumeKeyProtectorID;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetKeyProtectorNumericalPassword", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
                numericalPassword = outParams["NumericalPassword"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_ProtectKeyWithExternalKey = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }

            NumericalPassword = numericalPassword;
            return ret;
        }
        public UInt32 DisableKeyProtectors()
        {
            try
            {
                // Invoke DisableKeyProtectors method
                ManagementBaseObject outParams = classInstance.InvokeMethod("DisableKeyProtectors", null, null);
                UInt32 ret = (UInt32)outParams["ReturnValue"];
                if (ret == 0)
                {
                    Console.WriteLine("Key protectors disabled successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to disable key protectors. Error code: {0}", ret);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DisableKeyProtectors = {0}", ex.HResult.ToString());
                return (UInt32)ex.HResult;
            }
        }
        public UInt32 EnableKeyProtectors()
        {
            try
            {
                // Invoke DisableKeyProtectors method
                ManagementBaseObject outParams = classInstance.InvokeMethod("EnableKeyProtectors", null, null);
                UInt32 ret = (UInt32)outParams["ReturnValue"];
                if (ret == 0)
                {
                    Console.WriteLine("Key protectors enabled successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to disable key protectors. Error code: {0}", ret);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in EnableKeyProtectors = {0}", ex.HResult.ToString());
                return (UInt32)ex.HResult;
            }
        }
        public UInt32 BitLockerAPI_Encrypt(UInt32 EncryptionMethod, UInt32 EncryptionFlags)
        {
            Console.WriteLine("Enter BitLockerAPI_Encrypt");
            UInt32 ret = 1;
            try
            {
                Thread.Sleep(2000);
                ManagementBaseObject inParams = classInstance.GetMethodParameters("Encrypt");
                inParams["EncryptionFlags"] = EncryptionFlags;
                ManagementBaseObject outParams = classInstance.InvokeMethod("Encrypt", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_Encrypt = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_EncryptAfterHardWareTest(UInt32 EncryptionMethod, UInt32 EncryptionFlags)
        {
            Console.WriteLine("Enter BitLockerAPI_EncryptAfterHardWareTest");
            UInt32 ret = 1;
            try
            {
                Thread.Sleep(2000);
                ManagementBaseObject inParams = classInstance.GetMethodParameters("EncryptAfterHardwareTest");
                inParams["EncryptionMethod"] = EncryptionMethod;
                inParams["EncryptionFlags"] = EncryptionFlags;

                ManagementBaseObject outParams = classInstance.InvokeMethod("EncryptAfterHardwareTest", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_EncryptAfterHardWareTest = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_LockDrive(bool bForceDismount)
        {
            Console.WriteLine("Enter BitLockerAPI_LockDrive");
            UInt32 ret = 1;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("Lock");
                // Add the input parameters.
                inParams["ForceDismount"] = bForceDismount;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("Lock", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_LockDrive = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_ProtectionStatus()
        {
            Console.WriteLine("Enter BitlockerAPI_ProtectionStatus");
            UInt32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetProtectionStatus");

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetProtectionStatus", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_UnlockWithPassPhrase = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_Decrypt()
        {
            Console.WriteLine("Enter BitLockerAPI_Decrypt");
            UInt32 ret = 1;
            try
            {
                ManagementBaseObject outParams = classInstance.InvokeMethod("Decrypt", null, null);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_Decrypt = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_UnlockWithPassPhrase(string sPassPhrase)
        {
            Console.WriteLine("Enter BitLockerAPI_UnlockWithPassPhrase");
            UInt32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("UnlockWithPassPhrase");
                // Add the input parameters.
                inParams["PassPhrase"] = sPassPhrase;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("UnlockWithPassPhrase", inParams, null);
                ret = (UInt32)outParams["ReturnValue"];

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_UnlockWithPassPhrase = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_UnlockWithNumericalPassword(string NumericalKey)
        {
            Console.WriteLine("Enter BitLockerAPI_UnlockWithNumericalPassword");
            UInt32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("UnlockWithNumericalPassword");
                // Add the input parameters.
                inParams["NumericalPassword"] = NumericalKey;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("UnlockWithNumericalPassword", inParams, null);
                // List outParams
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_UnlockWithNumericalPassword = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public Int32 BitLockerAPI_DisableAutoUnlock()
        {
            Int32 ret = 0;

            try
            {
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("DisableAutoUnlock", null, null);
                // List outParams
                ret = Convert.ToInt32(outParams["ReturnValue"]);
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_DisableAutoUnlock = {0}", err.HResult.ToString());
                ret = err.HResult;
            }
            return ret;
        }
        public Int32 BitLockerAPI_EnableAutoUnlock(string sVolumeKeyProtectorID)
        {
            Int32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("EnableAutoUnlock");
                // Add the input parameters.
                inParams["VolumeKeyProtectorID"] = sVolumeKeyProtectorID;
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("EnableAutoUnlock", inParams, null);
                // List outParams
                ret = Convert.ToInt32(outParams["ReturnValue"]);
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_EnableAutoUnlock = {0}", err.HResult.ToString());

                ret = err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_IsAutoUnlockEnabled(out bool bIsAutoUnlockEnabled)
        {
            UInt32 ret = 0;
            bIsAutoUnlockEnabled = false;
            try
            {
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("IsAutoUnlockEnabled", null, null);
                bIsAutoUnlockEnabled = Convert.ToBoolean(outParams["IsAutoUnlockEnabled"]);
                ret = Convert.ToUInt32(outParams["ReturnValue"]);
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_IsAutoUnlockEnabled = {0}", err.HResult.ToString());

                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public Int32 BitLockerAPI_GetEncryptionMethod(out int EncryptionMethod, out string SelfEncryptionMethod)
        {
            Int32 ret = 0;
            int encryptMethod = -1;
            string selfencryptmethod = string.Empty;
            ManagementBaseObject outParams = null;
            try
            {
                // Execute the method and obtain the return values.
                outParams = classInstance.InvokeMethod("GetEncryptionMethod", null, null);

                // List outParams

                encryptMethod = Convert.ToInt32(outParams["EncryptionMethod"]);
                ret = Convert.ToInt32(outParams["ReturnValue"]);
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetEncryptionMethod = {0}", err.HResult.ToString());
                EncryptionMethod = -1;
                ret = err.HResult;
            }
            // The encryption algorithm is configured on the self-encrypting drive. 
            // A null string means that either BitLocker is using software encryption or no encryption method is reported.
            try
            {
                selfencryptmethod = (string)outParams["SelfEncryptionDriveEncryptionMethod"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetEncryptionMethod (2) = {0}", err.HResult.ToString());
                SelfEncryptionMethod = string.Empty;
                ret = err.HResult;
            }
            EncryptionMethod = encryptMethod;
            SelfEncryptionMethod = selfencryptmethod;
            return ret;
        }
        public UInt32 BitLockerAPI_DeleteProtectors()
        {
            UInt32 ret = 255;

            try
            {
                DisableKeyProtectors();
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("DeleteKeyProtectors", null, null);
                // List outParams
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_DeleteProtectors = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_DeleteProtector(string VolumeKeyProtectorID)
        {
            UInt32 ret = 255;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("DeleteKeyProtector");
                // Add the input parameters.
                inParams["VolumeKeyProtectorID"] = VolumeKeyProtectorID;
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("DeleteKeyProtector", inParams, null);
                // List outParams
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_DeleteProtector = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetKeyProtectors(UInt32 KeyProtectorType, out string[] KeyProtectors)
        {
            UInt32 ret = 255;

            try
            {   // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetKeyProtectors");

                // Add the input parameters.
                inParams["KeyProtectorType"] = KeyProtectorType;  // Gets specified key protectors; 8=passphrase, 2=ExternalKey, 3=Numeric

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetKeyProtectors", inParams, null);

                // List outParams
                KeyProtectors = (string[])outParams["VolumeKeyProtectorID"];
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectors = {0}", err.HResult.ToString());
                KeyProtectors = null;
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_ProtectKeyWithTPMAndPIN(string pin)
        {
            UInt32 ret = 255;

            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithTPMAndPIN");

                // Add the input parameters.
                inParams["PIN"] = pin;

                // Optional: Set a friendly name (you can leave this out if not needed)
                inParams["FriendlyName"] = "";

                // Optional: Platform validation profile, can be null for default
                byte[] platformValidationProfile = new byte[] { 0, 2, 4, 5, 8, 9, 10, 11 };  // Default PCR indices for validation
                inParams["PlatformValidationProfile"] = platformValidationProfile;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithTPMAndPIN", inParams, null);

                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectors = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_ProtectKeyWithTPM()
        {
            UInt32 ret = 255;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithTPM");
                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithTPM", inParams, null);
                string VolumeKeyProtectorID = Convert.ToString(outParams.Properties["VolumeKeyProtectorID"].Value);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectors = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetKeyProtectors(out string[] KeyProtectors)
        {
            UInt32 ret = 255;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetKeyProtectors");

                // Add the input parameters.
                inParams["KeyProtectorType"] = 0;  // Gets all key protectors

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetKeyProtectors", inParams, null);

                // List outParams
                KeyProtectors = (string[])outParams["VolumeKeyProtectorID"];
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectors (2) = {0}", err.HResult.ToString());
                KeyProtectors = null;
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetKeyProtectorType(string kp, out UInt32 KeyProtectorType)
        {
            UInt32 ret = 255;
            KeyProtectorType = 255;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetKeyProtectorType");

                // Add the input parameters.
                inParams["VolumeKeyProtectorID"] = kp;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetKeyProtectorType", inParams, null);

                // List outParams
                KeyProtectorType = (UInt32)outParams["KeyProtectorType"];
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectorType = {0}", err.HResult.ToString());
                KeyProtectorType = 255;
                return (UInt32)err.HResult;
            }
            // valid return types:
            //    0 = unknown (maybe locked?)
            //    1 = Trusted Platform Module (TPM)
            //    2 = External Key
            //    3 = Numerical password
            //    4 = TPN and PIN
            //    5 = TPM and Startup Key
            //    6 = TPM and PIN and Startup Key
            //    7 = Public Key
            //    8 = Passphrase
            //    9 = TPM Certificate
            //   10 = CryptoAPI Next Generation (CNG) Protector

            return ret;
        }
        public UInt32 BitLockerAPI_GetKeyProtectorFriendlyName(string KeyProtectorID, out string FriendlyName)
        {
            UInt32 ret = 255;
            FriendlyName = string.Empty;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetKeyProtectorFriendlyName");

                // Add the input parameters.
                inParams["VolumeKeyProtectorID"] = KeyProtectorID;    //"E7A25786-4A0E-496F-97E4-5ED496974361";

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetKeyProtectorFriendlyName", inParams, null);

                // List outParams
                FriendlyName = (string)outParams["FriendlyName"];
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetKeyProtectorFriendlyName = {0}", err.HResult.ToString());
                FriendlyName = string.Empty;
                return (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetProtectionStatus()
        {
            Console.WriteLine("Enter BitLockerAPI_GetProtectionStatus");
            UInt32 ret = 0;
            try
            {
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetProtectionStatus", null, null);
                ret = (UInt32)outParams["ProtectionStatus"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BitLockerAPI_GetProtectionStatus = {0}", ex.HResult.ToString());
                ret = (UInt32)ex.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetConversionStatus(out UInt32 iConversionStatus, out string sEncryptionPercentage)
        {
            UInt32 ret = 0;
            sEncryptionPercentage = string.Empty;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetConversionStatus");

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetConversionStatus", inParams, null);

                // List outParams
                //Console.WriteLine("Out parameters:");
                //Console.WriteLine("ConversionStatus: " + outParams["ConversionStatus"]);
                //Console.WriteLine("EncryptionFlags: " + outParams["EncryptionFlags"]);
                //Console.WriteLine("EncryptionPercentage: " + outParams["EncryptionPercentage"]);
                //Console.WriteLine("ReturnValue: " + outParams["ReturnValue"]);
                //Console.WriteLine("WipingPercentage: " + outParams["WipingPercentage"]);
                //Console.WriteLine("WipingStatus: " + outParams["WipingStatus"]);

                sEncryptionPercentage = Convert.ToString((UInt32)outParams["EncryptionPercentage"]);
                iConversionStatus = (UInt32)outParams["ConversionStatus"];
                //0=FullyDecrypted, 1=FullyEncrypted, 2=EncryptionInProgress, 3=DecryptionInProgress, 4=EncryptionPaused, 5=DecryptionPaused
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetConversionStatus = {0}", err.HResult.ToString());
                iConversionStatus = 999;
                ret = (UInt32)err.HResult;
            }
            catch (Exception err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetConversionStatus (2) = {0}", err.HResult.ToString());
                iConversionStatus = 999;
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetLockStatus(out int LockStatus)
        {
            UInt32 ret = 0;
            LockStatus = 0;//unlocked
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetConversionStatus");
                // no method in-parameters to define
                // Execute the method and obtain the return values.
                try
                {
                    ManagementBaseObject outParams = classInstance.InvokeMethod("GetLockStatus", null, null);
                    // List outParams
                    LockStatus = Convert.ToInt32(outParams["LockStatus"]);//1 is locked, 0 is unlocked
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in BitLockerAPI_GetLockStatus.  Most likely do to drive removal after the process started.  Exceptoin is: = {0}", e.ToString());
                }

            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetLockStatus = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_GetConversionStatus(out string sPercentage)
        {
            UInt32 ret = 0;
            try
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetConversionStatus");

                //// Add the input parameters. // Doesn't work on Windows 7
                // inParams["PrecisionFactor"] = 1;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("GetConversionStatus", inParams, null);
                // List outParams
                //Console.WriteLine("Out parameters:");
                //Console.WriteLine("ConversionStatus: " + outParams["ConversionStatus"]);
                //Console.WriteLine("EncryptionFlags: " + outParams["EncryptionFlags"]);
                //Console.WriteLine("EncryptionPercentage: " + outParams["EncryptionPercentage"]);
                //Console.WriteLine("ReturnValue: " + outParams["ReturnValue"]);
                //Console.WriteLine("WipingPercentage: " + outParams["WipingPercentage"]);
                //Console.WriteLine("WipingStatus: " + outParams["WipingStatus"]);
                sPercentage = Convert.ToString((UInt32)outParams["WipingPercentage"]);
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetConversionStatus (3) = {0}", err.HResult.ToString());
                sPercentage = string.Empty;
                ret = (UInt32)err.HResult;
            }
            catch (Exception err)
            {
                Console.WriteLine("Error in BitLockerAPI_GetConversionStatus (4) = {0}", err.HResult.ToString());
                sPercentage = string.Empty;
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public UInt32 BitLockerAPI_PauseConversion(out string PauseStatus)
        {
            UInt32 ret = 0;
            try
            {
                // None in-parameters required for the method
                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = classInstance.InvokeMethod("PauseConversion", null, null);
                PauseStatus = Convert.ToString((UInt32)outParams["PauseConversion"]);
            }
            catch (ManagementException err)
            {
                PauseStatus = "100";   // error code used to provide an otherwise invalid status
                Console.WriteLine("Error in BitLockerAPI_PauseConversion = {0}", err.HResult.ToString());
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public string CalculateNumericalPasswordFromSESKey(string SESKey)
        {
            // {BA508289-44C2-40ff-B976-96FEB8416716} - sample ses key format
            // 553894-263560-224070-591811-532367-599390-527791-518342  - numerical key sample with grouping
            // 553894263560224070591811532367599390527791518342           remove grouping for final output
            // each group is designated as such: x1 x2 x3 x4 x5 x6
            // x6 = (-x1 + x2 - x3 + x4 - x5) mod 11 (for each group)
            // if the result is negative then x6 = the absolute value of x6
            // if the result is positive then x6 = 11 - ((-x1 + x2 - x3 + x4 - x5) mod 11)
            // Also, the overall 6 digit number cannot be greater than (2^16 - 1) * 11 or 720,885.

            int irabs = 0;
            int i = 0;
            int j = 0;
            string sret = string.Empty;

            // The following line is for testing purposes
            //string SESKey = "{BA508289-44C2-40ff-B976-96FEB8416716}";

            // Pre-load array with padded chars
            char[] ch = { '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5',
                              '0','1','2','3','4','5'};

            // Remove braces and hyphens from SESKey
            // Convert 'A' to '1' etc for B,C,D,E,F as 2,3,4,5,6
            string parms = "{}-";
            foreach (char c in SESKey)
            {
                if (!parms.Contains(c.ToString()))
                {
                    if (c >= 'A' && c <= 'F' ||
                        c >= 'a' && c <= 'f')
                    {
                        int it = Convert.ToInt16(c);
                        it = it & 0x000f;
                        it = it | 0x0030;
                        ch[i] = (char)it;
                    }
                    else
                    {
                        ch[i] = c;
                    }
                    i++;
                }
            }

            char[] ch1 = new char[ch.Length];
            for (i = 0; i < 48; i++)
            {
                ch1[i] = ch[i];
            }

            for (i = 0, j = 0; i < 48; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    ch1[i++] = ch[j++];
                }
                ch1[i] = '0';
            }

            for (i = 0; i < 48;)
            {
            converter:
                uint ia = Convert.ToUInt16(ch1[i++]);
                ia = ia & 0x0f;
                // enforce rule that group cannot be larger than (2^16 - 1) * 11 by trimming largest digit to '6'
                if (ia > 0x06)
                {
                    ia = 0x06;
                    ch1[--i] = '6';
                    i++;
                }
                uint ib = Convert.ToUInt16(ch1[i++]);
                ib = ib & 0x0f;
                uint ic = Convert.ToUInt16(ch1[i++]);
                ic = ic & 0x0f;
                uint id = Convert.ToUInt16(ch1[i++]);
                id = id & 0x0f;
                uint ie = Convert.ToUInt16(ch1[i++]);
                ie = ie & 0x0f;
                int ir = (int)(-ia + ib - ic + id - ie);
                ir = ir % 11;
                if (ir == 0x0001 || ir == -10)
                {
                    j = i - 5;
                    for (int n = 0; n < 5; n++)
                    {
                        if (ch1[j] != '0')
                        {
                            char chr = ch1[j];
                            chr--;
                            ch1[j] = chr;
                            break;
                        }
                        j++;
                    }
                    i -= 5;
                    goto converter;
                }
                if (ir <= 0)
                {
                    irabs = Math.Abs(ir);
                }
                else
                {
                    irabs = 11 - ir;
                }
                irabs = irabs | 0x0030;
                ch1[i++] = (char)irabs;
            }
            sret = new string(ch1);
            return sret;
        }
        public UInt32 BitLockerAPI_ProtectKeyWithPassPhrase(string passPhrase)
        {
            UInt32 ret = 255;
            try
            {
                ManagementBaseObject inParams = classInstance.GetMethodParameters("ProtectKeyWithPassPhrase");
                inParams["FriendlyName"] = "";
                // Set passphrase (correct parameter key)
                inParams["Passphrase"] = passPhrase;
                // Execute the method and obtain the return values
                ManagementBaseObject outParams = classInstance.InvokeMethod("ProtectKeyWithPassPhrase", inParams, null);
                // Retrieve the VolumeKeyProtectorID if needed
                string VolumeKeyProtectorID = System.Convert.ToString(outParams.Properties["VolumeKeyProtectorID"].Value);
                ret = (UInt32)outParams["ReturnValue"];
            }
            catch (ManagementException err)
            {
                Console.WriteLine("Error in BitLockerAPI_ProtectKeyWithPassphrase = {0}", err.Message);
                ret = (UInt32)err.HResult;
            }
            return ret;
        }
        public class BLDrive
        {
            public BLDrive() { }
            public enum BLEncryptionMethod : Int32
            {
                None = 0,
                AES_128_WITH_DIFFUSER = 1,  // deprecated after Windows 8.1 or higher
                AES_256_WITH_DIFFUSER = 2,  // deprecated after Windows 8.1 or higher
                AES_128 = 3,
                AES_256 = 4,
                HARDWARE_ENCRYPTION = 5,
                XTS_AES_128 = 6,  // Only on Windows 10 version 1511 or higher
                XTS_AES_256 = 7,  // Only on Windows 10 version 1511 or higher
                UNKNOWN = -1
            }
            public enum BLVolumeType : Int32
            {
                SystemVolume = 0,
                FixedVolume = 1,
                RemovableMedia = 2
            }
            public enum BLProtectionStatus : Int32
            {
                None = 0,
                Protected = 1,
                Locked = 2
            }

            public string KeyProtectorPassPhrase;
            public string KeyProtectorPassPhraseFriendlyName;
            public string KeyProtectorNumericalKey;
            public string KeyProtectorNumericalKeyFriendlyName;

            public string DriveLetter;
            public string DeviceID;
            public BLProtectionStatus ProtectionStatus;
            public BLVolumeType VolumeType;//this API doesn't work on Win7
            public BLEncryptionMethod EncryptionMethod;
            public bool AutoUnlockEnabled;
            public int LockStatus;//1=locked, 0=unlocked

            public enum BLConversionStatus : Int32
            {
                FullyDecrypted = 0,
                FullyEncrypted = 1,
                EncryptionInProgress = 2,
                DecryptionInProgress = 3,
                EncryptionPaused = 4,
                DecryptionPaused = 5,

            }
            public BLConversionStatus ConversionStatus;
            public string EncryptionPercentage;
        }
    }
}