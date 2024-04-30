using eDrawings.Interop.EModelViewControl;

namespace CadConversion
{
    public partial class MainForm : Form
    {
        private EModelViewControl? m_ctrl;
        public MainForm()
        {
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