using eDrawings.Interop.EModelViewControl;
using System.Diagnostics;

namespace CadConversion
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private EModelViewControl? m_ctrl;
        public MainForm(AppSettings settings)
        {
            _settings = settings;
            // Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText("trace.txt")));
            // Trace.WriteLine(settings.OutputDirectory);
            // Trace.WriteLine(settings.LogFile);
            // Trace.WriteLine(settings.OutputFormat);
            // Trace.Flush();
            InitializeComponent();
            var host = new EDrawingsHost();
            host.ControlLoaded += OnControlLoaded;
            this.Controls.Add(host);
            host.Dock = DockStyle.Fill;
        }

        private void OnControlLoaded(EModelViewControl ctrl)
        {
            m_ctrl = ctrl;
        }
    }
}