using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CurrencyConverter.Services
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        string[] ReadAllLines(string path);
        void WriteAllLines(string path, IEnumerable<string> contents);
        void AppendAllText(string path, string content);
        string GetApplicationDataFolder();
    }

    public interface IDialogService
    {
        void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon);
    }

    public class PhysicalFileSystem : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
        public string[] ReadAllLines(string path) => File.ReadAllLines(path);
        public void WriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);
        public void AppendAllText(string path, string content) => File.AppendAllText(path, content);
        public string GetApplicationDataFolder() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    public class WindowsFormsDialogService : IDialogService
    {
        public void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, buttons, icon);
        }
    }
}
