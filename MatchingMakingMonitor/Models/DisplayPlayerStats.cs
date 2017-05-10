﻿using MatchMakingMonitor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MatchMakingMonitor.View.Util;

namespace MatchMakingMonitor.Models
{
	public class DisplayPlayerStats : BaseViewBinding
	{
		private static readonly string[] TierStrings = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
		public PlayerShip Player { get; set; }

		private SolidColorBrush _background;
		public SolidColorBrush Background
		{
			get => _background;
			set
			{
				_background = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorName = Brushes.Black;
		public SolidColorBrush ColorName
		{
			get => _colorName;
			set
			{
				_colorName = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorWinRate = Brushes.Black;
		public SolidColorBrush ColorWinRate
		{
			get => _colorWinRate;
			set
			{
				_colorWinRate = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorAvgXp = Brushes.Black;
		public SolidColorBrush ColorAvgXp
		{
			get => _colorAvgXp;
			set
			{
				_colorAvgXp = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorAvgFrags = Brushes.Black;
		public SolidColorBrush ColorAvgFrags
		{
			get => _colorAvgFrags;
			set
			{
				_colorAvgFrags = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorAvgDamage = Brushes.Black;
		public SolidColorBrush ColorAvgDamage
		{
			get => _colorAvgDamage;
			set
			{
				_colorAvgDamage = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorBattles = Brushes.Black;
		public SolidColorBrush ColorBattles
		{
			get => _colorBattles;
			set
			{
				_colorBattles = value;
				FirePropertyChanged();
			}
		}

		private SolidColorBrush _colorBorder = Brushes.Transparent;
		public SolidColorBrush ColorBorder
		{
			get => _colorBorder;
			set
			{
				_colorBorder = value;
				FirePropertyChanged();
			}
		}

		public string TextBattles => $"Battles: {Player.Battles}";
		public string TextWins => $"Wins: {Player.Wins}";
		public string TextWinRate => $"WinRate: {WinRate}%";
		public string TextAvgXp => $"Avg XP: {AvgXp}";
		public string TextAvgFrags => $"Avg Frags: {AvgFrags}";
		public string TextAvgDamage => $"Avg Damage: {AvgDamage}";
		public string TextName => $"{Player?.Nickname} | {AccountId}";

		public string TextShipName => $"{ShipName} (Tier {Player.ShipTier})";


		public string[] CommandParams => new[] { AccountId, Player.Nickname };

		public string ShipName => Player?.ShipName;
		public string AccountId => Player?.AccountId.ToString();

		public double WinRate { get; private set; }
		public double AvgXp { get; private set; }
		public double AvgFrags { get; private set; }
		public double AvgDamage { get; private set; }

		private int _colorNameKey;
		private int _colorWinRateKey;
		private int _colorAvgFragsKey;
		private int _colorAvgXpKey;
		private int _colorAvgDamageKey;
		private int _colorBattlesKey;


		public Visibility Visibility => Player.IsPrivateOrHidden ? Visibility.Collapsed : Visibility.Visible;

		public Visibility TextVisibility => !Player.IsPrivateOrHidden ? Visibility.Collapsed : Visibility.Visible;

		public SettingsWrapper SettingsWrapper { get; set; }
		public DisplayPlayerStats(SettingsWrapper settingsWrapper, PlayerShip player) : this()
		{
			Player = player;
			SettingsWrapper = settingsWrapper;
			WinRate = Math.Round(player.Wins / player.Battles * 100, 2);
			AvgFrags = Math.Round(player.Frags / player.Battles, 2);
			AvgXp = Math.Round(player.XpEarned / player.Battles, 0);
			AvgDamage = Math.Round(player.DamageDealt / player.Battles, 0);

			ComputeUi();
		}

		public DisplayPlayerStats()
		{

		}

		public static DisplayPlayerStats MockPlayer(int relation = 1, bool privateOrHidden = false)
		{
			return new DisplayPlayerStats()
			{
				Player = new PlayerShip() { Nickname = "Test", AccountId = 12323325, ShipName = "Name", IsPrivateOrHidden = privateOrHidden, Relation = relation },
				WinRate = 40,
				AvgFrags = 5,
				AvgXp = 1234,
				AvgDamage = 123242
			};
		}

		public void ComputeUi()
		{
			try
			{
				double totalRating = 0;
				if (!Player.IsPrivateOrHidden)
				{
					ColorWinRate = GetColor(WinRate, SettingsWrapper.CurrentSettings.WinLimits.Single(l => l.ShipTier == Player.ShipTier).Values.Select(v => v.Value), totalRating, SettingsWrapper.CurrentSettings.WinWeight, out totalRating,
						out _colorWinRateKey);
					ColorAvgFrags = GetColor(AvgFrags, SettingsWrapper.CurrentSettings.FragsLimits.Single(l => l.ShipTier == Player.ShipTier).Values.Select(v => v.Value), totalRating, SettingsWrapper.CurrentSettings.FragsWeight, out totalRating,
						out _colorAvgFragsKey);
					ColorAvgXp = GetColor(AvgXp, SettingsWrapper.CurrentSettings.XpLimits.Single(l => l.ShipTier == Player.ShipTier).Values.Select(v => v.Value), totalRating, SettingsWrapper.CurrentSettings.XpWeight, out totalRating,
						out _colorAvgXpKey);
					ColorAvgDamage = GetColor(AvgDamage, SettingsWrapper.CurrentSettings.DmgLimits.GetLimits(Player.ShipType, Player.ShipTier).Select(v => v.Value), totalRating,
						SettingsWrapper.CurrentSettings.DmgWeight, out totalRating, out _colorAvgDamageKey);
					ColorBattles = GetColor(Player.Battles, SettingsWrapper.CurrentSettings.BattleLimits.Single(l => l.ShipTier == Player.ShipTier).Values.Select(v => v.Value), totalRating, SettingsWrapper.CurrentSettings.BattleWeight, out totalRating,
						out _colorBattlesKey);

					_colorNameKey = (int)Math.Floor(totalRating / 5);
					if (_colorNameKey == 0) _colorNameKey = 1;
					ColorName = SettingsWrapper.Brushes[_colorNameKey - 1];
				}

				var color = ColorName.CloneCurrentValue();
				color.Opacity = 0.08;
				color.Freeze();
				Background = color;

				ColorBorder = Player.Relation == 0 ? ColorName : Brushes.Transparent;
			}
			catch (Exception e)
			{
				IoCKernel.Get<ILogger>().Error("Error during UI computation", e);
			}
		}

		#region Colors

		private SolidColorBrush GetColor(double value, IEnumerable<double> limits, double oldTotal, double multiplier, out double total, out int key)
		{
			var limitsList = limits.ToArray();
			for (var i = 0; i < limitsList.Length; i++)
			{
				if (!(value >= limitsList[i])) continue;
				key = i + 1;
				total = oldTotal + (key * multiplier);
				return SettingsWrapper.Brushes[i];
			}
			total = oldTotal + (9 * multiplier);
			key = 9;
			return Brushes.Black;
		}
		#endregion

		public MobileDisplayPlayerStats ToMobile()
		{
			return new MobileDisplayPlayerStats()
			{
				Relation = Player.Relation,
				PrivateOrHidden = Player.IsPrivateOrHidden,
				DisplayName = TextName,
				Name = Player.Nickname,
				AccountId = AccountId,
				ShipName = ShipName,
				Battles = Player.Battles,
				Wins = Player.Wins,
				WinRate = WinRate,
				AvgXp = AvgXp,
				AvgFrags = AvgFrags,
				AvgDamage = AvgDamage,

				ColorNameKey = _colorNameKey,
				ColorWinRateKey = _colorWinRateKey,
				ColorAvgFragsKey = _colorAvgFragsKey,
				ColorAvgXpKey = _colorAvgXpKey,
				ColorAvgDamageKey = _colorAvgDamageKey,
				ColorBattlesKey = _colorBattlesKey,
			};
		}
	}
}
