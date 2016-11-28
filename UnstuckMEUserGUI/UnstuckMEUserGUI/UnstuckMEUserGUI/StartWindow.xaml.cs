﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UnstuckMEUserGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            //if valid login
            Window disp = new MainWindow();
            disp.Show();
            this.Close();
        }

        private void CreateAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            Window disp = new NewAccountSetupWindow();
            disp.Show();
            this.Close();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Window disp = new UserLoginSettingsWindow();
            disp.Show();            
        }
    }
}
