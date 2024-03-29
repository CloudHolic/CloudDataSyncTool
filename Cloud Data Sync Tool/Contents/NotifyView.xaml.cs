﻿using CloudSync.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace CloudSync.Contents
{
    /// <summary>
    /// NotifyView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NotifyView : CustomDialog
    {
        public NotifyView(string title, string notifyText)
        {
            InitializeComponent();
            DataContext = new NotifyViewModel(title, notifyText);
        }
    }
}