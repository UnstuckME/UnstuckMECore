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
    /// Interaction logic for SocialPage.xaml
    /// </summary>
    public partial class SocialPage : Page
    {
        public SocialPage()
        {
            InitializeComponent();
        }

        public void NotificationCall(string username)
        {
            SocialText.Text = username + " Sent A Message";
        }
    }
}
