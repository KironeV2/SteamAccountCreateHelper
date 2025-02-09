﻿using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;
using SteamAccountCreateHelper.Forms;
using SteamAccountCreateHelper.Models;
using SteamAccountCreateHelper.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static SteamAccountCreateHelper.ManageEmails;

namespace SteamAccountCreateHelper
{
    public partial class Main : Form
    {
        public static Main _Form1;

        public static string Database_Path = AppDomain.CurrentDomain.BaseDirectory + "Database\\";
        public static string Used_Mail_DB_Path = Database_Path + "Used_Email_DB.json";
        public static string CreationidDB_Path = Database_Path + "CreationID_DB.json";
        public static string Pop3Domains_Path = Database_Path + "Pop3Domains.json";
        public static string Acc_Create_Path = Database_Path + "Created_Accounts\\";
        public static string LoginAndPassFilePath = Path.Combine(Database_Path + "LoginAndPass.txt");

        public static List<string> countries = JsonConvert.DeserializeObject<List<string>>("[\"AF\",\"AX\",\"AL\",\"DZ\",\"AS\",\"AD\",\"AO\",\"AI\",\"AQ\",\"AG\",\"AR\",\"AM\",\"AW\",\"AU\",\"AT\",\"AZ\",\"BS\",\"BH\",\"BD\",\"BB\",\"BY\",\"BE\",\"BZ\",\"BJ\",\"BM\",\"BT\",\"BO\",\"BQ\",\"BA\",\"BW\",\"BV\",\"BR\",\"IO\",\"VG\",\"BN\",\"BG\",\"BF\",\"BI\",\"KH\",\"CM\",\"CV\",\"KY\",\"CF\",\"TD\",\"CL\",\"CN\",\"CX\",\"CC\",\"CO\",\"KM\",\"CG\",\"CD\",\"CK\",\"CR\",\"CI\",\"HR\",\"CU\",\"CW\",\"CY\",\"CZ\",\"DK\",\"DJ\",\"DM\",\"DO\",\"EC\",\"EG\",\"SV\",\"GQ\",\"ER\",\"EE\",\"ET\",\"FK\",\"FO\",\"FJ\",\"FI\",\"FR\",\"GF\",\"PF\",\"TF\",\"GA\",\"GM\",\"GE\",\"DE\",\"GH\",\"GI\",\"GR\",\"GL\",\"GD\",\"GP\",\"GU\",\"GT\",\"GG\",\"GN\",\"GW\",\"GY\",\"HT\",\"HM\",\"HN\",\"HK\",\"HU\",\"IS\",\"IN\",\"ID\",\"IQ\",\"IE\",\"IR\",\"IM\",\"IL\",\"IT\",\"JM\",\"JP\",\"JE\",\"JO\",\"KZ\",\"KE\",\"KI\",\"KP\",\"KR\",\"XK\",\"KW\",\"KG\",\"LA\",\"LV\",\"LB\",\"LS\",\"LR\",\"LY\",\"LI\",\"LT\",\"LU\",\"MO\",\"MK\",\"MG\",\"MW\",\"MY\",\"MV\",\"ML\",\"MT\",\"MH\",\"MQ\",\"MR\",\"MU\",\"YT\",\"MX\",\"FM\",\"MD\",\"MC\",\"MN\",\"MS\",\"ME\",\"MA\",\"MZ\",\"MM\",\"NA\",\"NR\",\"NP\",\"NL\",\"NC\",\"NZ\",\"NI\",\"NE\",\"NG\",\"NU\",\"NF\",\"MP\",\"NO\",\"OM\",\"PK\",\"PW\",\"PS\",\"PA\",\"PG\",\"PY\",\"PE\",\"PH\",\"PN\",\"PL\",\"PT\",\"PR\",\"QA\",\"RE\",\"RO\",\"RU\",\"RW\",\"BL\",\"LC\",\"MF\",\"WS\",\"SM\",\"ST\",\"SA\",\"SN\",\"RS\",\"SC\",\"SL\",\"SG\",\"SX\",\"SK\",\"SI\",\"SB\",\"SO\",\"ZA\",\"GS\",\"SS\",\"ES\",\"LK\",\"SH\",\"KN\",\"PM\",\"VC\",\"SD\",\"SR\",\"SJ\",\"SZ\",\"SE\",\"CH\",\"SY\",\"TW\",\"TJ\",\"TZ\",\"TH\",\"TL\",\"TG\",\"TK\",\"TO\",\"TT\",\"TN\",\"TR\",\"TM\",\"TC\",\"TV\",\"UG\",\"UA\",\"AE\",\"GB\",\"UM\",\"VI\",\"UY\",\"UZ\",\"VU\",\"VA\",\"VE\",\"VN\",\"WF\",\"EH\",\"YE\",\"ZM\",\"ZW\"]"); 
        public static List<string> Avatar_URL_List = new List<string>();
        public static List<string> Proxy_URL_List = new List<string>();
        public static List<Pop3> pop3s = new List<Pop3>();

