using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace arete
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand RenderOutputCommand = new RoutedUICommand
            (
                "Render LaTeX output",
                "Render LaTeX output",
                typeof(MainWindow),
                new InputGestureCollection() { new KeyGesture(Key.F5) }
            );

        private static string latexSubDirectory = "arete_latex_processing";
        private string tempBase = Path.Combine(Path.GetTempPath(), latexSubDirectory);

        private string latexPath;
        private string dviPngPath;

        private static readonly int fontSize = 36; // will become variable later

        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        
        struct JobResult
        {
            public string output;
            public string pngPath;
        }


        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(RenderOutputCommand, RenderOutput_Executed));

            Loaded += MainWindow_Loaded;

            backgroundWorker.DoWork += (s, args) =>
            {
                JobResult result = new JobResult() { output = "", pngPath = "" };

                var wrappedMarkup = GenerateWrappedMarkup((string)args.Argument);

                double currentMs = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var tempLatexFile = Path.Combine(tempBase, $"arete_output{currentMs}.tex");
                
                File.WriteAllText(tempLatexFile, wrappedMarkup);

                Console.WriteLine(tempLatexFile);

                /* TODO notice that there are no quotes around aux-directory and output-directory. LaTeX appears to shit the bed
                when you provide quotes. I tried it with quotes first and it didn't work. It didn't work until I removed the
                quotes. I'll have to check the MikTeX source to figure out WTF is going on.
                 */
                var startInfo = new ProcessStartInfo();
                var p = new Process();

                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                startInfo.Arguments = $"-halt-on-error -aux-directory={tempBase} -output-directory={tempBase} \"{tempLatexFile}\"";
                startInfo.FileName = latexPath;

                p.StartInfo = startInfo;
                p.Start();

                result.output += p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                string expectedDviPath = tempLatexFile.Replace(".tex", ".dvi");

                if (!File.Exists(expectedDviPath))
                {
                    result.output += $"\nExpected DVI path '{expectedDviPath}' did not exist";
                    args.Result = result;
                    return;
                }

                string expectedPngPath = tempLatexFile.Replace(".tex", ".png");
                
                p = new Process();
                startInfo.Arguments = $"-T tight -D 96 -o \"{expectedPngPath}\" \"{expectedDviPath}\"";
                startInfo.FileName = dviPngPath;

                p.StartInfo = startInfo;
                p.Start();

                result.output += p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                if (!File.Exists(expectedPngPath))
                {
                    result.output += $"\nExpected PNG path '{expectedPngPath}' did not exist";
                    args.Result = result;
                    return;
                } else
                {
                    result.pngPath = expectedPngPath;
                }
                
                args.Result = result;
            };
            
            backgroundWorker.RunWorkerCompleted += (s, args) =>
            {
                BackgroundWorkerButton.IsEnabled = true;
                JobResult result = (JobResult)args.Result;
                progressBox.Text = result.output;
                if (result.pngPath != "" && result.pngPath != null)
                {
                    resultImage.Source = new BitmapImage(new Uri(result.pngPath));
                }
            };
        }

        public string PathForExecutableName(string executable)
        {
            string fullPath = Path.Combine(PropertyConfiguration.latexBinDirectory, executable + ".exe");

            if (!File.Exists(fullPath)) {
                throw new FileNotFoundException($"Path '{fullPath}' does not exist. Check your configuration.");
            }

            return fullPath;
        }

        public string GenerateWrappedMarkup(string markup)
        {
            return "\\documentclass[14pt]{extarticle}\n" +
                    "\\usepackage{amsmath}\n" +
                    "\\usepackage{amsthm}\n" +
                    "\\usepackage{amssymb}\n" + // TODO make this configurable
                    "\\newtheorem*{theorem}{Theorem}\n" +
                    "\\newtheorem*{proposition}{Proposition}\n" +
                    "\\begin{document}\n" +
                    markup + "\n" +
                    "\\end{document}";
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            new PropertyConfiguration();

            latexPath = PathForExecutableName("latex");
            dviPngPath = PathForExecutableName("dvipng");

            if (!File.Exists(tempBase))
            {
                Directory.CreateDirectory(tempBase);
            }
            
        }

        private void RenderOutput()
        {
            BackgroundWorkerButton.IsEnabled = false;

            progressBox.Text = "";
            backgroundWorker.RunWorkerAsync(inputBox.Text);
        }

        private void RenderOutput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RenderOutput();
        }

        private void BackgroundWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            RenderOutput();
        }
        
        /* keep selection from getting fucked up while text is possibly scrolling */

        private void progressBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (progressBox != null)
            {
                progressBox.ScrollToEnd();
            }
            
            e.Handled = true;
        }
    }
}
