﻿using System.Windows.Controls;
using CloudSync.Models;
using CloudSync.ViewModels;

namespace CloudSync.Controls
{
    /// <summary>
    /// SchemaEntry.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SchemaEntry : UserControl
    {
        public SchemaEntry(TableList tables, bool isSrc, bool isSelectable = true)
        {
            InitializeComponent();
            TableList.IsHitTestVisible = isSelectable;
            DataContext = new SchemaEntryViewModel(tables, isSrc);
        }
    }
}
