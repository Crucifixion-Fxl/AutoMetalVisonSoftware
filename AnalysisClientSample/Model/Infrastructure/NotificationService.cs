using System;
using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal class NotificationService : INotificationService
    {
        public void ShowWarning(IWin32Window owner, string message)
        {
            Show(owner, message, "提示", MessageBoxIcon.Warning);
        }

        public void ShowInfo(IWin32Window owner, string message)
        {
            Show(owner, message, "提示", MessageBoxIcon.Information);
        }

        public void ShowError(IWin32Window owner, string message)
        {
            Show(owner, message, "错误", MessageBoxIcon.Error);
        }

        private void Show(IWin32Window owner, string message, string title, MessageBoxIcon icon)
        {
            if (owner is Control control && control.InvokeRequired)
            {
                control.Invoke(new Action(() => Show(owner, message, title, icon)));
                return;
            }

            MessageBox.Show(owner, message, title, MessageBoxButtons.OK, icon);
        }
    }
}