        public static int MaxAccInEmail = 10;
        public static List<E_Mail> EMAIl_LIST = new List<E_Mail>();

        public string AvatarImageFilePath = "";
        public string EmailFilePath = "";

        public static E_Mail email = null;

        public static ChromiumWebBrowser chrome;
        public static CefSettings settings = new CefSettings();
        public static ProxyOptions proxy;
        public static bool ProxySet = false;

        public Main()
        {
            _Form1 = this;
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            CheckDiretoryOnStartup.CheckDiretory();
            LoadConfig();

            settings.IgnoreCertificateErrors = true;
            settings.CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"cache");
            settings.CefCommandLineArgs.Add("disable-application-cache", "1");
            settings.CefCommandLineArgs.Add("disable-session-storage", "1");

            var tete = Cef.Initialize(settings);
            txtUrl.Text = "store.steampowered.com/join/?l=english";
            chrome = new ChromiumWebBrowser(txtUrl.Text);
            this.panel1.Controls.Add(chrome);
            chrome.Dock = DockStyle.Fill;
            chrome.AddressChanged += Chrome_AddressChanged;


            Thread th = new Thread(() => ChangerProxyOnStartup());
            th.IsBackground = true;
            th.Start();
        }

        void ChangerProxyOnStartup()
        {
            while (true)
            {
                if (ckUseSingleProxy.Checked && !string.IsNullOrWhiteSpace(txt_SingleProxy.Text) && chrome.IsBrowserInitialized)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    ChangerProxy(txtUrl.Text);
                    break;
                }
            }
        }

        private void Chrome_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            if(e.Address.ToString() == "https://store.steampowered.com/?created_account=1")
            {
                SaveAccountInfo();
            }

            this.Invoke(new MethodInvoker(() =>
            {
                txtUrl.Text = e.Address;
            }));
        }

        void ChangerProxy(string URL)
        {
            if (!ProxySet)
            {
                if (ckUseSingleProxy.Checked)
                {
                    if (string.IsNullOrWhiteSpace(Main._Form1.txt_SingleProxy.Text))
                    {
                        MessageBox.Show("Proxy is null! Please enter proxy", "Proxy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try 
                    {
                        var split = txt_SingleProxy.Text.Split(':');
                        proxy = new ProxyOptions(split[0], split[1], split[2], split[3]);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show($"Proxy doesn't seem to be in the correct format:{Environment.NewLine}{Environment.NewLine}IP:Port:Username:Pass {Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    CefSharpSettings.Proxy = proxy;
                    var rc = chrome.GetBrowser().GetHost().RequestContext;

                    Cef.UIThreadTaskFactory.StartNew(delegate {
                        rc.SetProxy($"{proxy.IP}:{proxy.Port}", out var error);
                    });

                    ProxySet = true;
                }
            }

            chrome.Load(URL);
        }

        public async static void CheckExistingAccountOnEmail()
        {

            var myScript = "(() => { var element = document.getElementsByClassName('create_newaccount_intro'); return element[0].innerText; })();";
            var task = chrome.EvaluateScriptAsync(myScript);
            var response = await task;

            if (response.Result.ToString() == "If you prefer, you can make a new, separate Steam account.")
            {
                Main.chrome.ExecuteScriptAsync($"EmailConfirmedVerified(0);");
            }
        }

        public async static void CheckConfirmEmailDialog()
        {
            while (true)
            {
                try
                {
                    var script = "(() => { var element = document.getElementById('error_display'); return element.innerText; })();";
                    var task = chrome.EvaluateScriptAsync(script);
                    var response = await task;

                    if (response.Success == true && response.Result.ToString().Contains("Reference ID:"))
                    {
                        Main._Form1.Invoke(new Action(() => Main._Form1.ChangerProxy(Main._Form1.txtUrl.Text)));
                        break;
                    }

                     var myScript = "(() => { var element = document.getElementsByClassName('newmodal'); return element[0].innerText; })();";
                     task = chrome.EvaluateScriptAsync(myScript);
                     response = await task;

                    if (response.Success == true && response.Result.ToString().Contains("VERIFY YOUR EMAIL"))
                    {
                        Thread.Sleep(200);
                        Main._Form1.btn_ConfirLink.Invoke(new Action(() => Main._Form1.btn_ConfirLink.PerformClick()));
                        break;
                    }
                    else Thread.Sleep(100);
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            
        }

        private void btn_Open_Email_File_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Open Text File";
                theDialog.Filter = "TXT files|*.txt";
                theDialog.InitialDirectory = Application.StartupPath;
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    EmailFilePath = theDialog.FileName;

                    EMAIl_LIST.Clear();
                    var lista = File.ReadAllLines(theDialog.FileName);

                    foreach (var email in lista)
                    {
                        var split = email.Split(':');
                        E_Mail mail = new E_Mail { EMAIL = split[0], PASS = split[1] };
                        EMAIl_LIST.Add(mail);
                    }

                    lbl_Email_Load.Text = EMAIl_LIST.Count.ToString();
                    lbl_Email_Load.ForeColor = Color.DarkCyan;
                    lbl_Email_Load.Font = new Font("Arial", 10, FontStyle.Bold);
                    Log.info($"{EMAIl_LIST.Count} E-Mails Load!");
                    SaveConfig();
                    SynchronizeEmailListWithDatabase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR To Read File!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lbl_Email_Load.Text = "ERROR";
                lbl_Email_Load.ForeColor = Color.Red;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void btn_ConfirLink_Click(object sender, EventArgs e)
        {
            if(email == null)
            {
                MessageBox.Show("E-Mail is empty!!", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            btn_ConfirLink.Enabled = false;
            btn_GetEmail.Enabled = false;
            Thread th = new Thread(() => AccessEmailPop3Client.Get_URL_Confirm(Main.email));
            th.IsBackground = true;
            th.Start();
        }

        private void btn_GenLoginPass_Click(object sender, EventArgs e)
        {
            GenLoginPassFuction();
        }

        public static void GenLoginPassFuction()
        {
            if (Main._Form1.ck_GenRandoLoginNameFake.Checked)
            {
                Main._Form1.lbl_Login.Invoke(new Action(() => Main._Form1.lbl_Login.Text = RandomUtils.RandomLoginUsingNamefake()));
            }
            else
            {
                Main._Form1.lbl_Login.Invoke(new Action(() => Main._Form1.lbl_Login.Text = RandomUtils.RandomLoginCustomFormat(Main._Form1.txt_CustomLoginGeneratorFormat.Text)));
            }

            Main._Form1.lbl_Pass.Invoke(new Action(() => Main._Form1.lbl_Pass.Text = RandomUtils.RandomPassword()));

            Main.chrome.ExecuteScriptAsync($"document.getElementById('accountname').value = '{Main._Form1.lbl_Login.Text}'");
            Thread.Sleep(500);
            Main.chrome.ExecuteScriptAsync($"document.getElementById('accountname').onchange();");

            Main.chrome.ExecuteScriptAsync($"document.getElementById('password').value = '{Main._Form1.lbl_Pass.Text}'");
            Thread.Sleep(500);
            Main.chrome.ExecuteScriptAsync($"document.getElementById('password').onkeyup();");

            Main.chrome.ExecuteScriptAsync($"document.getElementById('reenter_password').value = '{Main._Form1.lbl_Pass.Text}'");
            Thread.Sleep(500);
            Main.chrome.ExecuteScriptAsync($"document.getElementById('reenter_password').onkeyup();");

            Thread.Sleep(800);
            Main.chrome.ExecuteScriptAsync($"CompleteCreateAccount();");
        }

        public static void AddedEmail(string Email)
        {
            Main.chrome.ExecuteScriptAsync($"document.getElementById('email').value = '{Email}'");
            Main.chrome.ExecuteScriptAsync($"document.getElementById('reenter_email').value = '{Email}'");
            Main.chrome.ExecuteScriptAsync($"document.getElementById('i_agree_check').checked = 'True'");
            Thread.Sleep(250);
            Main.chrome.ExecuteScriptAsync($"CreateAccount()");
        }

        private void btn_GetEmail_Click(object sender, EventArgs e)
        {
            try
            {
                int _MaxAccInEmail = Convert.ToInt32(txt_MaxAccInMail.Text);
                MaxAccInEmail = _MaxAccInEmail;
            }
            catch(Exception ex)
            {
                Log.error(ex.Message);
                MessageBox.Show("Maximum accounts per e-mail input is invalid!", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (EMAIl_LIST.Count == 0)
            {
                MessageBox.Show("E-Mail List is empty!!", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            btn_GetEmail.Enabled = false;

            Thread th = new Thread(() => ManageEmails.Get_Mail(Main.MaxAccInEmail));
            th.IsBackground = true;
            th.Start();
        }

        private void btn_SaveAcc_Click(object sender, EventArgs e)
        {
            SaveAccountInfo();
        }

        public static void SaveAccountInfo()
        {
            string path_to_save = Path.Combine(Main.Acc_Create_Path, $"{Main._Form1.lbl_Login.Text}.txt");

            if (string.IsNullOrWhiteSpace(Main._Form1.lbl_Login.Text))
            {
                MessageBox.Show("Login is empty!!", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrWhiteSpace(Main._Form1.lbl_Pass.Text))
            {
                MessageBox.Show("Pass is empty!!", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!File.Exists(path_to_save))
            {
                StreamWriter sw;
                sw = File.AppendText(path_to_save);

                sw.WriteLine(Main._Form1.lbl_Login.Text + ":" + Main._Form1.lbl_Pass.Text +
                    "\r\n\r\nEmail: " + Main.email.EMAIL +
                    "\r\nEMail Password: " + Main.email.PASS);

                sw.Close();
                sw.Dispose();
            }

            Log.info("Account Create: " + Main._Form1.lbl_Login.Text);

            ManageEmails.Add_Mail_To_DB(Main.email.EMAIL);


            Main._Form1.Invoke(new Action(() => {

                Main._Form1.lbl_OpenAccFile.Text = $"{Main._Form1.lbl_Login.Text}.txt";
                Main._Form1.lbl_OpenAccFile.Visible = true;

                DirectoryInfo di = new DirectoryInfo(Main.Acc_Create_Path);
                FileInfo[] accsCreated = di.GetFiles("*.txt");

                Main._Form1.lbl_AmoutAccsCreated.Text = $"{accsCreated.Count()}";
                Main._Form1.lbl_AmoutAccsCreated.Visible = true;

                string login = Main._Form1.lbl_Login.Text;
                string pass = Main._Form1.lbl_Pass.Text;


                Thread th = new Thread(() => Customize_profile.Login_An_Customize(path_to_save, login, pass));
                th.IsBackground = true;
                th.Start();

                File.AppendAllText(LoginAndPassFilePath, login + ":" + pass + "\n");

                Main._Form1.lbl_Email.Text = "";

                Main._Form1.lbl_Login.Text = "";
                Main._Form1.lbl_Pass.Text = "";
                Main._Form1.btn_GenLoginPass.Enabled = false;
                Main._Form1.btn_GetEmail.Enabled = true;

                Main._Form1.ChangerProxy("https://store.steampowered.com/join/?l=english");
            }));
        }


        private void lbl_OpenAccFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string path = Path.Combine(Main.Acc_Create_Path, lbl_OpenAccFile.Text);

                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = path;
                myProcess.Start();
            }
            catch
            {

            }
            
        }

        private void btn_CopyLogin_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(lbl_Login.Text))
                Clipboard.SetText(lbl_Login.Text);
        }

        private void btn_CopyPass_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(lbl_Pass.Text))
                Clipboard.SetText(lbl_Pass.Text);
        }

        private void btn_CopyEmail_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(lbl_Email.Text))
               Clipboard.SetText(lbl_Email.Text);
        }

        private void btn_open_File_avatarURL_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Open Text File";
                theDialog.Filter = "TXT files|*.txt";
                theDialog.InitialDirectory = Application.StartupPath;
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    AvatarImageFilePath = theDialog.FileName;

                    Avatar_URL_List.Clear();
                    var lista = File.ReadAllLines(theDialog.FileName);
                    foreach (var email in lista)
                    {
                        Avatar_URL_List.Add(email);
                    }

                    lbl_Avatar_Load.Text = Avatar_URL_List.Count.ToString();
                    lbl_Avatar_Load.ForeColor = Color.DarkCyan;
                    lbl_Avatar_Load.Font = new Font("Arial", 10, FontStyle.Bold);
                    Log.info(Avatar_URL_List.Count + " Avatars Load!");
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR To Read File!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lbl_Email_Load.Text = "ERROR";
                lbl_Email_Load.ForeColor = Color.Red;
            }
        }

        private void ck_use_custom_avatar_CheckedChanged(object sender, EventArgs e)
        {
            if (ck_use_custom_avatar.Checked == true)
            {
                gp_box_avatar.Enabled = true;
            }
            else
            {
                gp_box_avatar.Enabled = false;
            }
        }

        public static void SaveConfig()
        {
            Config config = new Config();
            config.AvatarImageFilePath = Main._Form1.AvatarImageFilePath;
            config.EmailFilePath = Main._Form1.EmailFilePath;
            config.SingleProxyChecked = Main._Form1.ckUseSingleProxy.Checked;
            config.SingleProxyText = Main._Form1.txt_SingleProxy.Text;
            config.LoginGeneratorCustomFormat = Main._Form1.txt_CustomLoginGeneratorFormat.Text;
            config.UseCustomLoginGenerator = Main._Form1.ck_GenerateUseCustomFormat.Checked;

            try { config.AccountPerEmail = Convert.ToInt32(Main._Form1.txt_MaxAccInMail.Text); } catch { }

            System.IO.File.WriteAllText(Database_Path + "Config.json", JsonConvert.SerializeObject(config, Formatting.Indented));//salvar o arquivo appids atualizado

            Log.info("Config Save..");
        }

        public static void LoadConfig()
        {
            if(File.Exists(Database_Path + "Config.json"))
            {
                try
                {
                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Database_Path + "Config.json"));

                    if (config.AccountPerEmail != 0) Main._Form1.txt_MaxAccInMail.Text = config.AccountPerEmail.ToString();

                    if (config.AvatarImageFilePath != "")
                    {
                        if (!File.Exists(config.AvatarImageFilePath))
                        {
                            Log.error($"Could not find a part of the path {config.AvatarImageFilePath}");
                        }
                        else
                        {
                            Main._Form1.AvatarImageFilePath = config.AvatarImageFilePath;

                            Avatar_URL_List.Clear();
                            var lista = File.ReadAllLines(config.AvatarImageFilePath);
                            foreach (var email in lista)
                            {
                                Avatar_URL_List.Add(email);
                            }

                            Main._Form1.lbl_Avatar_Load.Text = Avatar_URL_List.Count.ToString();
                            Main._Form1.lbl_Avatar_Load.ForeColor = Color.DarkCyan;
                            Main._Form1.lbl_Avatar_Load.Font = new Font("Arial", 10, FontStyle.Bold);
                            Log.info(Avatar_URL_List.Count + " Avatars Load!");
                            Main._Form1.ck_use_custom_avatar.Checked = true;
                        }
                    }

                    if (config.EmailFilePath != "")
                    {

                        Main._Form1.EmailFilePath = config.EmailFilePath;

                        EMAIl_LIST.Clear();
                        var lista = File.ReadAllLines(config.EmailFilePath);

                        foreach (var email in lista)
                        {
                            var split = email.Split(':');
                            E_Mail mail = new E_Mail { EMAIL = split[0], PASS = split[1] };
                            EMAIl_LIST.Add(mail);
                        }

                        Main._Form1.lbl_Email_Load.Text = EMAIl_LIST.Count.ToString();
                        Main._Form1.lbl_Email_Load.ForeColor = Color.DarkCyan;
                        Main._Form1.lbl_Email_Load.Font = new Font("Arial", 10, FontStyle.Bold);
                        Log.info(EMAIl_LIST.Count + " E-Mails Load!");
                        SynchronizeEmailListWithDatabase();
                    }
                    if (config.SingleProxyChecked)
                    {
                        Main._Form1.ckUseSingleProxy.Checked = config.SingleProxyChecked;
                    }

                    if (!string.IsNullOrWhiteSpace(config.SingleProxyText))
                    {
                        Main._Form1.txt_SingleProxy.Text = config.SingleProxyText;
                    }

                    if (!string.IsNullOrWhiteSpace(config.LoginGeneratorCustomFormat))
                    {
                        Main._Form1.txt_CustomLoginGeneratorFormat.Text = config.LoginGeneratorCustomFormat;
                    }

                    if (config.UseCustomLoginGenerator)
                    {
                        Main._Form1.ck_GenerateUseCustomFormat.Checked = true;
                    }
                    else
                    {
                        Main._Form1.ck_GenRandoLoginNameFake.Checked = true;
                    }

                }
                catch(Exception ex)
                {
                    Log.error($"Error to load Config: {ex.Message}");
                }
            }

            if (File.Exists(Pop3Domains_Path))
            {
                pop3s = JsonConvert.DeserializeObject<List<Pop3>>(File.ReadAllText(Main.Pop3Domains_Path));

                Main._Form1.lbl_TotalDomainsConfig.Text = pop3s.Count().ToString();

                if(pop3s.Count() > 0)
                  Main._Form1.lbl_TotalDomainsConfig.ForeColor = Color.DarkGreen;
            }
        }

        private void btn_OpenCriationPage_Click(object sender, EventArgs e)
        {
            if (chrome.IsLoading)
            {
                MessageBox.Show($"Wait WebBrowser is loading a page...\r\n\r\n{chrome.Address}", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                ChangerProxy("https://store.steampowered.com/join/?l=english");
            }
        }

        private void btn_OpenFormAddedDomain_Click(object sender, EventArgs e)
        {
            frm_AddDomain frm = new frm_AddDomain();
            frm.ShowDialog();
        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

            switch(e.ClickedItem.Name)
            {

                case "github":
                    {
                        Process myProcess = new Process();
                        myProcess.StartInfo.UseShellExecute = true;
                        myProcess.StartInfo.FileName = "https://github.com/Cappi1998/SteamAccountCreateHelper";
                        myProcess.Start();
                        break;
                    }
                case "restart":
                    {
                        System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
                        this.Close();
                        break;
                    }

                default: { break; }

            }

        }

        public static void SynchronizeEmailListWithDatabase()
        {
            UsedEmailDatabase mAIL_DATABASE = JsonConvert.DeserializeObject<UsedEmailDatabase>(File.ReadAllText(Main.Used_Mail_DB_Path));

            foreach (var usedmail in mAIL_DATABASE.AlreadyUsed)
            {
                var ml = Main.EMAIl_LIST.Where(a => a.EMAIL == usedmail.EMAIL).FirstOrDefault();

                if (ml != null)
                {
                    ml.LinkedAccounts = usedmail.LinkedAccounts;
                }
            }
        }

        private void ckUseSingleProxy_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void txt_SingleProxy_Leave(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void btn_GoToUrl_Click(object sender, EventArgs e)
        {

            if (chrome.IsLoading)
            {
                ShowMessageBox($"Wait WebBrowser is loading a page...\r\n\r\n{chrome.Address}", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                ChangerProxy(txtUrl.Text);
            }
        }
        private void btn_ConfigureCustomFormat_Click(object sender, EventArgs e)
        {
            frm_customLoginConfigure frm = new frm_customLoginConfigure();
            frm.ShowDialog();
        }
        private void txt_CustomLoginGeneratorFormat_Leave(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void ck_GenRandoLoginNameFake_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void ckGenerateUseCustomFormat_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void btn_GenLoginPass_MouseHover(object sender, EventArgs e) {
            btn_GenLoginPass.BackColor = Color.FromArgb(117, 207, 255);
        }

        private void btn_GenLoginPass_MouseLeave(object sender, EventArgs e) {
            btn_GenLoginPass.BackColor = Color.FromArgb(192, 194, 196);
        }

        private void btn_GetEmail_MouseHover(object sender, EventArgs e) {
            btn_GetEmail.BackColor = Color.FromArgb(117, 207, 255);
        }

        private void btn_GetEmail_MouseLeave(object sender, EventArgs e) {
            btn_GetEmail.BackColor = Color.FromArgb(192, 194, 196);
        }

        public void ShowMessageBox(string msg, string title, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
        {
            MessageBox.Show(msg, title, messageBoxButtons, messageBoxIcon);
        }

        public static void GenLoginAndPassAutomatic()
        {
            while (true)
            {
                if (chrome.Address.Contains("store.steampowered.com/join/completesignup"))
                {
                    if (!chrome.IsLoading)
                    {
                        Main._Form1.btn_GenLoginPass.Invoke(new Action(() => Main._Form1.btn_GenLoginPass.Enabled = false));
                        GenLoginPassFuction();
                        Main._Form1.btn_GenLoginPass.Invoke(new Action(() => Main._Form1.btn_GenLoginPass.Enabled = false));
                        break;
                    }
                    else Thread.Sleep(100);
                }
            }
        }

        private void btn_LoadProxys_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Open Text File";
                theDialog.Filter = "TXT files|*.txt";
                theDialog.InitialDirectory = Application.StartupPath;
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    //AvatarImageFilePath = theDialog.FileName;

                    Proxy_URL_List.Clear();
                    var lista = File.ReadAllLines(theDialog.FileName);
                    foreach (var email in lista)
                    {
                        Proxy_URL_List.Add(email);
                    }

                    lbl_ProxyCount.Text = Proxy_URL_List.Count.ToString();
                    lbl_ProxyCount.ForeColor = Color.DarkCyan;
                    lbl_ProxyCount.Font = new Font("Arial", 10, FontStyle.Bold);
                    Log.info(Proxy_URL_List.Count + " Proxy Load!");
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR To Read File!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lbl_ProxyCount.Text = "ERROR";
                lbl_ProxyCount.ForeColor = Color.Red;
            }
        }

        private void btn_ChangerProxy_Click(object sender, EventArgs e)
        {
            if(Proxy_URL_List.Count == 0)
            {
                MessageBox.Show("No Proxy List Loaded!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                txt_SingleProxy.Text = Proxy_URL_List[0];
                Proxy_URL_List.RemoveAt(0);
                ProxySet = false;

                lbl_ProxyCount.Text = Proxy_URL_List.Count.ToString();
                lbl_ProxyCount.ForeColor = Color.DarkCyan;
                lbl_ProxyCount.Font = new Font("Arial", 10, FontStyle.Bold);

                ChangerProxy(txtUrl.Text);
                btn_GetEmail.Enabled = true;
            }
        }

        private void btn_saveAccount_Click(object sender, EventArgs e)
        {
            SaveAccountInfo();
        }
    }
}
