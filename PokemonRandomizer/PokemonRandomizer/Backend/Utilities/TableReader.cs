using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class TableReader
    {
        public static void TableToDictFormat(string path, char separator, int maxWidth, int columns = 5)
        {
            string[] text = File.ReadAllLines(path, Encoding.UTF7);
            List<string> newText = new List<string>();
            int i = 0;
            string last = string.Empty;
            foreach (string s in text)
            {
                string[] sp = s.Split(',');
                string formatText = "{0x" + sp[0] + ",'" + sp[1] + "'},";
                if (i == 0)
                    newText.Add(formatText);
                else
                    newText[newText.Count - 1] += new string(' ', maxWidth - last.Length) + formatText;
                i = ++i == columns ? 0 : i;
                last = formatText;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "txt file|*.txt",
                Title = "Save Table Text"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, newText.ToArray());
            }
        }

    }
}
