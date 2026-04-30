using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal interface IFileDialogService
    {
        string SelectFolder(IWin32Window owner, string currentPath = null);
        string SelectFile(IWin32Window owner, string filter, string currentPath = null);
    }
}
