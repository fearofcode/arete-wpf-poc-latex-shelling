using System;
using System.Linq;
using System.IO;
using System.Windows;

namespace arete
{
    class PropertyConfiguration
    {
        static private readonly string CONFIGURATION_PATH = "application.properties";

        static private readonly object _lock = new object();
        static public bool parsed = false;
        static public readonly string editorPath;
        static public readonly string latexBinDirectory;
        static public readonly string backupPath;
        static public readonly string pgDumpPath;
        static public readonly string databaseName;
        static public readonly string databaseUserName;
        static public readonly string pythonPath;

        static PropertyConfiguration()
        {
            lock(_lock)
            {
                if (parsed)
                {
                    return;
                }
                
                if (!File.Exists(CONFIGURATION_PATH))
                {
                    string cwd = Directory.GetCurrentDirectory().ToString();
                    MessageBox.Show($"Cannot find configuration file {CONFIGURATION_PATH}. Please place it in {cwd} and try again.");
                    Application.Current.Shutdown();
                    return;
                }

                var lines = File.ReadAllLines(CONFIGURATION_PATH);
                
                foreach (string line in lines)
                {
                    string[] tokens = line.Split('=');
                    if (tokens.Count() != 2)
                    {
                        throw new ArgumentException($"Could not properly parse line {line}");
                    }

                    string key = tokens[0].Trim();
                    string value = tokens[1].Trim();
                    
                    switch(key)
                    {
                        case "editorPath":
                            editorPath = value;
                            break;
                        case "latexBinDirectory":
                            latexBinDirectory = value;
                            break;
                        case "backupPath":
                            backupPath = value;
                            break;
                        case "pgDumpPath":
                            pgDumpPath = value;
                            break;
                        case "databaseName":
                            databaseName = value;
                            break;
                        case "databaseUserName":
                            databaseUserName = value;
                            break;
                        case "pythonPath":
                            pythonPath = value;
                            break;
                        default:
                            throw new ArgumentException($"Got unknown key {key} in {CONFIGURATION_PATH}");
                    }
                }

                parsed = true;
            }
        }
    }
}
