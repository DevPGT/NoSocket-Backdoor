using System;
using System.Windows.Forms;

namespace Explorer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Client.Listener.HandleListener();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
        }
    }
}
