using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal interface IWinFormsLogWriter
    {
        void Append(ListBox listBox, string message);
    }
}
