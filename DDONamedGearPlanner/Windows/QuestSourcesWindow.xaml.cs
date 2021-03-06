﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace DDONamedGearPlanner
{
	/// <summary>
	/// Interaction logic for QuestSourcesWindow.xaml
	/// </summary>
	public partial class QuestSourcesWindow : Window
	{
		List<QuestSourceItemData> QuestSourceList = new List<QuestSourceItemData>();
		bool Initializing = true;

		public QuestSourcesWindow()
		{
			InitializeComponent();

			foreach (var ap in DatasetManager.Dataset.AdventurePacks)
				QuestSourceList.Add(new QuestSourceItemData() { Pack = ap, Allow = QuestSourceManager.IsAllowed(ap.Name) });

			lbQuestSources.ItemsSource = QuestSourceList;

			Initializing = false;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			QuestSourceManager.SaveSettings();
		}

		void SetAllQuestSources(bool allowed)
		{
			foreach (var qp in QuestSourceList)
				qp.Allow = allowed;
		}

		private void Toggle_Checked(object sender, RoutedEventArgs e)
		{
			if (!Initializing) SetAllQuestSources(true);
		}

		private void Toggle_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!Initializing) SetAllQuestSources(false);
		}
	}

	public class QuestSourceItemData : INotifyPropertyChanged
	{
		public DDOAdventurePackData Pack;
		public string Name => Pack.Name + (Pack.FreeToVIP ? " (VIP)" : "");

		bool _Allow;
		public bool Allow
		{
			get { return _Allow; }
			set
			{
				_Allow = value;
				QuestSourceManager.SetQuestSourceAllowed(Pack.Name, value, false);
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
