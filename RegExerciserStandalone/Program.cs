using RegExerciser;
using System;
using System.Windows;

namespace RegExerciserStandalone
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var app = new Application();
            var window = new RegexEditor();
            app.MainWindow = window;
            app.Run(window);
        }
    }
}
