using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

using System.Runtime.InteropServices;

using System.Data.SqlClient;

using System.Diagnostics;


using MySql.Data.MySqlClient;

using AForge.Video;

using System.Threading;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace GuiButItsFinalNow
{
    public partial class Form1 : Form
    {

        //-------------------------------------------------------------------------------------DECLARE GLOBAL VARIABLES HERE

        public static int CurrentIndex;

        public static bool IsEnteringKey;

        public static string TrainingType = "";
        public static string TrainingParameters = "";

        public static List<string> FacesInAllFaces = new List<string>();

        public static MJPEGStream Camera1Stream;
        public static MJPEGStream Camera2Stream;
        public static MJPEGStream Camera3Stream;
        public static MJPEGStream Camera4Stream;
        public static MJPEGStream Camera5Stream;
        public static MJPEGStream Camera6Stream;
        public static MJPEGStream Camera7Stream;
        public static MJPEGStream Camera8Stream;

        public static int IntroInterval = 5000;

        public static Process VideoScriptProcess = new Process();
        public static string VideoScriptFileName = @"Python\FinalVideoOnly.bat";

        public static Process RecogniserScriptProcess = new Process();
        public static string RecogniserScriptFileName = @"Python\Recogniser.bat";

        public static Process EmpScriptProcess = new Process();
        public static string EmpScriptFileName = @"Python\AddNewEmployee.bat";

        public static Process EditEmpScriptProcess = new Process();
        public static string EditEmpScriptFileName = @"Python\EditEmployee.bat";

        public static Process KillPythonProcess = new Process();
        public static string KillPythonFileName = @"Python\EndAllPythonProcesses.bat";

        public static Process DeleteFacesProcess = new Process();
        public static string DeleteFacesFileName = @"Python\DeleteAllFaces.bat";

        public static Process ExportScriptProcess = new Process();
        public static string ExportScriptFileName = @"Python\ExportData.bat";

        public static Process RetrainScriptProcess = new Process();
        public static string RetrainScriptFileName = @"Python\Retrain.bat";

        public static Process MarkAbsenteesScriptProcess = new Process();
        public static string MarkAbsenteesScriptFileName = @"Python\MarkAbsentees.bat";

        public static Process DBmProcess = new Process();
        public static string DbmScriptFileName = @"Python\DbM.bat";

        /*
        public static Process Save1 = new Process();
        public static string Save1ScriptFileName = @"Python\save1.bat";

        public static Process Save2 = new Process();
        public static string Save2ScriptFileName = @"Python\save2.bat";
        */

        public DateTime ExpiryDate;

        public static bool HasDbBeenSetup = false;
        public static bool MouseInLogs = false;

        public static string MainPassword = "";

        public static string ThisPcMAC = "";

        public static string ServerName, DBname, Uid, Pwd, ConnString;

        public static MySqlConnection MainConn;

        public bool IsScrollingLogsDataGridView = false;

        public List<Image> Last100ImagesInDescOrder = new List<Image>();


        public static bool TrainingSuccess = false;

        public static List<Image> LogsImages = new List<Image>();

        public static int NumberOfCameras;

        public void RunMarkAbsenteesScript()
        {
            try
            {
                ProcessStartInfo MarkAbsenteesScriptStartInfo = new ProcessStartInfo();
                MarkAbsenteesScriptStartInfo.FileName = MarkAbsenteesScriptFileName;
                MarkAbsenteesScriptStartInfo.CreateNoWindow = false;
                MarkAbsenteesScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                MarkAbsenteesScriptProcess.StartInfo = MarkAbsenteesScriptStartInfo;
                MarkAbsenteesScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "My name is Khan";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }              

        public static DateTime GetDateTime()
        {
            DateTime dateTime = DateTime.MinValue;
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://www.microsoft.com");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string todaysDates = response.Headers["date"];

                dateTime = DateTime.ParseExact(todaysDates, "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeUniversal);
            }

            return dateTime;
        }


        public void CheckIfExpired()
        {
            string TargetExpiryDate = "", KeyData = "";
            DateTime ExpiryDateFromCurrentKey;

            string ReadMAC = "";

            //USE THIS LINE FOR LOCAL DATE TIME
            DateTime CurrentDateTime = DateTime.Now;

            //USE THIS LINE FOR DATE TIME FROM THE INTERNET
            //DateTime CurrentDateTime = GetDateTime();

            using (StreamReader EDateTextFile = new StreamReader(@"WinformConfig\CurrentKey.txt"))
            {
                KeyData = EDateTextFile.ReadLine();
                //ReadMAC = EDateTextFile.ReadLine();
                EDateTextFile.Close();
            }

            KeyData = Decrypt(KeyData);

            string[] KeySplitData = KeyData.Split('+');

            if (KeySplitData.Length == 2)
            { 
            TargetExpiryDate = KeySplitData[0];
            ReadMAC = KeySplitData[1];

            if (ReadMAC != GetMACAddress2())
            {
                MessageBox.Show("Key is not activated for this computer");
                OnNewKey();
            }

            if (DateTime.TryParse(TargetExpiryDate, out ExpiryDateFromCurrentKey))
            {
                if (CurrentDateTime > ExpiryDateFromCurrentKey)
                {
                    IsEnteringKey = true;
                    MessageBox.Show("Your key has expired, please enter a new key");
                    OnNewKey();

                }
            }
            else
            {
                IsEnteringKey = true;
                MessageBox.Show("Invalid key");
                OnNewKey();

            }
        }
            else
            {
                IsEnteringKey = true;
                MessageBox.Show("Invalid Key");
                OnNewKey();
            }


        }

        public static Point KeyPromptLocation;
        public static bool StartKeyPromptAtCentre = false;

        public void OnNewKey()
        {

            IsEnteringKey = true;
            string Val;

            using (KeyPrompt NewPasswordPrompt = new KeyPrompt(StartKeyPromptAtCentre ,KeyPromptLocation))
            {
                var Result = NewPasswordPrompt.ShowDialog();
                Val = NewPasswordPrompt.ReturnValue1;
            }

            using (StreamWriter EDateTextFile = new StreamWriter(@"WinformConfig\CurrentKey.txt"))
            {
                EDateTextFile.WriteLine(Val);
                EDateTextFile.WriteLine(ThisPcMAC);
                EDateTextFile.Flush();
                EDateTextFile.Close();
            }
            IsEnteringKey = false;

            CheckIfExpired();

            /*

            string NewDate = Decrypt(Val);

            DateTime ExpiryDateFromCurrentKey;


            //USE THIS LINE FOR LOCAL DATE TIME
            DateTime CurrentDateTime = DateTime.Now;

            //USE THIS LINE FOR DATE TIME FROM THE INTERNET
            //DateTime CurrentDateTime = GetDateTime();

            if (DateTime.TryParse(NewDate, out ExpiryDateFromCurrentKey))
            {
                if (CurrentDateTime > ExpiryDateFromCurrentKey)
                {
                    MessageBox.Show("This key has expired");
                    OnNewKey();
                }
                else
                {
                    ThisPcMAC = GetMACAddress2();
                    ThisPcMAC = Encrypt(ThisPcMAC);

                    using (StreamWriter EDateTextFile = new StreamWriter(@"WinformConfig\CurrentKey.txt"))
                    {
                        EDateTextFile.WriteLine(Val);
                        EDateTextFile.WriteLine(ThisPcMAC);
                        EDateTextFile.Flush();
                        EDateTextFile.Close();
                    }
                    IsEnteringKey = false;
                }
            }
            else
            {
                MessageBox.Show("Invalid key");
                OnNewKey();

            }
            */
        }
            
        

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "My name is Khan";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string GetMACAddress2()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    //IPInterfaceProperties properties = adapter.GetIPProperties(); Line is not required
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }

        public static void GetDbSetup()
        {
            HasDbBeenSetup = true;
            try
            {
                using (StreamReader DbFileReader = new StreamReader(@"Python\Config\DbSetup.txt"))
                {

                }
            }
            catch
            {
                HasDbBeenSetup = false;
            }

            //MessageBox.Show(HasDbBeenSetup.ToString());

        }

        public void GetAllEmployeesForExport()
        {
            if (HasDbBeenSetup)
            {
                string AllEmployeesCommandString = "select Full_Name,ID from employees";
                //MessageBox.Show(AllEmployeesCommandString);

                MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable AllEmployeesDataTable = new DataTable();
                AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                ExportDataGridView.DataSource = AllEmployeesDataTable;
            }
        }

        public void GetAllEmployees()
        {
            if (HasDbBeenSetup)
            {
                string AllEmployeesCommandString = "select Full_Name,ID from employees";
                //MessageBox.Show(AllEmployeesCommandString);

                MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable AllEmployeesDataTable = new DataTable();
                AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);
                
                AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
            }
        }

        public void GetLogs()
        {
            if(HasDbBeenSetup)
            { 
                string TDate = DateTime.Now.ToString("yyyy/M/d");

                //WITH STATUS
                //string GetLogsCommandString = "select ImageNumber,Cam,Name,Status,ID,InTime,OutTime from people where InTime>'" + TDate + "' order by (-ImageNumber)";

                //WITHOUT STATUS
                string GetLogsCommandString = "select ImageNumber,Cam,Name,ID,InTime,OutTime from people where InTime>'" + TDate + "' order by (-ImageNumber)";

                //MessageBox.Show(GetLogsCommandString);
                MySqlCommand GetLogsCommand = new MySqlCommand(GetLogsCommandString, MainConn);

                MySqlDataAdapter GetLogsDataAdapter = new MySqlDataAdapter(GetLogsCommand);
                GetLogsDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable GetLogsDataTable = new DataTable();
                GetLogsDataAdapter.Fill(GetLogsDataTable);

                LogsDataGridView.DataSource = GetLogsDataTable;
                //LogsDataGridView.Columns[1].HeaderText = "Cam";

                foreach (DataGridViewRow RowX in LogsDataGridView.Rows)

                {
                    try
                    {
                        if (RowX.Cells[1].Value.ToString() == "None")
                        {
                            LogsDataGridView.Rows.Remove(RowX);
                        }
                    }
                    catch
                    {

                    }
                    

                    if (!(RowX.Cells[1].Value == null))
                    {
                        string ImageName = RowX.Cells[1].Value.ToString();
                        //Python\Images\" + ImageName + ".jpg"
                        if (ImageName != "0")
                        {
                            Image NewImage = Image.FromFile(@"Python\Images\" + ImageName + ".jpg");
                            RowX.Cells[0].Value = NewImage;
                        }                       
                    
                    }
                }

                LogsDataGridView.Columns.RemoveAt(1);

                
            }
            
        }

        public void CheckSqlCOnnection()
        {
            try
            {
                string TDate = DateTime.Now.ToString("yyyy/M/d");

                string GetLogsCommandString = "select Cam,Name,Status,ID,InTime,OutTime from people where InTime>'" + TDate + "' order by (-ImageNumber)";
                //MessageBox.Show(GetLogsCommandString);
                MySqlCommand GetLogsCommand = new MySqlCommand(GetLogsCommandString, MainConn);

                MySqlDataAdapter GetLogsDataAdapter = new MySqlDataAdapter(GetLogsCommand);
                GetLogsDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable GetLogsDataTable = new DataTable();
                GetLogsDataAdapter.Fill(GetLogsDataTable);
            }
            catch
            {
                OnIncorrentSqlConfig();
            }
        }

        public void ReadConfig()     //FINALISED
        {
            GetDbSetup();
            if (HasDbBeenSetup)
            {
                using (StreamReader HostFileReader = new StreamReader(@"Python\Config\host.txt"))
                {
                    ServerName = HostFileReader.ReadLine();
                    HostFileReader.Close();
                }

                using (StreamReader UserFileReader = new StreamReader(@"Python\Config\user.txt"))
                {
                    Uid = UserFileReader.ReadLine();
                    UserFileReader.Close();
                }

                using (StreamReader PasswordFileReader = new StreamReader(@"Python\Config\password.txt"))
                {
                    Pwd = PasswordFileReader.ReadLine();
                    PasswordFileReader.Close();
                }

                try
                {
                    DBname = "face__recog";
                    ConnString = @"Server=" + ServerName + ";Database=" + DBname + ";Uid=" + Uid + ";Pwd=" + Pwd + ";";
                    MainConn = new MySqlConnection(ConnString);
                    MainConn.Open();
                    //MessageBox.Show("Database connected");
                    SqlErrorLabel.Visible = false;
                    
                }
                catch
                {
                    //MessageBox.Show("Incorrect SQL configuration...");
                    HasDbBeenSetup = false;
                    OnIncorrentSqlConfig();
                    
                }

            }
            else
            {
                LoadDbSetupScreen();
            }
        }


        public static int NewIndex;
        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if (MainTabControl.SelectedIndex != 0)
            {
                /*
                if (MainTabControl.SelectedIndex == 1)
                {
                    EmployeesTabControl.Visible = false;
                }
                else if(MainTabControl.SelectedIndex == 2)
                {
                    manageCamerasContentPanel.Visible = false;
                }
                else if (MainTabControl.SelectedIndex == 3)
                {
                    DatabaseSetupContentPanel.Visible = false;
                }
                else if (MainTabControl.SelectedIndex == 4)
                {
                    ExportDataCOntentPanel.Visible = false;
                }
                else if (MainTabControl.SelectedIndex == 5)
                {
                    SettingsContentPanel.Visible = false;
                    SettingsLeftPanel.Visible = false;
                }        
                */



                string Val;
                using (PasswordPromptMessageBox NewPasswordPrompt = new PasswordPromptMessageBox())
                {
                    var Result = NewPasswordPrompt.ShowDialog();
                    Val = NewPasswordPrompt.ReturnValue1;
                }
                if (Val == MainPassword)
                {
                    if (MainTabControl.SelectedIndex == 1)
                    {
                        EmployeesTabControl.Visible = true;
                    }
                    else if (MainTabControl.SelectedIndex == 2)
                    {
                        ManageCamerasContentPanel.Visible = true;
                    }
                    else if (MainTabControl.SelectedIndex == 4)
                    {
                        DatabaseSetupContentPanel.Visible = true;
                    }
                    else if (MainTabControl.SelectedIndex == 3)
                    {
                        ExportDataCOntentPanel.Visible = true;
                        GetAllEmployeesForExport();
                    }
                    else if (MainTabControl.SelectedIndex == 5)
                    {
                        SettingsContentPanel.Visible = true;
                        //SettingsLeftPanel.Visible = true;
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect password");
                    MainTabControl.SelectedIndex = 0;
                }
            }
        }

        public void LoadPreviousSettings()
        {
            using (StreamReader ScreenSizeTextFile = new StreamReader(@"Python\Config\ScreenSize.txt"))
            {
                VideoScreenSizeComboBox.SelectedItem = ScreenSizeTextFile.ReadLine();
                ScreenSizeTextFile.Close();
            }
        }

        public static int TickNumber=0;
        private void MainTimer_Tick(object sender, EventArgs e)
        {                       
                if (MainTabControl.SelectedIndex == 0)
                {
                    if (!(MouseInLogs))
                    {
                        GetLogs();
                        GetAllFaces();
                    }
                }


            
            
        }              

        private void LogsDataGridView_MouseLeave(object sender, EventArgs e)
        {
            MouseInLogs = false;
            LogsNotUpdatingLabel.Text = "";
        }

        private void LogsDataGridView_MouseEnter(object sender, EventArgs e)
        {
            MouseInLogs = true;
            LogsNotUpdatingLabel.Text = "Logs are not live, remove cursor from table area to show live logs!";
        }

        private void Camera1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();

            Camera1Access1CheckBox.Enabled = Camera1CheckBox.Checked;
            Camera1Access2CheckBox.Enabled = Camera1CheckBox.Checked;
            Camera1Access3CheckBox.Enabled = Camera1CheckBox.Checked;
            Camera1Access4CheckBox.Enabled = Camera1CheckBox.Checked;

            if (Camera1CheckBox.Checked == true)
            {
                Camera1TextBox.Enabled = true;
                Camera1TextBox.Text = "";
            }
            else
            {
                Camera1TextBox.Enabled = false;
                Camera1TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();

            Camera2Access1CheckBox.Enabled = Camera2CheckBox.Checked;
            Camera2Access2CheckBox.Enabled = Camera2CheckBox.Checked;
            Camera2Access3CheckBox.Enabled = Camera2CheckBox.Checked;
            Camera2Access4CheckBox.Enabled = Camera2CheckBox.Checked;

            if (Camera2CheckBox.Checked == true)
            {
                Camera2TextBox.Enabled = true;
                Camera2TextBox.Text = "";
            }
            else
            {
                Camera2TextBox.Enabled = false;
                Camera2TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();

            Camera3Access1CheckBox.Enabled = Camera3CheckBox.Checked;
            Camera3Access2CheckBox.Enabled = Camera3CheckBox.Checked;
            Camera3Access3CheckBox.Enabled = Camera3CheckBox.Checked;
            Camera3Access4CheckBox.Enabled = Camera3CheckBox.Checked;

            if (Camera3CheckBox.Checked == true)
            {
                Camera3TextBox.Enabled = true;
                Camera3TextBox.Text = "";
            }
            else
            {
                Camera3TextBox.Enabled = false;
                Camera3TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();

            Camera4Access1CheckBox.Enabled = Camera4CheckBox.Checked;
            Camera4Access2CheckBox.Enabled = Camera4CheckBox.Checked;
            Camera4Access3CheckBox.Enabled = Camera4CheckBox.Checked;
            Camera4Access4CheckBox.Enabled = Camera4CheckBox.Checked;

            if (Camera4CheckBox.Checked == true)
            {
                Camera4TextBox.Enabled = true;
                Camera4TextBox.Text = "";                
            }
            else
            {
                Camera4TextBox.Enabled = false;
                Camera4TextBox.Text = "Device Index / IP Address";
            }
        }


        /*
        private void Camera5CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Camera5CheckBox.Checked == true)
            {
                Camera5TextBox.Enabled = true;
                Camera5TextBox.Text = "";
            }
            else
            {
                Camera5TextBox.Enabled = false;
                Camera5TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera6CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Camera6CheckBox.Checked == true)
            {
                Camera6TextBox.Enabled = true;
                Camera6TextBox.Text = "";
            }
            else
            {
                Camera6TextBox.Enabled = false;
                Camera6TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera7CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Camera7CheckBox.Checked == true)
            {
                Camera7TextBox.Enabled = true;
                Camera7TextBox.Text = "";
            }
            else
            {
                Camera7TextBox.Enabled = false;
                Camera7TextBox.Text = "Device Index / IP Address";
            }
        }

        private void Camera8CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Camera8CheckBox.Checked == true)
            {
                Camera8TextBox.Enabled = true;
                Camera8TextBox.Text = "";
            }
            else
            {
                Camera8TextBox.Enabled = false;
                Camera8TextBox.Text = "Device Index / IP Address";
            }
        }
        */
        private void CamerasSaveButton_Click(object sender, EventArgs e)
        {
            int NumberOfCameras = 0;
            bool Rewrite = false;
            string Accesses = "";

            List<string> Lines = new List<string>();

            if (Camera1TextBox.Text == "")
            {
                Camera1CheckBox.Checked = false;                
            }
            else if (Camera1CheckBox.Checked == true)
            {                
                Rewrite = true;
                NumberOfCameras++;
                Lines.Add("Camera 1: " + Camera1TextBox.Text);
                Accesses = "Camera 1 Access: ";

                if (Camera1Access1CheckBox.Checked == true)
                {
                    Accesses += "1,";
                }
                if (Camera1Access2CheckBox.Checked == true)
                {
                    Accesses += "2,";
                }
                if (Camera1Access3CheckBox.Checked == true)
                {
                    Accesses += "3,";
                }
                if (Camera1Access4CheckBox.Checked == true)
                {
                    Accesses += "4,";
                }

                Lines.Add(Accesses);
            }
            if (Camera2TextBox.Text == "")
            {
                Camera2CheckBox.Checked = false;
            }
            else if (Camera2CheckBox.Checked == true)
            {
                Rewrite = true;
                NumberOfCameras++;
                Lines.Add("Camera 2: " + Camera2TextBox.Text);
                Accesses = "Camera 2 Access: ";

                if (Camera2Access1CheckBox.Checked == true)
                {
                    Accesses += "1,";
                }
                if (Camera2Access2CheckBox.Checked == true)
                {
                    Accesses += "2,";
                }
                if (Camera2Access3CheckBox.Checked == true)
                {
                    Accesses += "3,";
                }
                if (Camera2Access4CheckBox.Checked == true)
                {
                    Accesses += "4,";
                }

                Lines.Add(Accesses);
            }
            if (Camera3TextBox.Text == "")
            {
                Camera3CheckBox.Checked = false;
            }
            else if (Camera3CheckBox.Checked == true)
            {
                Rewrite = true;
                NumberOfCameras++;
                Lines.Add("Camera 3: " + Camera3TextBox.Text);
                Accesses = "Camera 3 Access: ";

                if (Camera3Access1CheckBox.Checked == true)
                {
                    Accesses += "1,";
                }
                if (Camera3Access2CheckBox.Checked == true)
                {
                    Accesses += "2,";
                }
                if (Camera3Access3CheckBox.Checked == true)
                {
                    Accesses += "3,";
                }
                if (Camera3Access4CheckBox.Checked == true)
                {
                    Accesses += "4,";
                }

                Lines.Add(Accesses);
            }
            if (Camera4TextBox.Text == "")
            {
                Camera4CheckBox.Checked = false;
            }
            else if (Camera4CheckBox.Checked == true)
            {
                Rewrite = true;
                NumberOfCameras++;
                Lines.Add("Camera 4: " + Camera4TextBox.Text);
                Accesses = "Camera 4 Access: ";

                if (Camera4Access1CheckBox.Checked == true)
                {
                    Accesses += "1,";
                }
                if (Camera4Access2CheckBox.Checked == true)
                {
                    Accesses += "2,";
                }
                if (Camera4Access3CheckBox.Checked == true)
                {
                    Accesses += "3,";
                }
                if (Camera4Access4CheckBox.Checked == true)
                {
                    Accesses += "4,";
                }

                Lines.Add(Accesses);
            }

            if (Rewrite)
            {
                using (StreamWriter WriteCamerasTextFile = new StreamWriter(@"Python\Config\Cameras.txt"))
                {
                    WriteCamerasTextFile.WriteLine(NumberOfCameras.ToString());
                    WriteCamerasTextFile.Flush();

                    foreach (string L in Lines)
                    {
                        WriteCamerasTextFile.WriteLine(L);
                        WriteCamerasTextFile.Flush();
                    }
                    WriteCamerasTextFile.Close();

                }             

                using(StreamReader CheckCamerasTextFile = new StreamReader(@"Python\Config\Cameras.txt"))
                {
                    string CheckLine;

                    CheckLine = CheckCamerasTextFile.ReadLine();
                    if (CheckLine != NumberOfCameras.ToString())
                    {
                        CameraConfigurationSaveStatusLabel.Text = "Save Error!";
                        return;
                    }
                    else
                    {
                        for(int i = 0; i <(2*NumberOfCameras); i++)
                        {                            
                            CheckLine = CheckCamerasTextFile.ReadLine();
                            if (CheckLine != Lines[i])
                            {
                                CameraConfigurationSaveStatusLabel.Text = "Save Error!";
                                return;
                            }
                        }

                        CameraConfigurationSaveStatusLabel.Text = "Saved Sucessfully!";
                    }
                }
            }

            }

        public void GetPassword()
        {
            using (StreamReader PasswordTextFile = new StreamReader(@"WinformConfig\Password.txt"))
            {
                MainPassword = PasswordTextFile.ReadLine();
                PasswordTextFile.Close();
            }
            //`MessageBox.Show(MainPassword);
        }
        public void LoadPreviousCameraConfiguration()
        {
            using (StreamReader ReadCamerasTextFile = new StreamReader(@"Python\Config\Cameras.txt"))
            {
                string CameraNameAndSource, CameraAccess;
                int CameraNumber;

                if (Int32.TryParse(ReadCamerasTextFile.ReadLine(), out NumberOfCameras))
                {
                    for (int i = 0; i < NumberOfCameras; i++)
                    {
                        CameraNameAndSource = ReadCamerasTextFile.ReadLine();
                        CameraAccess = ReadCamerasTextFile.ReadLine();

                        string[] SeperatedNameAndSource = CameraNameAndSource.Split(new string[] { ":" }, 2, StringSplitOptions.None);
                        string[] SeperatedFormattedAccesses = CameraAccess.Split(new string[] { ":" }, 2, StringSplitOptions.None);
                        string[] SeperatedAccesses = SeperatedFormattedAccesses[1].Split(',');

                        char CNo = SeperatedNameAndSource[0][SeperatedNameAndSource[0].Length - 1];
                        //String[] SeperatedName = CameraName.Split(' ');
                        //char[] CNo = SeperatedNameAndSource[0].ToCharArray();

                        CameraNumber = Int32.Parse(CNo.ToString());

                        if (CameraNumber == 1)
                        {
                            Camera1CheckBox.Checked = true;
                            Camera1TextBox.Text = SeperatedNameAndSource[1];

                            foreach(string Access in SeperatedAccesses)
                            {                                
                                if (Access.Replace(" ", "") == "1")
                                {
                                    Camera1Access1CheckBox.Checked = true;
                                }
                                else if (Access == "2")
                                {
                                    Camera1Access2CheckBox.Checked = true;
                                }
                                else if (Access == "3")
                                {
                                    Camera1Access3CheckBox.Checked = true;
                                }
                                else if (Access == "4")
                                {
                                    Camera1Access4CheckBox.Checked = true;
                                }
                            }
                        }
                        else if (CameraNumber == 2)
                        {
                            Camera2CheckBox.Checked = true;
                            Camera2TextBox.Text = SeperatedNameAndSource[1];

                            foreach (string Access in SeperatedAccesses)
                            {
                                if (Access.Replace(" ", "") == "1")
                                {
                                    Camera2Access1CheckBox.Checked = true;
                                }
                                else if (Access == "2")
                                {
                                    Camera2Access2CheckBox.Checked = true;
                                }
                                else if (Access == "3")
                                {
                                    Camera2Access3CheckBox.Checked = true;
                                }
                                else if (Access == "4")
                                {
                                    Camera2Access4CheckBox.Checked = true;
                                }
                            }
                        }
                        else if (CameraNumber == 3)
                        {
                            Camera3CheckBox.Checked = true;
                            Camera3TextBox.Text = SeperatedNameAndSource[1];

                            foreach (string Access in SeperatedAccesses)
                            {
                                if (Access.Replace(" ", "") == "1")
                                {
                                    Camera3Access1CheckBox.Checked = true;
                                }
                                else if (Access == "2")
                                {
                                    Camera3Access2CheckBox.Checked = true;
                                }
                                else if (Access == "3")
                                {
                                    Camera3Access3CheckBox.Checked = true;
                                }
                                else if (Access == "4")
                                {
                                    Camera3Access4CheckBox.Checked = true;
                                }
                            }
                        }
                        else if (CameraNumber == 4)
                        {
                            Camera4CheckBox.Checked = true;
                            Camera4TextBox.Text = SeperatedNameAndSource[1];

                            foreach (string Access in SeperatedAccesses)
                            {
                                if (Access.Replace(" ", "") == "1")
                                {
                                    Camera4Access1CheckBox.Checked = true;
                                }
                                else if (Access == "2")
                                {
                                    Camera4Access2CheckBox.Checked = true;
                                }
                                else if (Access == "3")
                                {
                                    Camera4Access3CheckBox.Checked = true;
                                }
                                else if (Access == "4")
                                {
                                    Camera4Access4CheckBox.Checked = true;
                                }
                            }
                        }


                        //MessageBox.Show(CameraNumber.ToString());

                    }
                }
                ReadCamerasTextFile.Close();

            }
            
        }
        public void LoadDbSetupScreen()
        {
            MessageBox.Show("You will be taken to the Database Setup Screen");
            bool Bbreak = false;            
            string Val;

            while (!Bbreak)
            {
                using (PasswordPromptMessageBox NewPasswordPrompt = new PasswordPromptMessageBox())
                {
                    var Result = NewPasswordPrompt.ShowDialog();
                    Val = NewPasswordPrompt.ReturnValue1;
                }
                if (Val == MainPassword)
                {
                    MainTabControl.SelectedIndex = 4;
                    Bbreak = true;
                }
                else
                {
                    MessageBox.Show("Incorrect password");
                }
            }
            
        }

        private void AllEmployeesDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (AllEmployeesDataGridView.CurrentCell.ColumnIndex.Equals(1))
            {                
                ViewEmployee(AllEmployeesDataGridView.CurrentCell.Value.ToString());                         
            }
            else
            {
                ViewEmployee(AllEmployeesDataGridView.Rows[AllEmployeesDataGridView.CurrentCell.RowIndex].Cells[1].Value.ToString());
            }

        }

        public void ViewEmployee(string EID)
        {
            if (HasDbBeenSetup)
            {
                ViewEmployeeDeleteLabel.Visible = false;
                string VeDummyCommandString = "select * from employees where ID='" + EID + "'";

                MySqlCommand VeDummyCommand = new MySqlCommand(VeDummyCommandString, MainConn);

                MySqlDataAdapter VeDummyDataAdapter = new MySqlDataAdapter(VeDummyCommand);
                VeDummyDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable VeDummyDataTable = new DataTable();
                VeDummyDataAdapter.Fill(VeDummyDataTable);

                EmployeePersonalDataGridView.DataSource = VeDummyDataTable;


                
                string ID = EID;
                string Title = EmployeePersonalDataGridView.Rows[0].Cells[1].Value.ToString();

                string FullName = EmployeePersonalDataGridView.Rows[0].Cells[2].Value.ToString();

                string[] Names = FullName.Split(' ');
                string FirstName = Names[0];
                string LastName = Names[1];

                string DateOfBirth = EmployeePersonalDataGridView.Rows[0].Cells[3].Value.ToString();
                string[] Dates = DateOfBirth.Split(' ');
                DateOfBirth = Dates[0];

                string Gender = EmployeePersonalDataGridView.Rows[0].Cells[4].Value.ToString();
                string EmploymentStatus = EmployeePersonalDataGridView.Rows[0].Cells[5].Value.ToString();
                string Mobile1 = EmployeePersonalDataGridView.Rows[0].Cells[6].Value.ToString();
                string Mobile2 = EmployeePersonalDataGridView.Rows[0].Cells[7].Value.ToString();
                string WorkEmail = EmployeePersonalDataGridView.Rows[0].Cells[8].Value.ToString();
                string Address = EmployeePersonalDataGridView.Rows[0].Cells[9].Value.ToString();

                VeEid.Text = EID;
                VeLvl.Text = Title;
                VeFn.Text = FirstName;
                VeLn.Text = LastName;
                VeDOB.Text = DateOfBirth;
                VeGen.Text = Gender;
                VeEs.Text = EmploymentStatus;
                VeM1.Text = Mobile1;
                VeM2.Text = Mobile2;
                VeWe.Text = WorkEmail;
                VeAdd.Text = Address;
                try
                {
                    VeFFpPicBox.Image = Image.FromFile(@"Python\EmployeeImages\" + EID + ".jpg");
                }
                catch
                {

                }
                VeFFpPicBox.SizeMode = PictureBoxSizeMode.StretchImage;





                string ViewEmployeeCommandString = "select Cam,Name,Status,ID,InTime,OutTime from people where ID='" + EID + "' order by (-ImageNumber)";

                MySqlCommand ViewEmployeeCommand = new MySqlCommand(ViewEmployeeCommandString, MainConn);

                MySqlDataAdapter ViewEmployeeDataAdapter = new MySqlDataAdapter(ViewEmployeeCommand);
                ViewEmployeeDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable ViewEmployeeDataTable = new DataTable();
                ViewEmployeeDataAdapter.Fill(ViewEmployeeDataTable);

                EmployeePersonalDataGridView.DataSource = ViewEmployeeDataTable;







                EmployeesTabControl.SelectedIndex = 1;

                //MessageBox.Show("gettting details for " + EID);
            }
        }

        private void TrainByImageCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TrainByImageCheckBox.Checked == true)
            {
                TrainByVideoCheckBox.Checked = false;
                ChooseImageDirectoryButton.Enabled = true;
            }
            else
            {
                ChooseImageDirectoryButton.Enabled = false;
            }
            
            
        }

        private void TrainByVideoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TrainByVideoCheckBox.Checked == true)
            {
                TrainByImageCheckBox.Checked = false;
                TrainByExistingVideoCheckBox.Enabled = true;
                TrainByCameraCheckBox.Enabled = true;
            }
            else
            {
                TrainByExistingVideoCheckBox.Enabled = false;
                TrainByCameraCheckBox.Enabled = false;
            }
        }

        public void GetLevels()
        {            
            using (StreamReader LevelsTextFile = new StreamReader(@"Python\Config\Levels.txt"))
            {
                string NewLevel = "";
                LevelsRTB.Clear();
                AddEmployeeTitleComboBox.Items.Clear();
                AllEmpLvlComboBox.Items.Clear();
                AllEmpLvlComboBox.Items.Add("Any");
                ExportLvlComboBox.Items.Add("Any");
                while (!LevelsTextFile.EndOfStream)
                {
                    NewLevel = LevelsTextFile.ReadLine();
                    AddEmployeeTitleComboBox.Items.Add(NewLevel);
                    AllEmpLvlComboBox.Items.Add(NewLevel);
                    ExportLvlComboBox.Items.Add(NewLevel);
                    LevelsRTB.AppendText(NewLevel + "\r\n");
                }
                LevelsTextFile.Close();                
            }

            ExportLvlComboBox.SelectedIndex = 0;
            AllEmpLvlComboBox.SelectedIndex = 0;
            AddEmployeeTitleComboBox.SelectedIndex = 0;
            AddEmployeeGenderComboBox.SelectedIndex = 0;
            AddEmployeeEsComboBox.SelectedIndex = 0;
            GetSettings();
        }

        public void SaveLevels()
        {

        }

        private void TrainByCameraCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TrainByCameraCheckBox.Checked == true)
            {
                TrainByExistingVideoCheckBox.Checked = false;
                ChooseVideoButton.Enabled = false;
                TrainByCameraTextBox.Text = "";
                TrainByCameraTextBox.Enabled = true;
            }
            else
            {
                TrainByCameraTextBox.Enabled = false;
                TrainByCameraTextBox.Text = "Device Index / IP Address";
            }
        }

        private void TrainByExistingViewCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TrainByExistingVideoCheckBox.Checked == true)
            {
                TrainByCameraCheckBox.Checked = false;
                ChooseVideoButton.Enabled = true;
            }
            else
            {
                ChooseVideoButton.Enabled = false;
            }
        }

        private void ChooseImageDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog SelectImageDirectoryOFd = new FolderBrowserDialog())
            {
                DialogResult result = SelectImageDirectoryOFd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(SelectImageDirectoryOFd.SelectedPath))
                {
                    TrainByImageTextBox.Text = SelectImageDirectoryOFd.SelectedPath;                    
                }
            }
        }

        private void ChooseVideoButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog VideoOfd=new OpenFileDialog())
            {
                VideoOfd.Filter = "Videos(*.mp4)|*.mp4";
                if (VideoOfd.ShowDialog() == DialogResult.OK)
                {                 
                    TrainByExistingVideoTextBox.Text= VideoOfd.FileName;
                }
            }
        }

        

        public bool CheckAndWriteTrainingData()
        {
            if (TrainByImageCheckBox.Checked == true && TrainByImageTextBox.Text != "")
            {
                    TrainingType = "Images";
                  TrainingParameters = TrainByImageTextBox.Text.Replace('\\','/');
               TrainingParameters = TrainByImageTextBox.Text.Replace("\n", string.Empty);
                //RunSave1Script();
                return true;
                
            }
            else if (TrainByVideoCheckBox.Checked = true && TrainByExistingVideoCheckBox.Checked == true)
            {
                if (TrainByExistingVideoTextBox.Text == "")
                {
                    MessageBox.Show("Select a video");
                    return false;
                }
                else
                {
                    TrainingType = "Video";
                    TrainingParameters = TrainByExistingVideoTextBox.Text;
                    //RunSave2Script();
                    return true;
                }
            }
            else if (TrainByVideoCheckBox.Checked = true && TrainByCameraCheckBox.Checked == true)
            {
                if (TrainByCameraTextBox.Text == "")
                {
                    MessageBox.Show("Enter camera device index");
                    return false;
                }
                else
                {
                    TrainingType = "Camera";
                    TrainingParameters = TrainByCameraTextBox.Text;
                    //RunSave2Script();
                    return true;
                }
            }
            else
            {
                MessageBox.Show("Select a training method");
                return false;
            }            
        }
        public bool CheckForEmptyFields()
        {
            if (AddEmployeeEidTextBox.Text == "")
                return true;
            else if (AddEmployeeTitleComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeFnTextBox.Text == "")
                return true;
            else if (AddEmployeeLnTextBox.Text == "")
                return true;
            else if (AddEmployeeDobDTP.Value.ToString() == "")
                return true;
            else if (AddEmployeeGenderComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeEsComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeM1TextBox.Text == "")
                return true;
            else if (AddEmployeeM2TextBox.Text == "")
                return true;
            else if (AddEmployeeWeTextBox.Text == "")
                return true;
            else if (AddEmployeeAddressTextBox.Text == "")
                return true;
            else if (FfpFilenameLabel.Text == "No file selected")
                return true;
            else if (TrainByVideoCheckBox.Checked == false && TrainByImageCheckBox.Checked == false)
                return true;
            else
                return false;
        }
        public bool CheckForEmptyFieldsWhileEditing()
        {
            if (AddEmployeeEidTextBox.Text == "")
                return true;
            else if (AddEmployeeTitleComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeFnTextBox.Text == "")
                return true;
            else if (AddEmployeeLnTextBox.Text == "")
                return true;
            else if (AddEmployeeDobDTP.Value.ToString() == "")
                return true;
            else if (AddEmployeeGenderComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeEsComboBox.SelectedItem.ToString() == "")
                return true;
            else if (AddEmployeeM1TextBox.Text == "")
                return true;
            else if (AddEmployeeM2TextBox.Text == "")
                return true;
            else if (AddEmployeeWeTextBox.Text == "")
                return true;
            else if (AddEmployeeAddressTextBox.Text == "")
                return true;
            else
                return false;
        }

        public bool CheckIfEmployeeExistsAlready(string EId)
        {
            bool ReturnValue;

            //string TDate = DateTime.Now.ToString("yyyy/M/d");

            string GetLogsCommandString = "select * from employees where ID='" + EId + "'";
            //MessageBox.Show(GetLogsCommandString);
            MySqlCommand GetLogsCommand = new MySqlCommand(GetLogsCommandString, MainConn);

            MySqlDataAdapter GetLogsDataAdapter = new MySqlDataAdapter(GetLogsCommand);
            GetLogsDataAdapter.SelectCommand.CommandType = CommandType.Text;

            DataTable GetLogsDataTable = new DataTable();
            GetLogsDataAdapter.Fill(GetLogsDataTable);

            LogsDataGridView.DataSource = GetLogsDataTable;

            if (LogsDataGridView.Rows.Count > 1)
            {
                ReturnValue = true;
                MessageBox.Show("An employee exists with the same Employee ID");
            }
            else
            {
                ReturnValue = false;
            }
            GetLogs();
            return ReturnValue;
        }

        private void TrainButton_Click(object sender, EventArgs e)
        {
          
            if (!(CheckForEmptyFields()))
            {
                if (CheckAndWriteTrainingData())
                {
                    if (!CheckIfEmployeeExistsAlready(AddEmployeeEidTextBox.Text))
                    {
                        using (StreamWriter TrainingTypeTextFile = new StreamWriter(@"Python\NewEmployee\Training.txt"))
                        {
                            TrainingTypeTextFile.WriteLine(TrainingType);
                            TrainingTypeTextFile.Flush();
                            TrainingTypeTextFile.Close();
                        }
                        using (StreamWriter TrainingParamsTextFile = new StreamWriter(@"Python\NewEmployee\Params.txt"))
                        {
                            TrainingParamsTextFile.WriteLine(TrainingParameters);
                            TrainingParamsTextFile.Flush();
                            TrainingParamsTextFile.Close();
                        }

                        using (StreamWriter DetailsTextFile = new StreamWriter(@"Python\NewEmployee\Details.txt"))
                        {
                            DetailsTextFile.WriteLine(AddEmployeeEidTextBox.Text);
                            DetailsTextFile.WriteLine(AddEmployeeTitleComboBox.SelectedItem.ToString());
                            DetailsTextFile.WriteLine(AddEmployeeFnTextBox.Text + " " + AddEmployeeLnTextBox.Text);
                            DetailsTextFile.WriteLine(AddEmployeeDobDTP.Value.ToString());
                            DetailsTextFile.WriteLine(AddEmployeeGenderComboBox.SelectedItem.ToString());
                            DetailsTextFile.WriteLine(AddEmployeeEsComboBox.SelectedItem.ToString());
                            DetailsTextFile.WriteLine(AddEmployeeM1TextBox.Text);
                            DetailsTextFile.WriteLine(AddEmployeeM2TextBox.Text);
                            DetailsTextFile.WriteLine(AddEmployeeWeTextBox.Text);
                            DetailsTextFile.WriteLine(AddEmployeeAddressTextBox.Text);

                            DetailsTextFile.Flush();
                            DetailsTextFile.Close();
                        }

                        /*
                        try
                        {
                            Bitmap NewEmployeeImage = (Bitmap)AddEmployeeFfpPictureBox.Image;
                            NewEmployeeImage.Save(@"Python\NewEmployee\Image.jpg");
                            NewEmployeeImage.Dispose();
                        }
                        catch
                        {
                            MessageBox.Show("Two employees canot have the same Front Face Photo");
                        }
                        */
                        

                        RunAddNewEmployeeScript();
                        TrainingConfirmationLabel.Text = "Click check button to see training success";
                    }
                }
                else
                {
                    MessageBox.Show("Empty Fields In Training!");
                }
            }
            else
            {
                MessageBox.Show("Empty Fields!");
            }
        }

        public void RunVideoAndRecogniserScript()
        {
            try
            {
                //string CmdCommand = "/C python C:\\Users\\Lavi\\Desktop\\GuiButItsFinalNow\\GuiButItsFinalNow\\GuiButItsFinalNow\\bin\\Debug\\Python\\FinalVideoOnly.py";
                ProcessStartInfo VideoScriptStartInfo = new ProcessStartInfo("cmd.exe", "/c " + VideoScriptFileName);
                VideoScriptStartInfo.FileName = VideoScriptFileName;
                VideoScriptStartInfo.CreateNoWindow = false;
                VideoScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                VideoScriptProcess.StartInfo = VideoScriptStartInfo;
                VideoScriptProcess.Start();

                //System.Diagnostics.Process.Start("CMD.exe", CmdCommand);

            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }

            try
            {
                ProcessStartInfo RecogniserScriptStartInfo = new ProcessStartInfo();
                RecogniserScriptStartInfo.FileName = RecogniserScriptFileName;
                RecogniserScriptStartInfo.CreateNoWindow = false;
                RecogniserScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                RecogniserScriptProcess.StartInfo = RecogniserScriptStartInfo;
                RecogniserScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }
            try
            {
                ProcessStartInfo DeleteFacesStartInfo = new ProcessStartInfo();
                DeleteFacesStartInfo.FileName = DeleteFacesFileName;
                DeleteFacesStartInfo.CreateNoWindow = false;
                DeleteFacesStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                DeleteFacesProcess.StartInfo = DeleteFacesStartInfo;
                DeleteFacesProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Deleting script");
            }
        }

        public void RunEditEmployeeScript()
        {
            try
            {
                ProcessStartInfo EditEmpScriptStartInfo = new ProcessStartInfo();
                EditEmpScriptStartInfo.FileName = EditEmpScriptFileName;
                EditEmpScriptStartInfo.CreateNoWindow = false;
                EditEmpScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                EditEmpScriptProcess.StartInfo = EditEmpScriptStartInfo;
                EditEmpScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }
        }

        /*
        public void RunSave1Script()
        {
            try
            {
                ProcessStartInfo Save1ScriptStartInfo = new ProcessStartInfo();
                Save1ScriptStartInfo.FileName = Save1ScriptFileName;
                Save1ScriptStartInfo.CreateNoWindow = false;
                Save1.StartInfo = Save1ScriptStartInfo;
                Save1.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Employee script");
            }
        }
        public void RunSave2Script()
        {
            try
            {
                ProcessStartInfo Save2ScriptStartInfo = new ProcessStartInfo();
                Save2ScriptStartInfo.FileName = Save2ScriptFileName;
                Save2ScriptStartInfo.CreateNoWindow = false;
                Save2.StartInfo = Save2ScriptStartInfo;
                Save2.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Employee script");
            }
        }
        */
        public void RunAddNewEmployeeScript()
        {
            try
            {
                ProcessStartInfo EmpScriptStartInfo = new ProcessStartInfo();
                EmpScriptStartInfo.FileName = EmpScriptFileName;          
                EmpScriptStartInfo.CreateNoWindow = false;
                EmpScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                EmpScriptProcess.StartInfo = EmpScriptStartInfo;
                EmpScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Employee script");
            }
        }

        public void RunDBmScript()
        {
            try
            {
                ProcessStartInfo DBmScriptStartInfo = new ProcessStartInfo();
                DBmScriptStartInfo.FileName = DbmScriptFileName;
                DBmScriptStartInfo.CreateNoWindow = false;
                DBmScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                DBmProcess.StartInfo = DBmScriptStartInfo;
                DBmProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run DBmloyee script");
            }
        }

        public void EditEmployee(string Eid,string Lvl,string FirstName,string LastName,string DOB,string Gender,string EmploymentStatus,string Mobile1,string Mobile2,string WorkEmail,string Address,Image FrontFacePhoto)
        {
            EmployeesTabControl.SelectedIndex = 0;
            EditModeCheckBox.Checked = true;
            EditModeCheckBox.Visible = true;
            //TrainingPanel.Visible = false;
            TrainButton.Visible = false;
            RetrainButton.Visible = true;
            EditModeSaveButton.Visible = true;
            AddEmployeeHeading.Text = "Edit Employee";

            AddEmployeeEidTextBox.Text = Eid;
            AddEmployeeTitleComboBox.SelectedItem = Lvl;
            AddEmployeeFnTextBox.Text = FirstName;
            AddEmployeeLnTextBox.Text = LastName;
            AddEmployeeDobDTP.Value = Convert.ToDateTime(DOB);
            AddEmployeeDobDTP.Value = Convert.ToDateTime(DOB);
            AddEmployeeGenderComboBox.SelectedItem = Gender;
            AddEmployeeEsComboBox.SelectedItem = EmploymentStatus;
            AddEmployeeM1TextBox.Text = Mobile1;
            AddEmployeeM2TextBox.Text = Mobile2;
            AddEmployeeWeTextBox.Text = WorkEmail;
            AddEmployeeAddressTextBox.Text = Address;
            //AddEmployeeFfpPictureBox.Image = FrontFacePhoto;
            AddEmployeeFfpPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

        }


        private void EditModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EditModeCheckBox.Checked == false)
            {
                EditModeCheckBox.Visible = false;
                EditModeSaveButton.Visible = false;
                //TrainingPanel.Visible = true;
                TrainButton.Visible = true;
                RetrainButton.Visible = false;
                //TrainButton.Text = "Retrain";
                AddEmployeeEidTextBox.Enabled = true;
                AddEmployeeHeading.Text = "Add Employee";
                AddEmployeeFfpButton.Enabled = true;

                AddEmployeeEidTextBox.Text = "";
                AddEmployeeTitleComboBox.SelectedIndex = 0;
                AddEmployeeFnTextBox.Text = "";
                AddEmployeeLnTextBox.Text = "";
                //AddEmployeeDobDTP.Value.ToString = "";
                AddEmployeeGenderComboBox.SelectedIndex = 0;
                AddEmployeeEsComboBox.SelectedIndex = 0;
                AddEmployeeM1TextBox.Text = "";
                AddEmployeeM2TextBox.Text = "";
                AddEmployeeWeTextBox.Text = "";
                AddEmployeeAddressTextBox.Text = "";

            }
        }

        private void ViewEmployeeEditButton_Click(object sender, EventArgs e)
        {
          EditEmployee(VeEid.Text,VeLvl.Text,VeFn.Text,VeLn.Text,VeDOB.Text,VeGen.Text,VeEs.Text,VeM1.Text,VeM2.Text,VeWe.Text,VeAdd.Text, VeFFpPicBox.Image);
            AddEmployeeEidTextBox.Enabled = false;
            AddEmployeeFfpButton.Enabled = false;
        }

        private void EditModeSaveButton_Click(object sender, EventArgs e)
        {
            if (!(CheckForEmptyFieldsWhileEditing()))
            {
                using (StreamWriter EditEmployeeDetailsTextFile = new StreamWriter(@"Python\EditEmployee\Details.txt"))
                {                    
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeEidTextBox.Text);
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeTitleComboBox.SelectedItem.ToString());
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeFnTextBox.Text + " " + AddEmployeeLnTextBox.Text);
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeDobDTP.Value.ToString());
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeGenderComboBox.SelectedItem.ToString());
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeEsComboBox.SelectedItem.ToString());
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeM1TextBox.Text);
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeM2TextBox.Text);
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeWeTextBox.Text);
                    EditEmployeeDetailsTextFile.WriteLine(AddEmployeeAddressTextBox.Text);
                    EditEmployeeDetailsTextFile.Flush();
                    EditEmployeeDetailsTextFile.Close();
                }

                /*
                Bitmap NewEmployeeImage = (Bitmap)AddEmployeeFfpPictureBox.Image;
                if (System.IO.File.Exists(@"Python\EmployeeImages\" + AddEmployeeEidTextBox.Text + ".jpg"))
                    System.IO.File.Delete(@"Python\EmployeeImages\" + AddEmployeeEidTextBox.Text + ".jpg");

                NewEmployeeImage.Save(@"Python\EmployeeImages\" + AddEmployeeEidTextBox.Text + ".jpg");

                NewEmployeeImage.Dispose();
                */
                RunEditEmployeeScript();
            }
            else
            {
                MessageBox.Show("Empty Fields!");
            }
        }

        public static System.DateTime LastUpdateTime = DateTime.Now;


        public static List<PictureBox> AllFacesPictureBoxes = new List<PictureBox>();

        public void GetAllFaces()
        {
            var directory = new DirectoryInfo(@"Python\AllFaces");
            FileInfo[] Files = (from f in directory.GetFiles() orderby f.LastWriteTime descending select f).ToArray();
            
            try
            {
                
                for (int i = 0; i < Files.Length; i++)
                {
                    if (i == 0)
                    {
                        AllFacesPictureBox1.Image = Image.FromFile(Files[i].FullName);
                    }
                    else if (i == 1)
                    {
                        AllFacesPictureBox2.Image = Image.FromFile(Files[i].FullName);
                    }
                    else if (i == 2)
                    {
                        AllFacesPictureBox3.Image = Image.FromFile(Files[i].FullName);
                    }
                    else if (i == 4)
                    {
                        AllFacesPictureBox4.Image = Image.FromFile(Files[i].FullName);
                    }
                    else if (i == 5)
                    {
                        AllFacesPictureBox5.Image = Image.FromFile(Files[i].FullName);
                    }
                    else 
                    {
                    if (File.Exists(Files[i].FullName))
                    {
                        try
                        {
                            File.Delete(Files[i].FullName);
                        }
                        catch
                        {

                        }
                     }
                    }
                }              
                

            }
            catch (OutOfMemoryException e)
            {
                //MessageBox.Show("Out of Memory: {0} " + e.Message.ToString());
            }
        }

        public void DecodeKey(string Key)
        {

        }

        public void AddAllFacePictureBoxesToList()
        {
            AllFacesPictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            AllFacesPictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            AllFacesPictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            AllFacesPictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            AllFacesPictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void EmployeesTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EmployeesTabControl.SelectedIndex != 0)
            {
                AddEmployeeFfpPictureBox.Image = null;

                if (EmployeesTabControl.SelectedIndex == 2)
                {
                    if (SearchEmpEidCheckBox.Checked == false && SearchEmpFnCheckBox.Checked == false)
                    {
                        GetAllEmployees();
                    }
                }
            }
            if (EmployeesTabControl.SelectedIndex == 3)
            {
                GetAllLogs();
            }
        }

        private void SetupButton_Click(object sender, EventArgs e)
        {
            int Checker = 3;
            if (ServerTextBox.Text == "")
            {
                ServerTextBox.BackColor = Color.FromArgb(255, 255, 70, 70);
                Checker--;
            }
            else
            {
                ServerTextBox.BackColor = Color.FromArgb(255, 250, 250, 250);
            }

            if (UserTextBox.Text == "")
            {
                UserTextBox.BackColor = Color.FromArgb(255, 255, 70, 70);
                Checker--;
            }
            else
            {
                UserTextBox.BackColor = Color.FromArgb(255, 250, 250, 250);
            }

            if (PasswordTextBox.Text == "")
            {
                PasswordTextBox.BackColor = Color.FromArgb(255, 255, 70, 70);
                Checker--;
            }
            else
            {
                PasswordTextBox.BackColor = Color.FromArgb(255, 250, 250, 250);
            }

            if (Checker == 3)
            {
                SetupDB(ServerTextBox.Text, UserTextBox.Text, PasswordTextBox.Text);
                DbErrorLabel.Text = "Database has been setup, restart application to continue...";
            }
            else
            {
                DbErrorLabel.Text = "Missing Fields";
            }
            

        }

        public void SetupDB(string ServerName, string UserName, string Password)
        {
            using (StreamWriter ServerFile = new StreamWriter(@"Python\Config\host.txt"))
            {
                ServerFile.WriteLine(ServerName);
                ServerFile.Flush();
                ServerFile.Close();
            }
            using (StreamWriter UserFile = new StreamWriter(@"Python\Config\user.txt"))
            {
                UserFile.WriteLine(UserName);
                UserFile.Flush();
                UserFile.Close();
            }
            using (StreamWriter PassFile = new StreamWriter(@"Python\Config\password.txt"))
            {
                PassFile.WriteLine(Password);
                PassFile.Flush();
                PassFile.Close();
            }
            using (StreamWriter DbFileWriter = new StreamWriter(@"Python\Config\DbSetup.txt"))
            {
                DbFileWriter.WriteLine("True");
                DbFileWriter.Flush();
                DbFileWriter.Close();
                RunDBmScript();
            }
        }

        public bool CheckIfHoursNumeric()
        {
            int x;
            if (Int32.TryParse(ImageTimePeriodTextBox.Text, out x))
            {
                return true;
            }
            else
            {
                return false;
            }
                }

        public bool CheckIfSecondsNumeric()
        {
            int x;
            if (Int32.TryParse(LogsTimePeriodTextBox.Text, out x))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SettingsSaveButton_Click(object sender, EventArgs e)
        {
            if (NewPasswordTextBox.Text != "")
            {
                if (NewPasswordTextBox.Text == RetypeNewPasswordTextBox.Text)
                {
                    using (StreamWriter NewPasswordTextFile = new StreamWriter(@"WinformConfig\Password.txt"))
                    {
                        NewPasswordTextFile.WriteLine(NewPasswordTextBox.Text);
                        NewPasswordTextFile.Flush();
                        NewPasswordTextFile.Close();
                    }
                    GetPassword();
                }
            }

            using (StreamWriter UiSizeTextFile = new StreamWriter(@"WinformConfig\UiSize.txt"))
            {
                UiSizeTextFile.WriteLine(UiScreenSizeComboBox.SelectedItem);
                UiSizeTextFile.Flush();
                UiSizeTextFile.Close();
            }
    

                using (StreamWriter ScreenSizeTextFile = new StreamWriter(@"Python\COnfig\ScreenSize.txt"))
            {
                ScreenSizeTextFile.WriteLine(VideoScreenSizeComboBox.SelectedItem);
                ScreenSizeTextFile.Flush();
                ScreenSizeTextFile.Close();
            }
            System.IO.File.WriteAllText(@"Python\Config\Levels.txt", LevelsRTB.Text);

            if (CheckIfHoursNumeric())
            {
                using (StreamWriter ImageTimeWriter = new StreamWriter(@"Python\Config\ImageDeleteTimePeriod.txt"))
                {
                    ImageTimeWriter.WriteLine(ImageTimePeriodTextBox.Text);
                    ImageTimeWriter.Flush();
                    ImageTimeWriter.Close();
                }
                ImageTimePeriodTextBox.BackColor = Color.White;
            }
            else
            {
                ImageTimePeriodTextBox.BackColor = Color.Red;
            }

            if (CheckIfSecondsNumeric())
            {
                using (StreamWriter ImageTimeWriter = new StreamWriter(@"Python\Config\LogsTimePeriod.txt"))
                {
                    ImageTimeWriter.WriteLine(LogsTimePeriodTextBox.Text);
                    ImageTimeWriter.Flush();
                    ImageTimeWriter.Close();
                }
                LogsTimePeriodTextBox.BackColor = Color.White;
            }
            else
            {
                LogsTimePeriodTextBox.BackColor = Color.Red;
            }

            using (StreamWriter AllFacesLocationTextFile = new StreamWriter(@"Python\Config\AllFacesSaveLocation.txt"))
            {
                AllFacesLocationTextFile.WriteLine(AllFacesLocationShowerLabel.Text);
                AllFacesLocationTextFile.Flush();
                AllFacesLocationTextFile.Close();
            }

            GetLevels();

            GetSettings();
        }

        public void GetSettings()
        {
            using (StreamReader ImageTimeReader = new StreamReader(@"Python\Config\ImageDeleteTimePeriod.txt"))
            {
                ImageTimePeriodTextBox.Text = ImageTimeReader.ReadLine();
                ImageTimeReader.Close();
            }
            using (StreamReader LogsTimeReader = new StreamReader(@"Python\Config\LogsTimePeriod.txt"))
            {
                LogsTimePeriodTextBox.Text = LogsTimeReader.ReadLine();
                LogsTimeReader.Close();
            }
            using (StreamReader AllFacesLocationReader = new StreamReader(@"Python\Config\AllFacesSaveLocation.txt"))
            {
                AllFacesLocationShowerLabel.Text = AllFacesLocationReader.ReadLine();
                AllFacesLocationReader.Close();
            }

            string ScreenSize;

            using (StreamReader UiSizeTextFile = new StreamReader(@"WinformConfig\UiSize.txt"))
            {
                ScreenSize = UiSizeTextFile.ReadLine();
                UiSizeTextFile.Close();
            }

            if (ScreenSize == "Fullscreen")
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                string[] Bounds = ScreenSize.Split('x');

                Bounds[0] = Bounds[0].Replace(" ", string.Empty);
                Bounds[1] = Bounds[1].Replace(" ", string.Empty);

                int w, h;

                if(Int32.TryParse(Bounds[0],out w) && Int32.TryParse(Bounds[1], out h))
                {
                    this.Size = new System.Drawing.Size(w, h);
                }
                else
                {
                    MessageBox.Show("Invalid size");
                }

            }

        }

        public void KillPythonProcesses()
        {
            try
            {
                ProcessStartInfo KillPythonStartInfo = new ProcessStartInfo();
                KillPythonStartInfo.FileName = KillPythonFileName;
                KillPythonStartInfo.CreateNoWindow = false;
                KillPythonStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                KillPythonProcess.StartInfo = KillPythonStartInfo;
                KillPythonProcess.Start();
                KillPythonProcess.WaitForExit();
            }
            catch
            {
                MessageBox.Show("Unable to run KillPythonloyee script");
            }
        }


        public void RestartVideoRecogniser()
        {
            KillPythonProcesses();
            //System.Threading.Thread.Sleep(5000);
            RunVideoAndRecogniserScript();
        }
        private void RestartRecogniserButton_Click(object sender, EventArgs e)
        {
            RestartVideoRecogniser();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (SearchByIdTextBox.Text == "")
            {
                SearchEmpEidCheckBox.Checked = false;
            }
            if (SearchByFnTextBox.Text == "")
            {
                SearchEmpFnCheckBox.Checked = false;
            }

            if (SearchEmpEidCheckBox.Checked == true)
            {
                string AllEmployeesCommandString = "select Full_Name,ID from employees where ID='" + SearchByIdTextBox.Text + "'";
                //MessageBox.Show(AllEmployeesCommandString);
                MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable AllEmployeesDataTable = new DataTable();
                AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
            }
            else if (SearchEmpFnCheckBox.Checked == true)
            {
                string AllEmployeesCommandString = "select Full_Name,ID from employees where Full_Name like '" + SearchByFnTextBox.Text + "%'";
                //MessageBox.Show(AllEmployeesCommandString);
                MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable AllEmployeesDataTable = new DataTable();
                AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
            }
            else
            {
                string TargetLevel = AllEmpLvlComboBox.SelectedItem.ToString();
                string TargetEmploymentStatus = AllEmpEsComboBox.SelectedItem.ToString();

                if(TargetLevel=="Any" && TargetEmploymentStatus != "Any")
                {
                    string AllEmployeesCommandString = "select Full_Name,ID from employees where Employment_Status = '" + TargetEmploymentStatus + "'";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
                }
                else if(TargetLevel != "Any" && TargetEmploymentStatus == "Any")
                {
                    string AllEmployeesCommandString = "select Full_Name,ID from employees where Title = '" + TargetLevel + "'";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
                }
                else if (TargetLevel != "Any" && TargetEmploymentStatus != "Any")
                {
                    string AllEmployeesCommandString = "select Full_Name,ID from employees where Title = '" + TargetLevel + "' and Employment_Status = '" + TargetEmploymentStatus + "'";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    AllEmployeesDataGridView.DataSource = AllEmployeesDataTable;
                }
                else
                {
                    GetAllEmployees();
                }                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillPythonProcesses();
        }

        public Image FFP;

        private void AddEmployeeFfpButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog EmpImageOfd = new OpenFileDialog())
            {
                EmpImageOfd.Filter = "Images(*.jpg)|*.jpg";
                if (EmpImageOfd.ShowDialog() == DialogResult.OK)
                {
                    FfpFilenameLabel.Text = EmpImageOfd.FileName;

                    //AddEmployeeFfpPictureBox.Image?.Dispose();

                    using (var image = System.Drawing.Image.FromFile(EmpImageOfd.FileName))
                    {
                        //AddEmployeeFfpPictureBox.Image = new System.Drawing.Bitmap(image);
                    }

                    //FFP = Image.FromFile(FfpFilenameLabel.Text);
                    AddEmployeeFfpPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            //AddEmployeeFfpPictureBox.Image = FFP;
        }

        private void AllFacesFlowLayoutPanel_MouseEnter(object sender, EventArgs e)
        {
            MouseInLogs = true;
            LogsNotUpdatingLabel.Text = "Logs are not live, remove cursor from image area to show live logs!";
        }

        private void AllFacesFlowLayoutPanel_MouseLeave(object sender, EventArgs e)
        {
            MouseInLogs = false;
            LogsNotUpdatingLabel.Text = "";
        }

        public void DeleteEmployeeTrainingData(string Eid)
        {
            string RecogniserTextFileContent = "";
            string SubString = "\"" + Eid + "\"";
            //string SubString = "\"" + EId + "\"";

            using (StreamReader RecogniserTextFileReader = new StreamReader(@"Python\facerec_128D.txt"))
            {
                RecogniserTextFileContent = RecogniserTextFileReader.ReadToEnd();
                RecogniserTextFileReader.Close();
            }

            int StartIndex = RecogniserTextFileContent.IndexOf(SubString);

            if (StartIndex != (-1))
            {
                int EndIndex = RecogniserTextFileContent.IndexOf("}", StartIndex);
                EndIndex += 3;

                RecogniserTextFileContent = RecogniserTextFileContent.Remove(StartIndex, EndIndex - StartIndex);


                using (StreamWriter RecogniserTextFileWriter = new StreamWriter(@"Python\facerec_128D.txt"))
                {
                    RecogniserTextFileWriter.WriteLine(RecogniserTextFileContent);
                    RecogniserTextFileWriter.Flush();
                    RecogniserTextFileWriter.Close();
                }
            }
            else
            {
                MessageBox.Show("No training data was found for employee: " + Eid);
            }
        }

        private void ViewEmployeeDeleteButton_Click(object sender, EventArgs e)
        {
            string EId = VeEid.Text;
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete employee "+EId, "Delete Employee "+EId, MessageBoxButtons.YesNo);
            
            if (dialogResult == DialogResult.Yes)
            {
                string DeleteEmployeeDataFromEmployeesTableCommandString = "DELETE FROM Employees WHERE ID='" + EId + "'";
                MySqlCommand DeleteEmployeeDataFromEmployeesTableCommand = new MySqlCommand(DeleteEmployeeDataFromEmployeesTableCommandString, MainConn);

                DeleteEmployeeDataFromEmployeesTableCommand.ExecuteNonQuery();

                string DeleteEmployeeDataFromPeopleTableCommandString = "DELETE FROM People WHERE ID='" + EId + "'";
                MySqlCommand DeleteEmployeeDataFromPeopleTableCommand = new MySqlCommand(DeleteEmployeeDataFromPeopleTableCommandString, MainConn);

                DeleteEmployeeDataFromPeopleTableCommand.ExecuteNonQuery();


                DeleteEmployeeTrainingData(EId);

                VeEid.Text = "_";
                VeLvl.Text = "_";
                VeFn.Text = "_";
                VeLn.Text = "_";
                VeDOB.Text = "_";
                VeGen.Text = "_";
                VeEs.Text = "_";
                VeM1.Text = "_";
                VeM2.Text = "_";
                VeWe.Text = "_";
                VeAdd.Text = "_";

                EmployeePersonalDataGridView.DataSource = null;

                EmployeesTabControl.SelectedIndex = 2;


                ViewEmployeeDeleteLabel.Visible = true;



                /*
                MySqlDataAdapter DeleteEmployeDataAdapter = new MySqlDataAdapter(DeleteEmployeCommand);
                DeleteEmployeDataAdapter.SelectCommand.CommandType = CommandType.Text;

                DataTable DeleteEmployeDataTable = new DataTable();
                DeleteEmployeDataAdapter.Fill(DeleteEmployeDataTable);

                DeleteEmployeDataGridView.DataSource = DeleteEmployeDataTable;
                */
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }
        }

        private void DatabaseCleanButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("THIS WILL DELETE ALL ATTENDANCE AND EMPLOYEE RECORDS !!!", "CLean Database?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                KillPythonProcesses();
                //System.Threading.Thread.Sleep(2000);

                string DeleteString = "drop table people";
                MySqlCommand DeleteTablePeopleCommand = new MySqlCommand(DeleteString, MainConn);
                DeleteTablePeopleCommand.ExecuteNonQuery();

                DeleteString = "drop table employees";
                MySqlCommand DeleteTableEmployees = new MySqlCommand(DeleteString, MainConn);
                DeleteTableEmployees.ExecuteNonQuery();

                DeleteString = "drop table logs";
                MySqlCommand DeleteTableLogs = new MySqlCommand(DeleteString, MainConn);
                DeleteTableLogs.ExecuteNonQuery();

                using (StreamWriter RecogniserTextFileReWriter = new StreamWriter(@"Python\facerec_128D.txt",false))
                {
                    RecogniserTextFileReWriter.WriteLine("{}");
                    RecogniserTextFileReWriter.Flush();
                    RecogniserTextFileReWriter.Close();
                }



                    RestartVideoRecogniser();
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }
        }

        private void ExportByDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ExportByDateCheckBox.Checked == false)
            {
                ExportDataStartDateDTP.Enabled = false;
                ExportDataEndDateDTP.Enabled = false;
            }
            else
            {
                ExportByMonthCheckBox.Checked = false;
                ExportDataStartDateDTP.Enabled = true;
                ExportDataEndDateDTP.Enabled = true;
            }
        }

        private void ExportByMonthCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ExportByMonthCheckBox.Checked == true)
            {
                ExportByDateCheckBox.Checked = false;
                ExportByMonthDTP.Enabled = true;
            }
            else
            {
                ExportByMonthDTP.Enabled = false;
            }
        }

        public bool CheckExportDetails()
        {
            if (ExportByDateCheckBox.Checked == false && ExportByMonthCheckBox.Checked == false)
            {
                MessageBox.Show("Please select an export method!");
                return false;
            }
            else if(ExportLocationShowerLabel.Text== "No folder selected")
            {
                MessageBox.Show("Select an export location");
                return false;                
            }
            else
            {
                return true;
            }
        }

        public void AutoInterchangeDatesIfRequired()
        {
            if (ExportDataStartDateDTP.Value > ExportDataEndDateDTP.Value)
            {
                System.DateTime TempDate = ExportDataStartDateDTP.Value;
                ExportDataStartDateDTP.Value = ExportDataEndDateDTP.Value;
                ExportDataEndDateDTP.Value = TempDate;
                // MessageBox.Show("errur");
            }
        }

        public void RunExportScript()
        {
            try
            {
                ProcessStartInfo ExportScriptStartInfo = new ProcessStartInfo();
                ExportScriptStartInfo.FileName = ExportScriptFileName;
                ExportScriptStartInfo.CreateNoWindow = false;
                ExportScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                ExportScriptProcess.StartInfo = ExportScriptStartInfo;
                ExportScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }
        }


        public void OnIncorrentSqlConfig()
        {
            //MessageBox.Show("Incorrect SQL configuration...");
            SqlErrorLabel.Text = "Incorrect SQL configuration!";
            LoadDbSetupScreen();
        }

        /*
        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            SuspendLayout();
            base.OnResizeBegin(e);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            ResumeLayout();
            base.OnResizeEnd(e);
        }
        */

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            this.Update();
        }

        private void MainTabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 1)
            {
                EmployeesTabControl.Visible = false;
            }
            else if (MainTabControl.SelectedIndex == 2)
            {
                ManageCamerasContentPanel.Visible = false;
            }
            else if (MainTabControl.SelectedIndex == 4)
            {
                DatabaseSetupContentPanel.Visible = false;
            }
            else if (MainTabControl.SelectedIndex == 3)
            {
                ExportDataCOntentPanel.Visible = false;
            }
            else if (MainTabControl.SelectedIndex == 5)
            {
                SettingsContentPanel.Visible = false;
                //SettingsLeftPanel.Visible = false;
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            ResumeLayout();
        }

        /*
        protected override void OnResizeBegin(EventArgs e)
        {
            SuspendLayout();
            base.OnResizeBegin(e);
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            ResumeLayout();
            base.OnResizeEnd(e);
        }
        */
        private void Form1_Resize_1(object sender, EventArgs e)
        {
            if (this.Size.Width >1000)
            {
                LogsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                AllEmployeesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                EmployeePersonalDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            else
            {
                LogsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                AllEmployeesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                EmployeePersonalDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
        }

        List<int> ScreenWidthList = new List<int>();

        public Form1()
        {                     
            Resize += Form1_Resize;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

            Thread Thread1 = new Thread(new ThreadStart(ShowLoadingScreen));
            Thread1.Start();

            StartKeyPromptAtCentre = false;

            int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

            KeyPromptLocation = new Point(ScreenWidth/2 -300, ScreenHeight/2+100);

            CheckIfExpired();

            GetAllEmployees();
            GetPassword();
            RunVideoAndRecogniserScript();
            InitializeComponent();
            DesignFormComponents();

            AddAllFacePictureBoxesToList();
            ReadConfig();            
            GetLevels();
            LoadPreviousCameraConfiguration();
            RunDBmScript();
            Thread.Sleep(IntroInterval);
            Thread1.Abort();
            Thread.Sleep(1000);
            BringFormToFront();

            //OnNewKey();

            StartKeyPromptAtCentre = true;
        }

        private void TrainingConfirmationButton_Click(object sender, EventArgs e)
        {
            if (HasDbBeenSetup)
            {
                string DummyID = new string(AddEmployeeEidTextBox.Text.ToCharArray());
                    


                            string TrainingTestCommandString = "select * from employees where ID='" + DummyID + "'";
                            //MessageBox.Show(TrainingTestCommandString);

                            MySqlCommand TrainingTestCommand = new MySqlCommand(TrainingTestCommandString, MainConn);

                            MySqlDataAdapter TrainingTestDataAdapter = new MySqlDataAdapter(TrainingTestCommand);
                            TrainingTestDataAdapter.SelectCommand.CommandType = CommandType.Text;

                            DataTable TrainingTestDataTable = new DataTable();
                            TrainingTestDataAdapter.Fill(TrainingTestDataTable);

                            //MessageBox.Show(TrainingTestDataTable.Rows.Count.ToString());

                            if (TrainingTestDataTable.Rows.Count > 0)
                {
                    TrainingConfirmationLabel.Text = "Training successful";
                    TrainingConfirmationLabel.ForeColor = Color.Green;

                    //Thread.Sleep(50);

                    //AddEmployeeEidTextBox.Text = "";
                    AddEmployeeTitleComboBox.SelectedIndex = 0;
                    AddEmployeeFnTextBox.Text = "";
                    AddEmployeeLnTextBox.Text = "";
                    //AddEmployeeDobDTP.Value.ToString = "";
                    AddEmployeeGenderComboBox.SelectedIndex = 0;
                    AddEmployeeEsComboBox.SelectedIndex = 0;
                    AddEmployeeM1TextBox.Text = "";
                    AddEmployeeM2TextBox.Text = "";
                    AddEmployeeWeTextBox.Text = "";
                    AddEmployeeAddressTextBox.Text = "";
                }                          
                                
                
                            else
                            {
                                TrainingConfirmationLabel.Text = "Training unsuccessful";
                                TrainingConfirmationLabel.ForeColor = Color.Red;
                            }
            }
        }

        private void AddEmployeeEidTextBox_TextChanged(object sender, EventArgs e)
        {
            TrainingConfirmationLabel.Text = "Training Pending";
            TrainingConfirmationLabel.ForeColor = Color.Red;
        }

       

        private void ExportDataExportButton_Click(object sender, EventArgs e)
        {
            if (CheckExportDetails())
            {
                AutoInterchangeDatesIfRequired();

                if (ExportDataNameTextBox.Text != "")
                {
                    ExportLvlComboBox.SelectedIndex = 0;
                    ExportEsComboBox.SelectedIndex = 0;
                }

                using (StreamWriter ExportDetailsTextFile = new StreamWriter(@"Python\Export\ExportDetails.txt"))
                {
                    ExportDetailsTextFile.WriteLine(ExportDataNameTextBox.Text);
                    if (ExportByDateCheckBox.Checked == true)
                    {
                        ExportDetailsTextFile.WriteLine("Dates");
                        ExportDetailsTextFile.WriteLine(ExportDataStartDateDTP.Value.ToString("yyyy-MM-dd"));
                        ExportDetailsTextFile.WriteLine(ExportDataEndDateDTP.Value.ToString("yyyy-MM-dd"));
                        ExportDetailsTextFile.WriteLine(ExportLocationShowerLabel.Text);
                        ExportDetailsTextFile.WriteLine(ExportLvlComboBox.SelectedItem.ToString());
                        ExportDetailsTextFile.WriteLine(ExportEsComboBox.SelectedItem.ToString());
                        ExportDetailsTextFile.WriteLine(ExportDataExportFileNameTextBox.Text);
                    }
                    else
                    {
                        ExportDetailsTextFile.WriteLine("Month");

                        System.DateTime date = ExportByMonthDTP.Value;
                        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                        var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                        ExportDetailsTextFile.WriteLine(firstDayOfMonth.ToString("yyyy-MM-dd"));
                        ExportDetailsTextFile.WriteLine(lastDayOfMonth.ToString("yyyy-MM-dd"));
                        ExportDetailsTextFile.WriteLine(ExportLocationShowerLabel.Text);
                        ExportDetailsTextFile.WriteLine(ExportLvlComboBox.SelectedItem.ToString());
                        ExportDetailsTextFile.WriteLine(ExportEsComboBox.SelectedItem.ToString());
                        ExportDetailsTextFile.WriteLine(ExportDataExportFileNameTextBox.Text);
                    }
                    ExportDetailsTextFile.Flush();
                    ExportDetailsTextFile.Close();
                }

                RunExportScript();
            }
            else
            {
                
            }
        }

        private void SearchEmpEidCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SearchEmpEidCheckBox.Checked == true)
            {
                SearchEmpFnCheckBox.Checked = false;
                SearchByIdTextBox.Enabled = true;
            }
            else
            {
                SearchByIdTextBox.Enabled = false;
            }
        }

        private void SearchEmpFnCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SearchEmpFnCheckBox.Checked == true)
            {
                SearchEmpEidCheckBox.Checked = false;
                SearchByFnTextBox.Enabled = true;
            }
            else
            {
                SearchByFnTextBox.Enabled = false;
            }
        }

        private void ExpiryTimer_Tick(object sender, EventArgs e)
        {
            if (!IsEnteringKey)
            {
                CheckIfExpired();
                RunMarkAbsenteesScript();
            }         
        }

        private void ExportLocationButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog SelectExportDirectoryOFd = new FolderBrowserDialog())
            {
                DialogResult result = SelectExportDirectoryOFd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(SelectExportDirectoryOFd.SelectedPath))
                {
                    ExportLocationShowerLabel.Text = SelectExportDirectoryOFd.SelectedPath;
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog SelectAllFacesDirectoryOFd = new FolderBrowserDialog())
            {
                DialogResult result = SelectAllFacesDirectoryOFd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(SelectAllFacesDirectoryOFd.SelectedPath))
                {
                    AllFacesLocationShowerLabel.Text = SelectAllFacesDirectoryOFd.SelectedPath;                    
                }
            }
        }

        public void RunRetrainScript()
        {
            try
            {
                ProcessStartInfo RetrainScriptStartInfo = new ProcessStartInfo();
                RetrainScriptStartInfo.FileName = RetrainScriptFileName;
                RetrainScriptStartInfo.CreateNoWindow = false;
                RetrainScriptStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                RetrainScriptProcess.StartInfo = RetrainScriptStartInfo;
                RetrainScriptProcess.Start();
            }
            catch
            {
                MessageBox.Show("Unable to run Edit Employee script");
            }
        }

        private void RetrainButton_Click(object sender, EventArgs e)
        {           
            if (CheckAndWriteTrainingData())
            {
                using (StreamWriter RetrainTextFile = new StreamWriter(@"Python\Retrain\RetrainDetails.txt"))
                {
                    RetrainTextFile.WriteLine(TrainingType);
                    RetrainTextFile.WriteLine(TrainingParameters);
                    RetrainTextFile.WriteLine(AddEmployeeEidTextBox.Text);
                    RetrainTextFile.Flush();
                    RetrainTextFile.Close();
                }

                DeleteEmployeeTrainingData(AddEmployeeEidTextBox.Text);

                RunRetrainScript();
            }
        }
        private void ExportDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ExportDataNameTextBox.Text = ExportDataGridView.CurrentRow.Cells[1].Value.ToString();
        }

        private void ExportClearIdButton_Click(object sender, EventArgs e)
        {
            ExportDataGridView.ClearSelection();
            ExportDataNameTextBox.Text = "";
        }

        private void LogsSearchButton_Click(object sender, EventArgs e)
        {
            if (LogsEmployeeIdTextBox.Text == "")
            {
                LogsEmployeeIDCheckBox.Checked = false;
            }
            if (LogsFnTextBox.Text == "")
            {
                LogsFnCheckBox.Checked = false;
            }

            if (LogsCustomIntervalCheckBox.Checked == false)
            {
                if (LogsEmployeeIDCheckBox.Checked == true)
                {
                    string AllEmployeesCommandString = "select Cam,LogTime from logs where ID='" + LogsEmployeeIdTextBox.Text + "' order by EntryNumber desc";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                }
                else if (LogsFnCheckBox.Checked == true)
                {
                    string AllEmployeesCommandString = "select Cam,LogTime from logs where Full_Name like '" + LogsFnTextBox.Text + "%' order by EntryNumber desc";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                }
                else
                {
                    GetAllLogs();
                    
                    /*

                    string TargetLevel = LogsLevelComboBox.SelectedItem.ToString();
                    string TargetEmploymentStatus = LogsEmploymentStatusComboBox.SelectedItem.ToString();

                    if (TargetLevel == "Any" && TargetEmploymentStatus != "Any")
                    {
                        string AllEmployeesCommandString = "select Full_Name,ID,Cam,LogTime from logs where Employment_Status = '" + TargetEmploymentStatus + "'";
                        //MessageBox.Show(AllEmployeesCommandString);
                        MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                        MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                        AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                        DataTable AllEmployeesDataTable = new DataTable();
                        AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                        EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                    }
                    else if (TargetLevel != "Any" && TargetEmploymentStatus == "Any")
                    {
                        string AllEmployeesCommandString = "select Full_Name,ID,Cam,LogTime from logs where Title = '" + TargetLevel + "'";
                        //MessageBox.Show(AllEmployeesCommandString);
                        MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                        MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                        AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                        DataTable AllEmployeesDataTable = new DataTable();
                        AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                        EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                    }
                    else if (TargetLevel != "Any" && TargetEmploymentStatus != "Any")
                    {
                        string AllEmployeesCommandString = "select Full_Name,ID,Cam,LogTime from logs where Title = '" + TargetLevel + "' and Employment_Status = '" + TargetEmploymentStatus + "'";
                        //MessageBox.Show(AllEmployeesCommandString);
                        MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                        MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                        AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                        DataTable AllEmployeesDataTable = new DataTable();
                        AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                        EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                    }
                    else
                    {
                        GetAllEmployees();
                    }
                    */
                }
            }
            else
            {
                List<string> TempStrings = new List<string>();

                string StartDateString = LogsStartDateDateTimePicker.Value.ToString("yyyy-MM-dd");
                TempStrings = StartDateString.Split(' ').ToList();
                StartDateString = TempStrings[0];
                
                string StartTimeString = LogsStartTimeDateTimePicker.Value.ToString();
                TempStrings = StartTimeString.Split(' ').ToList();
                StartTimeString = TempStrings[1];

                string EndDateString = LogsEndDateDateTimePicker.Value.ToString("yyyy-MM-dd");
                TempStrings = EndDateString.Split(' ').ToList();
                EndDateString = TempStrings[0];

                string EndTimeString = LogsEndTimeDateTimePicker.Value.ToString();
                TempStrings = EndTimeString.Split(' ').ToList();
                EndTimeString = TempStrings[1];

                string StartDateTime = StartDateString + " " + StartTimeString;
                string EndDateTime = EndDateString + " " + EndTimeString;

                if (LogsEmployeeIDCheckBox.Checked == true)
                {
                    string AllEmployeesCommandString = "select Cam,LogTime from logs where ID='" + LogsEmployeeIdTextBox.Text + "' and LogTime > '" + StartDateTime + "' and LogTime < '" + EndDateTime + "' order by EntryNumber desc";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                }
                else if (LogsFnCheckBox.Checked == true)
                {
                    string AllEmployeesCommandString = "select Cam,LogTime from logs where Full_Name like '" + LogsFnTextBox.Text + "%' and LogTime > '" + StartDateTime + "' and LogTime < '" + EndDateTime + "' order by EntryNumber desc";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                }
                else
                {
                    string AllEmployeesCommandString = "select Full_Name,ID,Cam,LogTime from logs where LogTime > '" + StartDateTime + "' and LogTime < '" + EndDateTime + "' order by EntryNumber desc";
                    //MessageBox.Show(AllEmployeesCommandString);
                    MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

                    MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
                    AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable AllEmployeesDataTable = new DataTable();
                    AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

                    EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
                }

                    //MessageBox.Show("select Full_Name,ID,Cam,LogTime from logs where LogTime > '" + StartDateTime + "' and LogTime < '" + EndDateTime + "' order by EntryNumber desc");
            }
        }

        void GetAllLogs()
        {
            string AllEmployeesCommandString = "select Full_Name,ID,Cam,LogTime from logs order by EntryNumber desc";
            //MessageBox.Show(AllEmployeesCommandString);
            MySqlCommand AllEmployeesCommand = new MySqlCommand(AllEmployeesCommandString, MainConn);

            MySqlDataAdapter AllEmployeesDataAdapter = new MySqlDataAdapter(AllEmployeesCommand);
            AllEmployeesDataAdapter.SelectCommand.CommandType = CommandType.Text;

            DataTable AllEmployeesDataTable = new DataTable();
            AllEmployeesDataAdapter.Fill(AllEmployeesDataTable);

            EmpLogsDataGridView.DataSource = AllEmployeesDataTable;
        }

        private void LogsEmployeeIDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LogsEmployeeIDCheckBox.Checked == true)
            {
                LogsFnCheckBox.Checked = false;
                LogsEmployeeIdTextBox.Enabled = true;
            }
            else
            {
                LogsEmployeeIdTextBox.Enabled = false;
            }
        }

        private void LogsFnCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LogsFnCheckBox.Checked == true)
            {
                LogsEmployeeIDCheckBox.Checked = false;
                LogsFnTextBox.Enabled = true;
            }
            else
            {
                LogsFnTextBox.Enabled = false;
            }
        }

        void ResetCameraConfigurationSaveStatusLabel()
        {
            CameraConfigurationSaveStatusLabel.Text = "";
        }
        private void Camera1TextBox_TextChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera2TextBox_TextChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera3TextBox_TextChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera4TextBox_TextChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera4Access4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera4Access3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera4Access2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera4Access1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera3Access4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera3Access3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera3Access2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera3Access1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera2Access4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera2Access3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera2Access2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera2Access1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera1Access4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera1Access3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera1Access2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void Camera1Access1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetCameraConfigurationSaveStatusLabel();
        }

        private void LogsCustomIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LogsCustomIntervalCheckBox.Checked == true)
            {
                LogsStartDateLabel.BackColor = Color.FromArgb(255, 250, 250, 250);
                LogsStartDateDateTimePicker.Enabled = true;
                LogsStartTimeDateTimePicker.Enabled = true;

                LogsEndDateLabel.BackColor = Color.FromArgb(255, 250, 250, 250);
                LogsEndDateDateTimePicker.Enabled = true;
                LogsEndTimeDateTimePicker.Enabled = true;
            }
            else if (LogsCustomIntervalCheckBox.Checked == false)
            {
                LogsStartDateLabel.BackColor = Color.Silver;
                LogsStartDateDateTimePicker.Enabled = false;
                LogsStartTimeDateTimePicker.Enabled = false;

                LogsEndDateLabel.BackColor = Color.Silver;
                LogsEndDateDateTimePicker.Enabled = false;
                LogsEndTimeDateTimePicker.Enabled = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ReadConfig();
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            SuspendLayout();
        }

        public void ShowLoadingScreen()
        {
            Application.Run(new IntroForm());
        }
        
        public void BringFormToFront()
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        List<int> ScreenHeightList = new List<int>();

        public void DesignFormComponents()
        {
            //ScreenWidthList.Add(1024);
            ScreenWidthList.Add(1280);
            ScreenWidthList.Add(1366);
            ScreenWidthList.Add(1600);
            ScreenWidthList.Add(1920);
            ScreenWidthList.Add(2560);
            ScreenWidthList.Add(3840);

            //ScreenHeightList.Add(576);
            ScreenHeightList.Add(720);
            ScreenHeightList.Add(768);
            ScreenHeightList.Add(900);
            ScreenHeightList.Add(1080);
            ScreenHeightList.Add(1440);
            ScreenHeightList.Add(2160);

            int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

            float AspectRatio = ScreenWidth / ScreenHeight;
            UiScreenSizeComboBox.Items.Clear();


            for(int i = 0; i < 6; i++)
            {
                if(ScreenWidthList[i]<ScreenWidth)
                {
                    UiScreenSizeComboBox.Items.Add(ScreenWidthList[i].ToString() + " x " + ScreenHeightList[i].ToString());
                }
                else
                {
                    UiScreenSizeComboBox.Items.Add("Fullscreen");
                    break;
                }
            }

            EmployeesTabControl.Visible = false;
                
                    ManageCamerasContentPanel.Visible = false;
                
                    DatabaseSetupContentPanel.Visible = false;
                
                    ExportDataCOntentPanel.Visible = false;
                
                
                    SettingsContentPanel.Visible = false;
            //SettingsLeftPanel.Visible = false;


            AllEmpEsComboBox.SelectedIndex = 0;
            ExportEsComboBox.SelectedIndex = 0;

            ExportByMonthDTP.Format = DateTimePickerFormat.Custom;
            ExportByMonthDTP.CustomFormat = "MMMM yyyy";

            LogsStartDateDateTimePicker.CustomFormat = "MM/dd/yyyy";

            LogsStartTimeDateTimePicker.Format = DateTimePickerFormat.Time;
            LogsStartTimeDateTimePicker.ShowUpDown = true;


            LogsEndDateDateTimePicker.CustomFormat = "MM/dd/yyyy";

            LogsEndTimeDateTimePicker.Format = DateTimePickerFormat.Time;
            LogsEndTimeDateTimePicker.ShowUpDown = true;

            //ExportByMonthDTP.ShowUpDown = true;

            LogsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            AllEmployeesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            EmployeePersonalDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


            DataGridViewImageColumn LogsImageColumn = new DataGridViewImageColumn();
            LogsImageColumn.HeaderText = "Image";
            LogsDataGridView.Columns.Insert(0, LogsImageColumn);

            LogsDataGridView.BorderStyle = BorderStyle.None;
            LogsDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            LogsDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            LogsDataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            LogsDataGridView.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            LogsDataGridView.BackgroundColor = Color.White;

            LogsDataGridView.EnableHeadersVisualStyles = false;
            LogsDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            LogsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            LogsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            LogsDataGridView.DefaultCellStyle.Font = new Font("Arial", 18F, GraphicsUnit.Pixel);
            LogsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 18F, FontStyle.Bold);

            AllEmployeesDataGridView.BorderStyle = BorderStyle.None;
            AllEmployeesDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            AllEmployeesDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            AllEmployeesDataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            AllEmployeesDataGridView.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            AllEmployeesDataGridView.BackgroundColor = Color.White;

            AllEmployeesDataGridView.EnableHeadersVisualStyles = false;
            AllEmployeesDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            AllEmployeesDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            AllEmployeesDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            AllEmployeesDataGridView.DefaultCellStyle.Font = new Font("Arial", 18F, GraphicsUnit.Pixel);
            AllEmployeesDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 18F, FontStyle.Bold);

            ExportDataGridView.BorderStyle = BorderStyle.None;
            ExportDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            ExportDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ExportDataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            ExportDataGridView.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            ExportDataGridView.BackgroundColor = Color.White;

            ExportDataGridView.EnableHeadersVisualStyles = false;
            ExportDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ExportDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            ExportDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            ExportDataGridView.DefaultCellStyle.Font = new Font("Arial", 14F, GraphicsUnit.Pixel);
            ExportDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 14F, FontStyle.Bold);

            EmployeePersonalDataGridView.BorderStyle = BorderStyle.None;
            EmployeePersonalDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            EmployeePersonalDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            EmployeePersonalDataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            EmployeePersonalDataGridView.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            EmployeePersonalDataGridView.BackgroundColor = Color.White;

            EmployeePersonalDataGridView.EnableHeadersVisualStyles = false;
            EmployeePersonalDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EmployeePersonalDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            EmployeePersonalDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            EmployeePersonalDataGridView.DefaultCellStyle.Font = new Font("Arial", 18F, GraphicsUnit.Pixel);
            EmployeePersonalDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 18F, FontStyle.Bold);

            EmpLogsDataGridView.BorderStyle = BorderStyle.None;
            EmpLogsDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            EmpLogsDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            EmpLogsDataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            EmpLogsDataGridView.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            EmpLogsDataGridView.BackgroundColor = Color.White;

            EmpLogsDataGridView.EnableHeadersVisualStyles = false;
            EmpLogsDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EmpLogsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            EmpLogsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            EmpLogsDataGridView.DefaultCellStyle.Font = new Font("Arial", 18F, GraphicsUnit.Pixel);
            EmpLogsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 18F, FontStyle.Bold);

        }
    }
}
