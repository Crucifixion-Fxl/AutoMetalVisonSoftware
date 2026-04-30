using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal class WinFormsLogWriter : IWinFormsLogWriter
    {
        private readonly int _maxLineLength;

        public WinFormsLogWriter(int maxLineLength = 80)
        {
            _maxLineLength = Math.Max(20, maxLineLength);
        }

        public void Append(ListBox listBox, string message)
        {
            var wrappedLines = Wrap(message, _maxLineLength);
            string timePrefix = $"{DateTime.Now:HH:mm:ss} - ";
            for (int i = 0; i < wrappedLines.Count; i++)
            {
                string prefix = i == 0 ? timePrefix : new string(' ', timePrefix.Length);
                listBox.Items.Add(prefix + wrappedLines[i]);
            }
            listBox.TopIndex = listBox.Items.Count - 1;
        }

        private static List<string> Wrap(string message, int maxLineLength)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(message))
            {
                lines.Add(string.Empty);
                return lines;
            }

            var rawLines = message.Replace("\r\n", "\n").Split('\n');
            foreach (var raw in rawLines)
            {
                string remaining = raw;
                while (remaining.Length > maxLineLength)
                {
                    int breakPos = remaining.LastIndexOf(' ', maxLineLength);
                    if (breakPos <= 0)
                    {
                        breakPos = maxLineLength;
                    }

                    lines.Add(remaining.Substring(0, breakPos).TrimEnd());
                    remaining = remaining.Substring(breakPos).TrimStart();
                }
                lines.Add(remaining);
            }
            return lines;
        }
    }
}
