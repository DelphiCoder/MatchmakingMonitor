﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MatchMakingMonitor.config;
using MatchMakingMonitor.config.Reflection;

namespace MatchMakingMonitor.View.Settings
{
	public class ColorsEditor
	{
		private Color _color1;
		private Color _color2;
		private Color _color3;
		private Color _color4;
		private Color _color5;
		private Color _color6;
		private Color _color7;
		private Color _color8;
		private Color _color9;

		private readonly BehaviorSubject<ChangedSetting> _changedSubject;
		private readonly string[] _values;
		private Action _valuesChanged;
		private readonly bool _initial;
		public ColorsEditor(BehaviorSubject<ChangedSetting> changedSubject, ref string[] values)
		{
			_changedSubject = changedSubject;
			_values = values;
			_initial = true;
			LoadValues();
			_initial = false;
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void LoadValues()
		{
			_color1 = (Color)ColorConverter.ConvertFromString(_values[0]);
			_color2 = (Color)ColorConverter.ConvertFromString(_values[1]);
			_color3 = (Color)ColorConverter.ConvertFromString(_values[2]);
			_color4 = (Color)ColorConverter.ConvertFromString(_values[3]);
			_color5 = (Color)ColorConverter.ConvertFromString(_values[4]);
			_color6 = (Color)ColorConverter.ConvertFromString(_values[5]);
			_color7 = (Color)ColorConverter.ConvertFromString(_values[6]);
			_color8 = (Color)ColorConverter.ConvertFromString(_values[7]);
			_color9 = (Color)ColorConverter.ConvertFromString(_values[8]);
			_valuesChanged?.Invoke();
			if (!_initial)
			{
				_changedSubject.OnNext(new ChangedSetting(true, false));
			}
		}

		public void RegisterValuesChanged(Action action)
		{
			_valuesChanged = action;
		}

		public Color Color1 => _color1;
		public Color Color2 => _color2;
		public Color Color3 => _color3;
		public Color Color4 => _color4;
		public Color Color5 => _color5;
		public Color Color6 => _color6;
		public Color Color7 => _color7;
		public Color Color8 => _color8;
		public Color Color9 => _color9;

		private bool UpdateColor(Color color, int index, ref Color field)
		{
			var oldColor = field;
			var newColor = color.ToString();
			_values[index] = newColor;
			_changedSubject.OnNext(new ChangedSetting(oldColor, newColor, "UISetting"));
			return true;
		}

		public bool UpdateColor1(Color color)
		{
			return UpdateColor(color, 0, ref _color1);
		}

		public bool UpdateColor2(Color color)
		{
			return UpdateColor(color, 1, ref _color2);
		}

		public bool UpdateColor3(Color color)
		{
			return UpdateColor(color, 2, ref _color3);
		}

		public bool UpdateColor4(Color color)
		{
			return UpdateColor(color, 3, ref _color4);
		}

		public bool UpdateColor5(Color color)
		{
			return UpdateColor(color, 4, ref _color5);
		}

		public bool UpdateColor6(Color color)
		{
			return UpdateColor(color, 5, ref _color6);
		}

		public bool UpdateColor7(Color color)
		{
			return UpdateColor(color, 6, ref _color7);
		}

		public bool UpdateColor8(Color color)
		{
			return UpdateColor(color, 7, ref _color8);
		}

		public bool UpdateColor9(Color color)
		{
			return UpdateColor(color, 8, ref _color9);
		}
	}
}
