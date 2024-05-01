A Windows Forms application to convert CAD files using eDrawings.

Build the application with `dotnet build`

The **CadConversion.exe** takes the following command line arguments:

- `-input` One or more input files or directories. For directories, all files from all subdirectories will be processed.
- `-outdir` The output directory to write the converted files to. Default is a directory named `outdir` relative to the current directory.
- `-format` The format to convert the files to. Like **stl** or **png**. Default is `png`.
- `-filter` A regex for how to select files if a directory is passed to -input. Default is `_._`.
