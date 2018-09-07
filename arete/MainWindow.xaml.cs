using System.Windows;
using System.Windows.Input;

namespace arete
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand GotoLatexShellingPage = new RoutedUICommand
            (
                "Go to LaTeX Page",
                "Go to LaTeX Page",
                typeof(MainWindow),
                new InputGestureCollection() { new KeyGesture(Key.F1) }
            );

        public static readonly RoutedUICommand GotoCodePage = new RoutedUICommand
            (
                "Go to Python Page",
                "Go to Python Page",
                typeof(MainWindow),
                new InputGestureCollection() { new KeyGesture(Key.F2) }
            );

        public static readonly RoutedUICommand GotoDatabaseBackupPage = new RoutedUICommand
            (
                "Go to Database Backup Page",
                "Go to Database Backup Page",
                typeof(MainWindow),
                new InputGestureCollection() { new KeyGesture(Key.F3) }
            );

        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(GotoLatexShellingPage, GotoLatexShellingPage_Executed));
            CommandBindings.Add(new CommandBinding(GotoCodePage, GotoCodePageCommand_Executed));
            CommandBindings.Add(new CommandBinding(GotoDatabaseBackupPage, GotoDatabaseBackupPage_Executed));

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPythonPage();
        }
        
        private void GotoLatexShellingPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLatexShellingPage();
        }

        private void GotoDatabaseBackupPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowBackupPage();
        }

        private void GotoCodePageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowPythonPage();
        }

        private void LatexPageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLatexShellingPage();
        }

        private void CodePageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPythonPage();
        }

        private void DatabasePageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBackupPage();
        }

        private void ShowLatexShellingPage()
        {
            Main.Content = LatexShellingPage.instance;
            LatexShellingPage.instance.inputBox.Focus();
        }

        private void ShowPythonPage()
        {
            Main.Content = CodeEvaluationPage.instance;
            // needed for keyboard shortcuts to work!
            CodeEvaluationPage.instance.BackgroundWorkerButton.Focus();
        }

        private void ShowBackupPage()
        {
            Main.Content = DatabaseBackupPage.instance;
        }
    }
}
