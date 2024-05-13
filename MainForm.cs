using eDrawings.Interop.EModelViewControl;
using System.Diagnostics;

namespace CadConversion
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private EModelViewControl? m_ctrl;
        private ClosePopupHook _popupHook;
        public MainForm(AppSettings settings)
        {
            _settings = settings;
            _popupHook = new ClosePopupHook();
            InitializeComponent();
            InitializeEDrawingsHost();
            // These options will not display the window, but still render the images
            // Probably need to disable them in testing
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Opacity = 0;  // Make the window completely transparent.
            this.Visible = false;  // Hide the form.
        }

        private void InitializeEDrawingsHost()
        {
            var host = new EDrawingsHost();
            host.ControlLoaded += OnControlLoaded;
            this.Controls.Add(host);
            host.Dock = DockStyle.Fill;
        }

        private void OnControlLoaded(EModelViewControl ctrl)
        {
            m_ctrl = ctrl;
            m_ctrl.OnFinishedLoadingDocument += OnFinishedLoadingDocument;
            m_ctrl.OnFailedLoadingDocument += OnFailedLoadingDocument;
            m_ctrl.OnFinishedSavingDocument += OnFinishedSavingDocument;
            m_ctrl.OnFailedSavingDocument += OnFailedSavingDocument;

            ProcessNext();
        }

        private void OnFinishedLoadingDocument(string fileName)
        {
            if (m_ctrl == null)
                throw new InvalidOperationException("eDrawing control is not initialized.");

            Console.WriteLine($"Loaded {fileName}");
            string outputFilePath = Path.Combine(_settings.OutputDirectory, GenerateRandomFilename());
            // This is where the conversion and saving happens
            m_ctrl.Save(outputFilePath, false, "");
            CsvFileProcessing(fileName, outputFilePath);
        }

        private string GenerateRandomFilename()
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 10)
                              .Select(s => s[random.Next(s.Length)]).ToArray()) + _settings.OutputFormat;
        }

        private void CsvFileProcessing(string inputFilePath, string outputFilePath)
        {
            bool fileExists = File.Exists(_settings.LogCsvFile);
            if (!fileExists)
            {
                string header = "inputFile, outputFile\n";
                File.AppendAllText(_settings.LogCsvFile, header);
            }
            string row = $"\"{inputFilePath}\",\"{outputFilePath}\"\n";
            File.AppendAllText(_settings.LogCsvFile, row);
        }

        private void OnFailedLoadingDocument(string fileName, int errorCode, string errorString)
        {
            Console.WriteLine($"failed to load {fileName}: {errorString}");
            ProcessNext();
        }

        private void OnFinishedSavingDocument()
        {
            ProcessNext();
        }

        private void OnFailedSavingDocument(string fileName, int ErrorCode, string errorString)
        {
            Console.WriteLine($"failed to save {fileName}: {errorString}");
            ProcessNext();
        }

        private void ProcessNext()
        {
            if (m_ctrl == null)
                throw new InvalidOperationException("eDrawing control is not initialized.");

            if (_settings.InputFiles.Count != 0)
            {
                string filePath = _settings.InputFiles[0];
                _settings.InputFiles.RemoveAt(0);
                m_ctrl.CloseActiveDoc("");
                m_ctrl.OpenDoc(filePath, false, false, false, "");
            }
            else
            {
                Console.WriteLine("Completed");
                Application.Exit();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _popupHook.Dispose();
            base.OnFormClosed(e);
        }
    }
}