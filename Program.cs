namespace CadConversion
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            var settings = ConfigurationManager.ParseArguments(args);
            Application.Run(new MainForm(settings));
        }
    }
}