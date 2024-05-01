namespace CadConversion
{
    
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static void Main(string[] args)
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            ApplicationConfiguration.Initialize();
            var settings = ConfigurationManager.ParseArguments(args);
            Application.Run(new MainForm(settings));
        }
    }
}