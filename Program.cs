namespace CadConversion
{
    
    internal static class Program
    {
        // These lines only needed for testing
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static void Main(string[] args)
        {
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                Console.WriteLine("Failed to attach console.");
            }
            try
            {
                ApplicationConfiguration.Initialize();
                var settings = ConfigurationManager.ParseArguments(args);
                Application.Run(new MainForm(settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }
    }
}