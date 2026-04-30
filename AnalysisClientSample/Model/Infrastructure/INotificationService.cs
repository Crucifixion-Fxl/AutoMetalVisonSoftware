using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal interface INotificationService
    {
        void ShowWarning(IWin32Window owner, string message);
        void ShowInfo(IWin32Window owner, string message);
        void ShowError(IWin32Window owner, string message);
    }
}
