using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace arete
{
    public partial class CodeEvaluationPage : Page
    {
        public static CodeEvaluationPage instance = new CodeEvaluationPage();

        private StringBuilder stringBuilder;

        private BackgroundWorker backgroundWorker;

        public CodeEvaluationPage()
        {
            InitializeComponent();

            backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };

            stringBuilder = new StringBuilder(1000);

            CommandBindings.Add(new CommandBinding(new RoutedUICommand
            (
                "Evaluate Code",
                "Evaluate Code",
                typeof(CodeEvaluationPage),
                new InputGestureCollection() { new KeyGesture(Key.F5) }
            ), EvaluateCode_Executed));

            backgroundWorker.DoWork += (s, args) =>
            {
                backgroundWorker.ReportProgress(0);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                Process p = new Process();

                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;

                startInfo.UseShellExecute = false;
                startInfo.Arguments = @"C:\Users\Warren\Downloads\evaluator_test.py";
                startInfo.FileName = PropertyConfiguration.pythonPath;

                p.StartInfo = startInfo;
                p.Start();

                backgroundWorker.ReportProgress(100, p.StandardOutput.ReadToEnd());
                backgroundWorker.ReportProgress(100, p.StandardError.ReadToEnd());

                p.WaitForExit();
            };

            backgroundWorker.ProgressChanged += (s, args) =>
            {
                stringBuilder.AppendLine((string)args.UserState);
                progressBox.Text = stringBuilder.ToString();
                progressBox.ScrollToEnd();
            };

            backgroundWorker.RunWorkerCompleted += (s, args) =>
            {
                BackgroundWorkerButton.IsEnabled = true;

                stringBuilder = new StringBuilder(1000);
            };
        }

        private void EvaluateCode()
        {
            BackgroundWorkerButton.IsEnabled = false;

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            EvaluateCode();
        }
        
        private void ProgressBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (progressBox != null)
            {
                progressBox.ScrollToEnd();
            }

            e.Handled = true;
        }

        private void EvaluateCode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EvaluateCode();
        }
    }
}
