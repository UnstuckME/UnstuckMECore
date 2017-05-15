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
using System.Windows.Shapes;
using System.Windows.Threading;
using UnstuckME_Classes;

namespace UnstuckMEUserGUI
{
    /// <summary>
    /// Interaction logic for ReconnectingWindow.xaml
    /// </summary>
    public partial class ReconnectingWindow : Window
    {
        TimeSpan _time;
        private readonly DispatcherTimer _timer;
        public ReconnectingWindow()
        {
            InitializeComponent();
            _time = TimeSpan.Zero;
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if(_time.Seconds < 10)
                {
                    TimerLabel.Content = string.Format("{0}:0{1}", _time.Minutes, _time.Seconds);
                }
                else
                {
                    TimerLabel.Content = string.Format("{0}:{1}", _time.Minutes, _time.Seconds);
                }
                _time = _time.Add(TimeSpan.FromSeconds(1));
            }, Application.Current.Dispatcher);
        }

        private void REWindow_ContentRendered(object sender, EventArgs e)
        {
            //try
            //{
            //    while (UnstuckME.ChannelFactory.State != System.ServiceModel.CommunicationState.Opened)
            //    {
            //        UnstuckME.ConnectToServer();
            //    }
            //    UserInfo test = new UserInfo();
            //    test = UnstuckME.Server.UserLoginAttempt(UnstuckME.User.EmailAddress, UnstuckME.UPW);
            //    this.Close();
            //    UnstuckME.MainWindow.Show();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void ReconnectButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                while (UnstuckME.ChannelFactory.State != System.ServiceModel.CommunicationState.Opened)
                {
                    UnstuckME.ConnectToServer();
                    if(_time.Minutes > 5)
                    {
                        throw new Exception("Reconnecting Time Exceeded 5 Minutes.");
                    }
                }
                UserInfo test = new UserInfo();
                test = UnstuckME.Server.UserLoginAttempt(UnstuckME.User.EmailAddress, UnstuckME.UPW);
                this.Close();
                UnstuckME.MainWindow.Show();
            }
            catch (Exception)
            {
                TimerLabel.Visibility = Visibility.Hidden;
                ReconnectingLabel.Visibility = Visibility.Hidden;
                FailedLabel.Visibility = Visibility.Visible;
                TryAgainLabel.Visibility = Visibility.Visible;
                ExitButton.Visibility = Visibility.Visible;
                _timer.Stop();
            }
        }

        private void ReconnectButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ReconnectButton.Background = Brushes.IndianRed;
        }

        private void ReconnectButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ReconnectButton.Background = UnstuckME.Red;
        }

        private void ExitButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ReconnectButton.Background = Brushes.IndianRed;
        }

        private void ExitButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ExitButton.Background = UnstuckME.Red;
        }

        private void ExitButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UnstuckME.UserExit = true;
            App.Current.Shutdown();
        }
    }
}