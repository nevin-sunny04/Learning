using BitLocker;
using BitLockerGUI.BitLocker;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static BitLocker.BitLockerCommon;

namespace BitLockerGUI
{
    public class Program
    {
        private static BitLockerCommon bitLockerCommon = null;
        private bool encryptionInProgress = true;
        private bool isTpmActive = false;
        public static string driveLetter = "";
        
        [STAThread] // Required for GUI
        static void Main(string[] args)
        {
            Thread.Sleep(20000);
            Program program = new Program();
            program.CheckAndEncryptDrives();
        }
        public void CheckAndEncryptDrives()
        {
            try
            {
                List<string> driveLetters = GetDriveLetters();
                Console.WriteLine("Starting drive encryption...");
                foreach (var driveLetterLoop in driveLetters)
                {
                    driveLetter = driveLetterLoop;
                    bool isNonOS = IsNonOSDrive(driveLetter);
                    bitLockerCommon = new BitLockerCommon(driveLetter);
                    if (!IsAlreadyEncrypted())
                    {
                        if (isNonOS)
                        {
                            EncryptNonOSDrives();
                        }
                        else
                        {
                            TpmThread();
                            HandleUserInputAndEncrypt();
                        }
                        Console.WriteLine($"Drive {driveLetter} is {(isNonOS ? "a non-OS drive" : "an OS-related drive")}.");
                    }
                    else
                    {
                        Console.WriteLine("Drive Already Encrypted");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public bool IsAlreadyEncrypted()
        {
            if(bitLockerCommon.BitLockerAPI_ProtectionStatus() == 0)
            {
                string[] KeyProtectors;
                UInt32 iret = bitLockerCommon.BitLockerAPI_GetKeyProtectors(out KeyProtectors);
                if (iret == 0)
                {
                    foreach (string keyProtector in KeyProtectors)
                    {
                        uint retrieveNumericalPassword = bitLockerCommon.BitLockerAPI_GetKeyProtectorNumericalPassword(keyProtector, out string numericalPassword);
                        if (retrieveNumericalPassword == 0)
                        {
                            GenerateERIFile(keyProtector, numericalPassword);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        public void TpmThread()
        {
            Console.WriteLine("Checking TPM status");
            try
            {
                uint tpmStatus = TpmCommon.ClearOwnership();
                if (tpmStatus == 0)
                {
                    tpmStatus = TpmCommon.TakeOwnership();
                    if (tpmStatus == 0)
                    {
                        Console.WriteLine("TPM operations completed successfully.");
                        isTpmActive = true;
                    }
                }
                else
                {
                    Console.WriteLine("This device does not have TPM enabled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private List<string> GetDriveLetters()
            {
                List<string> driveLetters = new List<string>();
                var scope = new ManagementScope("\\\\.\\root\\CIMV2");
                var query = new ObjectQuery("SELECT * FROM Win32_Volume WHERE DriveLetter IS NOT NULL");

                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    var volumes = searcher.Get();

                    foreach (ManagementObject volume in volumes)
                    {
                        string drive = volume["DriveLetter"]?.ToString();
                        if (!string.IsNullOrEmpty(drive))
                        {
                            driveLetters.Add(drive);
                        }
                    }
                }

                return driveLetters;
            }
        public bool IsNonOSDrive(string drive)
        {
            try
            {
                var scope = new ManagementScope("\\\\.\\root\\CIMV2");
                var query = new ObjectQuery($"SELECT * FROM Win32_Volume WHERE DriveLetter = '{drive}'");
                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    var volumes = searcher.Get();
                        foreach (ManagementObject volume in volumes)
                    {
                        bool isBootVolume = Convert.ToBoolean(volume["BootVolume"]);
                        bool isSystemVolume = Convert.ToBoolean(volume["SystemVolume"]);
                        return !isBootVolume && !isSystemVolume;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return false;
        }
        public void EncryptNonOSDrives()
        {
            try
            {
                uint encryptionMethod = (Int32)BLDrive.BLEncryptionMethod.XTS_AES_256;
                UInt32 EncryptionFlags = 0;
                uint retPrepNumerical = bitLockerCommon.BitLockerAPI_PrepForEncryptionNumericalKey("OpenText", out string sVolumeKeyProtectorID);
                if (retPrepNumerical != 0)
                    return;
                uint retrieveNumericalPassword = bitLockerCommon.BitLockerAPI_GetKeyProtectorNumericalPassword(sVolumeKeyProtectorID, out string numericalPassword);
                if (retrieveNumericalPassword != 0)
                        return;
                GenerateERIFile(sVolumeKeyProtectorID, numericalPassword);
                uint retEncrypt = bitLockerCommon.BitLockerAPI_Encrypt(encryptionMethod, EncryptionFlags);
                if (retEncrypt == 0)
                {
                    Console.WriteLine("Encryption started successfully");
                    MonitorEncryptionProgress();
                }
                else
                {
                    Console.WriteLine("Encryption failed with error code: " + retEncrypt);
                    encryptionInProgress = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during encryption: " + ex.Message);
                encryptionInProgress = false;
            }
        }
        public void HandleUserInputAndEncrypt()
        {
            uint retPrepNumerical = bitLockerCommon.BitLockerAPI_PrepForEncryptionNumericalKey("OpenText", out string sVolumeKeyProtectorID);
            if (retPrepNumerical != 0)
                return;
            uint retrieveNumericalPassword = bitLockerCommon.BitLockerAPI_GetKeyProtectorNumericalPassword(sVolumeKeyProtectorID, out string numericalPassword);
            if (retrieveNumericalPassword != 0)
                return;
            GenerateERIFile(sVolumeKeyProtectorID, numericalPassword);
            string imagePath = @"C:\Users\Admin\Desktop\bitlocker\ZFDE_banner_wider.png";
            string userInput = ShowInputDialog(isTpmActive ? "Enter PIN for encryption:" : "Enter passphrase for encryption:", imagePath);
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.WriteLine("Input cannot be empty. Exiting...");
                return;
            }
            uint retProtectKey = isTpmActive
                ? bitLockerCommon.BitLockerAPI_ProtectKeyWithTPMAndPIN(userInput): bitLockerCommon.BitLockerAPI_ProtectKeyWithPassPhrase(userInput);
            if (retProtectKey == 0)
            {
                Console.WriteLine("Key protector added successfully. Starting encryption...");
                uint encryptionMethod = (Int32)BLDrive.BLEncryptionMethod.XTS_AES_256;
                uint retEncrypt = bitLockerCommon.BitLockerAPI_Encrypt(encryptionMethod, 0);
                if (retEncrypt == 0)
                {
                    ShowProgressDialog("BitLocker Encryption Progress");
                }
                else
                {
                    Console.WriteLine("Encryption failed with error code: " + retEncrypt);
                }
            }
            else
            {
                Console.WriteLine("Failed to add key protector.");
            }
        }
        private string ShowInputDialog(string message, string imagePath)
        {
            string input = null;
            string confirmInput = null;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Add header image
            PictureBox headerImage = new PictureBox
            {
                Image = Image.FromFile(imagePath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = 480,
                Height = 100,
                Top = 10,
                Left = 10
            };

            Form prompt = new Form
            {
                Width = 500,
                Height = 300,
                Text = "ZENworks BitLocker Encryption",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label { Left = 50, Top = 120, Text = message, Width = 400 };
            TextBox textBox = new TextBox { Left = 50, Top = 150, Width = 400, PasswordChar = '*' };

            Label confirmLabel = new Label { Left = 50, Top = 180, Text = "Confirm password:", Width = 400 };
            TextBox confirmTextBox = new TextBox { Left = 50, Top = 210, Width = 400, PasswordChar = '*' };

            Button confirmation = new Button { Text = "OK", Left = 350, Width = 100, Top = 240 };
            confirmation.Click += (sender, e) =>
            {
                if (textBox.Text == confirmTextBox.Text)
                {
                    input = textBox.Text;
                    prompt.Close();
                }
                else
                {
                    MessageBox.Show("Passwords do not match. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox.Clear();
                    confirmTextBox.Clear();
                    textBox.Focus();
                }
            };

            prompt.Controls.Add(headerImage);
            prompt.Controls.Add(label);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmLabel);
            prompt.Controls.Add(confirmTextBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            Application.Run(prompt);
            return input;
        }
        private void MonitorEncryptionProgress()
            {
            while (encryptionInProgress)
            {
                bitLockerCommon.BitLockerAPI_GetConversionStatus(out uint conversionStatus, out string encryptionPercentage);
                if (conversionStatus == 1) // Fully encrypted
                {
                    Console.WriteLine("Encryption completed successfully.");
                    encryptionInProgress = false;
                }
                else if (conversionStatus == 2) // Encryption in progress
                { 
                    Console.WriteLine($"Encryption in progress: {encryptionPercentage}% complete.");
                }
                else
                {
                    Console.WriteLine("An error occurred while checking encryption status.");
                    encryptionInProgress = false;
                }
                Thread.Sleep(5000); // Check every 5 seconds
            }
            encryptionInProgress = true;
        }
        public void GenerateERIFile(string volumeKeyProtectorID, string numericalPassword)
        {
            string sanitizedDriveLetter = driveLetter.TrimEnd(':');
            Random random = new Random();
            int randomInteger = random.Next();
            string outputFilePath = @"C:\Temp\KeyProtectors_" + sanitizedDriveLetter + randomInteger + ".xml";
            try
            {
                XElement xml = new XElement("KeyProtectorsReport",
                    new XElement("DriveDetails",
                    new XElement("DriveLetter", sanitizedDriveLetter),
                    new XElement("ProtectorID", volumeKeyProtectorID),
                    new XElement("NumericalPassword", numericalPassword)
                    )
                    );
                xml.Save(outputFilePath);
                Console.WriteLine($"XML file successfully created at {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private void ShowProgressDialog(string title)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form progressForm = new Form
            {
                Width = 400,
                Height = 200,
                Text = title, // Set the title dynamically
                StartPosition = FormStartPosition.CenterScreen
            };

            Label statusLabel = new Label
            {
                Left = 30,
                Top = 30,
                Width = 340,
                Text = "Encryption in progress: 0% complete."
            };

            ProgressBar progressBar = new ProgressBar
            {
                Left = 30,
                Top = 60,
                Width = 340,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            Button closeButton = new Button
            {
                Text = "Close",
                Left = 150,
                Top = 100,
                Width = 100,
                Enabled = false // Disable until encryption completes
            };

            closeButton.Click += (sender, e) =>
            {
                progressForm.Close();
            };

            progressForm.Controls.Add(statusLabel);
            progressForm.Controls.Add(progressBar);
            progressForm.Controls.Add(closeButton);

            Task.Run(() =>
            {
                while (true)
                {
                    // Simulate checking encryption status
                    bitLockerCommon.BitLockerAPI_GetConversionStatus(out uint conversionStatus, out string encryptionPercentage);

                    progressForm.Invoke(new Action(() =>
                    {
                        if (conversionStatus == 1) // Fully encrypted
                        {
                            statusLabel.Text = "Encryption completed successfully.";
                            progressBar.Value = 100;
                            closeButton.Enabled = true;
                        }
                        else if (conversionStatus == 2) // Encryption in progress
                        {
                            statusLabel.Text = $"Encryption in progress: {encryptionPercentage}% complete.";
                            progressBar.Value = int.Parse(encryptionPercentage);
                        }
                        else if (conversionStatus == 999) // Error
                        {
                            statusLabel.Text = "An error occurred during encryption.";
                            closeButton.Enabled = true;
                        }
                    }));

                    if (conversionStatus == 1 || conversionStatus == 999) break;
                    Thread.Sleep(5000); // Wait for 5 seconds
                }
            });
            Application.Run(progressForm);
        }
    }    
}

