using System;
using System.Linq;
using System.IO;

namespace CadConversion
{
    public class AppSettings
    {
        public List<string> InputFiles { get; private set; }
        public string OutputDirectory { get; private set; }
        public string LogFile {get; private set; }
        public string OutputFormat {get; private set; }
        public AppSettings(List<string> inputFiles, string outputDirectory, string logFile, string outputFormat)
        {
            InputFiles = inputFiles;
            OutputDirectory = outputDirectory;
            LogFile = logFile;
            OutputFormat = outputFormat;
        }
    }

    public static class ConfigurationManager
    {
        private const string ARG_INPUT = "-input";
        private const string ARG_FILTER = "-filter";
        private const string DEFAULT_FILTER = "*.*";
        private const string ARG_OUTPUT_DIR = "-outdir";
        private const string DEFAULT_OUTDIR = "outdir";
        private const string ARG_FORMAT = "-format";
        private const string DEFAULT_FORMAT = ".png";
       
        public static AppSettings ParseArguments(string[] args)
        {
            var inputs = new List<string>();
            var filters = new List<string>();
            var outDirs = new List<string>();
            var formats = new List<string>();

            List<string>? curList = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(ARG_INPUT, StringComparison.CurrentCultureIgnoreCase))
                {
                    curList = inputs;
                }
                else if (args[i].Equals(ARG_FILTER, StringComparison.CurrentCultureIgnoreCase))
                {
                    curList = filters;
                }
                else if (args[i].Equals(ARG_OUTPUT_DIR, StringComparison.CurrentCultureIgnoreCase))
                {
                    curList = outDirs;
                }
                else if (args[i].Equals(ARG_FORMAT, StringComparison.CurrentCultureIgnoreCase))
                {
                    curList = formats;
                }
                else
                {
                    if (curList != null)
                    {
                        curList.Add(args[i]);
                    }
                    else
                    {
                        throw new ArgumentException("Arguments are invalid, specify the correct switch");
                    }
                }
            }

            string outputDirectory = outDirs.FirstOrDefault() ?? DEFAULT_OUTDIR;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyy-MM-dd-HHmmss");
            string filename = $"CadConversion-{timestamp}.log";
            string LogFile = Path.Combine(outputDirectory, $"CadConversion-{timestamp}.log");

            string outputFormat = formats.FirstOrDefault() ?? DEFAULT_FORMAT;
            if (!outputFormat.StartsWith("."))
            {
                outputFormat = "." + outputFormat;
            }
            
            var filter = filters.FirstOrDefault() ?? DEFAULT_FILTER;

            if (!inputs.Any())
            {
                throw new ArgumentException($"Inputs are not specified. Use {ARG_INPUT} switch to specify the input directory(s) or file(s)");
            }

            var inputFiles = new List<string>();
            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    inputFiles.AddRange(Directory.GetFiles(input, filter, SearchOption.AllDirectories).ToList());
                }
                else if (File.Exists(input))
                {
                    inputFiles.Add(input);
                }
                else
                {
                    throw new Exception("Specify input file or directory");
                }
            }

            return new AppSettings(inputFiles, outputDirectory, LogFile, outputFormat);
        }
    }
}