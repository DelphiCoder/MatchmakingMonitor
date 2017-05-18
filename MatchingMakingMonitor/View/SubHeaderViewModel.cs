﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using MatchMakingMonitor.config;
using MatchMakingMonitor.config.Reflection;
using MatchMakingMonitor.Services;
using MatchMakingMonitor.View.Util;

namespace MatchMakingMonitor.View
{
	public class SubHeaderViewModel : ViewModelBase
	{
		public RelayCommand PathClickCommand { get; set; }

		public IEnumerable<Region> Regions { get; } = new List<Region> { Region.NA, Region.EU, Region.RU, Region.ASIA };

		private Region _region;

		public Region Region
		{
			get => _region;
			set
			{
				var oldValue = _region;
				_region = value;
				FirePropertyChanged();
				_settingsWrapper.CurrentSettings.Region = _region;
				_settingsWrapper.SettingChangedSubject.OnNext(new ChangedSetting(oldValue, _region, nameof(SettingsJson.Region)));
			}
		}


		private string _installDirectoryText = "Check install directory";

		public string InstallDirectoryText
		{
			get => _installDirectoryText;
			set
			{
				_installDirectoryText = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _installDirectoryColor;

		public SolidColorBrush InstallDirectoryColor
		{
			get => _installDirectoryColor;
			set
			{
				_installDirectoryColor = value;
				FirePropertyChanged();
			}
		}

		private string _statusText = "Not currently in Battle...";

		public string StatusText
		{
			get => _statusText;
			set
			{
				_statusText = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _statusColor = Brushes.Black;

		public SolidColorBrush StatusColor
		{
			get => _statusColor;
			set
			{
				_statusColor = value;
				FirePropertyChanged();
			}
		}

		private bool _enableUi = true;
		public bool EnableUi
		{
			get => _enableUi;
			set
			{
				_enableUi = value;
				FirePropertyChanged();
			}
		}

		private Visibility _showProgress = Visibility.Collapsed;
		public Visibility ShowProgress
		{
			get => _showProgress;
			set
			{
				_showProgress = value;
				FirePropertyChanged();
			}
		}

		private readonly SettingsWrapper _settingsWrapper;

		public SubHeaderViewModel(SettingsWrapper settingsWrapper, StatsService statsService)
		{
			_settingsWrapper = settingsWrapper;

			PathClickCommand = new RelayCommand(PathClicked);

			_region = _settingsWrapper.CurrentSettings.Region;

			_settingsWrapper.SettingChanged(nameof(SettingsJson.InstallDirectory)).Subscribe(s => InitPath());
			statsService.StatsStatusChanged.Subscribe(status =>
			{
				SetStatusText(status);
				if (status == StatsStatus.Fetching)
				{
					EnableUi = false;
					ShowProgress = Visibility.Visible;
				}
				else
				{
					EnableUi = true;
					ShowProgress = Visibility.Collapsed;
				}
			});
		}

		public SubHeaderViewModel()
		{
		}

		private void InitPath()
		{
			var directory = _settingsWrapper.CurrentSettings.InstallDirectory;
			if (Directory.Exists(Path.Combine(directory, "replays")) && File.Exists(Path.Combine(directory, "WorldOfWarships.exe")))
			{
				InstallDirectoryColor = Brushes.Green;
				InstallDirectoryText = directory;
			}
			else
			{
				InstallDirectoryColor = Brushes.OrangeRed;
				InstallDirectoryText = directory + " - Invalid Path or Replays not enabled! - Click here to update!";
			}
		}

		private void PathClicked()
		{
			var folderBrowser = new FolderBrowserDialog();
			var result = folderBrowser.ShowDialog();
			if (result == DialogResult.OK && !string.IsNullOrEmpty(folderBrowser.SelectedPath))
			{
				var oldValue = _settingsWrapper.CurrentSettings.InstallDirectory;
				_settingsWrapper.CurrentSettings.InstallDirectory = folderBrowser.SelectedPath;
				_settingsWrapper.SettingChangedSubject.OnNext(new ChangedSetting(oldValue, _settingsWrapper.CurrentSettings.InstallDirectory, nameof(SettingsJson.InstallDirectory)));
			}
		}

		private void SetStatusText(StatsStatus status)
		{
			switch (status)
			{
				case StatsStatus.Fetching:
					StatusText = "Fetching player stats for current battle";
					StatusColor = Brushes.DarkOrange;
					break;
				case StatsStatus.Fetched:
					StatusText = "Player Stats Succesfully Updated";
					StatusColor = Brushes.Green;
					break;
				case StatsStatus.WrongRegion:
					StatusText = "Found a battle but no stats. Wrong region?";
					StatusColor = Brushes.Red;
					break;
				case StatsStatus.Waiting:
					StatusText = "Not Currently in Battle - Stats are from Last Battle";
					StatusColor = Brushes.Black;
					break;
				default:
					StatusText = string.Empty;
					StatusColor = Brushes.Black;
					break;
			}
		}
	}
}
