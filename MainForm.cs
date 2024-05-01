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
            InitializeComponent();
            InitializeEDrawingsHost();
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
            LogFileProcessing(fileName, outputFilePath);
        }

        private string GenerateRandomFilename()
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 10)
                              .Select(s => s[random.Next(s.Length)]).ToArray()) + _settings.OutputFormat;
        }

        private void LogFileProcessing(string inputFilePath, string outputFilePath)
        {
            string logMessage = $"Processed '{inputFilePath}' and created '{outputFilePath}'\n";
            File.AppendAllText(_settings.LogFile, logMessage);
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
    }
}