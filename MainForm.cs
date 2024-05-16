using eDrawings.Interop.EModelViewControl;
using System.Diagnostics;
using System.Threading;
using System;
using System.IO;
using System.Security.Cryptography;

namespace CadConversion
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private EModelViewControl? m_ctrl;
        private List<EMVViewOrientation> _viewOrientations = new List<EMVViewOrientation>
        {
            EMVViewOrientation.eMVOrientationIsoMetric,
            EMVViewOrientation.eMVOrientationFront,
            EMVViewOrientation.eMVOrientationRight,
            EMVViewOrientation.eMVOrientationBack,
            EMVViewOrientation.eMVOrientationLeft,
            EMVViewOrientation.eMVOrientationTop,
            EMVViewOrientation.eMVOrientationBottom
        };
        private int _currentViewIndex;
        private string? _currentInputFile;
        private string? _currentFileId;

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
            _currentViewIndex = 0;
            _currentInputFile = fileName;
            SaveNextView();
        }

        private void SaveNextView()
        {
            if (m_ctrl == null)
                throw new InvalidOperationException("eDrawing control is not initialized.");
            if (_currentInputFile == null)
                throw new InvalidProgramException("_currentInputFile should not be null here");
            if (_currentViewIndex < _viewOrientations.Count)
            {
                var currentView = _viewOrientations[_currentViewIndex];
                m_ctrl.ViewOrientation = currentView;
                m_ctrl.UpdateScene();
                // Thread.Sleep(2000);

                string suffix = GetViewSuffix(currentView);
                string outputFilePath = Path.Combine(_settings.OutputDirectory, $"{_currentFileId}_{suffix}.png");
                m_ctrl.Save(outputFilePath, false, "");
                CsvFileProcessing(_currentInputFile, outputFilePath);

                _currentViewIndex++;
            }
            else
            {
                ProcessNext();
            }
        }

        private string GetViewSuffix(EMVViewOrientation viewOrientation)
        {
            return viewOrientation switch
            {
                EMVViewOrientation.eMVOrientationIsoMetric => "iso",
                EMVViewOrientation.eMVOrientationFront => "front",
                EMVViewOrientation.eMVOrientationRight => "right",
                EMVViewOrientation.eMVOrientationBack => "back",
                EMVViewOrientation.eMVOrientationLeft => "left",
                EMVViewOrientation.eMVOrientationTop => "top",
                EMVViewOrientation.eMVOrientationBottom => "bottom",
                _ => "unknown"
            };
        }

        private string ComputeMd5Hash(string filePath)
        {
            Console.WriteLine($"Computing Md5 Hash of {filePath}");
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    Console.WriteLine($"Computed Hash if {hash}");
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
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
            SaveNextView();
        }

        private void OnFailedSavingDocument(string fileName, int ErrorCode, string errorString)
        {
            Console.WriteLine($"failed to save {fileName}: {errorString}");
            SaveNextView();
        }

        private void ProcessNext()
        {
            if (m_ctrl == null)
                throw new InvalidOperationException("eDrawing control is not initialized.");

            if (_settings.InputFiles.Count != 0)
            {
                string filePath = _settings.InputFiles[0];
                _settings.InputFiles.RemoveAt(0);
                _currentFileId = ComputeMd5Hash(filePath);
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
            base.OnFormClosed(e);
        }
    }
}
