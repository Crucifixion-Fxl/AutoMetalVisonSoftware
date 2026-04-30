using System.IO;
using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal class FileDialogService : IFileDialogService
    {
        public string SelectFolder(IWin32Window owner, string currentPath = null)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
                {
                    folderDialog.SelectedPath = currentPath;
                }

                return folderDialog.ShowDialog(owner) == DialogResult.OK
                    ? folderDialog.SelectedPath
                    : string.Empty;
            }
        }

        public string SelectFile(IWin32Window owner, string filter, string currentPath = null)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    try
                    {
                        var baseDir = Path.GetDirectoryName(currentPath);
                        if (!string.IsNullOrWhiteSpace(baseDir) && Directory.Exists(baseDir))
                        {
                            dialog.InitialDirectory = baseDir;
                        }
                    }
                    catch
                    {
                    }
                }

                return dialog.ShowDialog(owner) == DialogResult.OK
                    ? dialog.FileName
                    : string.Empty;
            }
        }
    }
}
