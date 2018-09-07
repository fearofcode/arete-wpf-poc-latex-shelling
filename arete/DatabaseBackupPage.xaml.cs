using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace arete
{
    public partial class DatabaseBackupPage : Page
    {
        public static DatabaseBackupPage instance = new DatabaseBackupPage();

        private BackgroundWorker backgroundWorker;
        
        public DatabaseBackupPage()
        {
            InitializeComponent();

            backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, args) =>
            {
                string backupFolderProperty = PropertyConfiguration.backupPath;
                string backupExecutablePath = PropertyConfiguration.pgDumpPath;
                string databaseName = PropertyConfiguration.databaseName;
                string databaseUserName = PropertyConfiguration.databaseUserName;

                if (backupFolderProperty == null || backupExecutablePath == null || databaseName == null
                        || databaseUserName == null)
                {
                    MessageBox.Show("Invalid properties config");
                    Application.Current.Shutdown();
                    return;
                }

                string backupPath = Path.Combine(backupFolderProperty, "arete_backup.sql");

                string backupTempFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sql";
                
                var startInfo = new ProcessStartInfo();
                var p = new Process();

                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.Arguments = $"-d {databaseName} -U {databaseUserName} -f \"{backupTempFile}\"";
                startInfo.FileName = backupExecutablePath;

                p.StartInfo = startInfo;
                p.Start();

                args.Result = p.StandardOutput.ReadToEnd();
                args.Result += p.StandardError.ReadToEnd();
                p.WaitForExit();

                File.Copy(backupTempFile, backupPath, overwrite: true);
            };

            backgroundWorker.RunWorkerCompleted += (s, args) =>
            {
                progressBox.Text = $"Output: |{(string)args.Result}|";
                string backupPath = Path.Combine(PropertyConfiguration.backupPath, "arete_backup.sql");
                string backupContents = File.ReadAllText(backupPath);
                progressBox.Text += $"\n{backupContents}";
                progressBox.Text += $"\nLast modified: {File.GetLastWriteTime(backupPath)}";

                DoBackupButton.IsEnabled = true;
            };
        }

        private void DoBackupButton_Click(object sender, RoutedEventArgs e)
        {
            DoBackupButton.IsEnabled = false;

            progressBox.Text = "";
            backgroundWorker.RunWorkerAsync();
        }

        private void ProgressBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (progressBox != null)
            {
                progressBox.ScrollToEnd();
            }

            e.Handled = true;
        }
    }
}
