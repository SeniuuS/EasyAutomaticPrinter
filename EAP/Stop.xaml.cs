/*
  Easy Automatic Printer  - The Printer Utility
  Copyright (C) 2016 SeniuuS.

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License version 3 as published by
  the Free Software Foundation.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace EAP
{
    /// <summary>
    /// Logique d'interaction pour Stop.xaml
    /// </summary>
    public partial class Stop : Window
    {
        private const string RESUME = "Resume";
        private const string PAUSE = "Pause";

        private ManualResetEvent _retry = new ManualResetEvent(false);
        public ManualResetEvent Retry
        {
            get
            {
                return _retry;
            }
        }

        private ManualResetEvent _pause = new ManualResetEvent(false);
        public ManualResetEvent Pause
        {
            get
            {
                return _pause;
            }
        }

        private bool _skip = false;
        public bool Skip {
            get
            {
                return _skip;
            }
            set
            {
                _skip = value;
            }
        }

        private bool _stopped = false;
        public bool Stopped
        { 
            get
            {
                return _stopped;
            }
        }

        private CancellationTokenSource _printToken;
        public CancellationTokenSource SearchDocToken { get; set; }

        public Stop(IList<Document> list, CancellationTokenSource token)
        {
            InitializeComponent();

            int nb = 0;
            foreach (var doc in list)
                nb += doc.Number;
            MaxNumber.Text = nb.ToString();
            _printToken = token;
        }

        private void btn_StopClick(object sender, RoutedEventArgs e)
        {
            _pause.Set();
            _retry.Set();
            if(_printToken != null)
                _printToken.Cancel();
            if(SearchDocToken != null)
                SearchDocToken.Cancel();
            _stopped = true;
            Close();
        }

        private void btn_Pause_ResumeClick(object sender, RoutedEventArgs e)
        {
            switch (btn_Pause_Resume.Content.ToString())
            {
                case PAUSE:
                    _pause.Reset();
                    btn_Pause_Resume.Content = RESUME;
                    break;
                case RESUME:
                    _pause.Set();
                    btn_Pause_Resume.Content = PAUSE;
                    break;
            }
        }

        private void btn_SkipClick(object sender, RoutedEventArgs e)
        {
            _skip = true;
            _retry.Set();
        }

        private void btn_RetryClick(object sender, RoutedEventArgs e)
        {
            _retry.Set();
        }

        public void ActualizationActualNumber(int actualNumber)
        {
            ActualNumber.Text = actualNumber.ToString();
        }

        public void skipVisibility(Visibility mode)
        {
            btn_Skip.Visibility = mode;
        }

        public void retryVisibility(Visibility mode)
        {
            btn_Retry.Visibility = mode;
        }
    }
}
