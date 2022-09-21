using Avalonia.Controls;
using System;

namespace G64Installer.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            Installer.GetReleases();
        }
    }
}
