/*
 * This file is part of EmergencyLightsChanger, licensed under the MIT License (MIT).
 *
 * Copyright (c) Felix Schmidt <http://homepage.rub.de/Felix.Schmidt-c2n/>
 * Extended by NinjaNoobSlayer, March 28, 2020. <https://www.patreon.com/ninjamods>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EmergencyLightsManagerV2
{
	public class EmergencyLightsManagerV2 : ILoadingExtension, IUserMod
	{
		private enum Service
		{
			Police,
			Fire,
			Ambulance,
            Rotary
		}

		private enum Side
		{
			Left,
			Right
		}

		private enum Setting
		{
			Preset,
			PoliceLeft,
			PoliceRight,
			FireLeft,
			FireRight,
			AmbulanceLeft,
			AmbulanceRight,
			ManualRearFire,
			ManualRearAmbulance,
			FireLeftRear,
			FireRightRear,
			AmbulanceLeftRear,
			AmbulanceRightRear,
            SnowPlowLeft,
            SnowPlowRight
		}

		private const string CONFIG_PATH = "EmergencyLightsManagerV2.txt";

		private static Dictionary<string, Color> Colors = new Dictionary<string, Color>
		{
			{
				"White",
				new Color(1f, 0.98f, 0.96f)
			},
			{
				"Red",
				new Color(1f, 0f, 0f)
			},
			{
				"Blue",
				new Color(0f, 0.5f, 1f)
			},
			{
				"Light Blue",
				new Color(0.5f, 0.75f, 1f)
			},
			{
				"Green",
				new Color(0f, 1f, 0f)
			},
			{
				"Purple",
				new Color(0.5f, 0f, 1f)
			},
			{
				"Orange",
				new Color(1f, 0.75f, 0f)
			},
			{
				"Off",
				new Color(0f, 0f, 0f)
			}
		};

		private string[] ColorNames = Colors.Keys.ToArray();

		private static string[] EmergencyLightPresets = new string[7]
		{
			"Default",
			"Custom",
			"No Lights (All Off)",
			"American (Red-Blue)",
			"European (Blue-Blue)",
			"Japanese (Red-Red)",
            "Ideal Settings For Ninja's Vehicles"
		};

		private Dictionary<Setting, string> settings;

		private static Dictionary<Setting, string> defaultSettings = new Dictionary<Setting, string>
		{
			{
				Setting.Preset,
				"0"
			},
			{
				Setting.PoliceLeft,
				"Red"
			},
			{
				Setting.PoliceRight,
				"Blue"
			},
			{
				Setting.FireLeft,
				"Light Blue"
			},
			{
				Setting.FireRight,
				"Light Blue"
			},
			{
				Setting.AmbulanceLeft,
				"Red"
			},
			{
				Setting.AmbulanceRight,
				"Blue"
			},
			{
				Setting.ManualRearFire,
				"False"
			},
			{
				Setting.ManualRearAmbulance,
				"False"
			},
			{
				Setting.FireLeftRear,
				"Light Blue"
			},
			{
				Setting.FireRightRear,
				"Light Blue"
			},
			{
				Setting.AmbulanceLeftRear,
				"Red"
			},
			{
				Setting.AmbulanceRightRear,
				"Blue"
			},
            {Setting.SnowPlowLeft,
                "Orange"
            },
            {Setting.SnowPlowRight,
                "Orange"
            }
		};

		private bool loaded;

		private int _selectedPreset;

		private bool _customSettingsVisiblity;

		private UITabstrip strip;

		private UITabContainer container;

		private UICheckBox chkManualRearFire;

		private UICheckBox chkManualRearAmbulance;

		private UIDropDown ddFireLeftRear;

		private UIDropDown ddFireRightRear;

		private UIDropDown ddAmbulanceLeftRear;

		private UIDropDown ddAmbulanceRightRear;

		public string Name => "Emergency Lights Manager V2";

		public string Description => "Change colors of emergency vehicle lights and rotary lights - full customization.";

		private int SelectedPreset
		{
			get
			{
				return _selectedPreset;
			}
			set
			{
				if (_selectedPreset != value)
				{
					_selectedPreset = value;
					settings[Setting.Preset] = _selectedPreset.ToString();
				}
			}
		}

		private bool CustomSettingsVisibility
		{
			get
			{
				return _customSettingsVisiblity;
			}
			set
			{
				_customSettingsVisiblity = value;
				if (_customSettingsVisiblity)
				{
					strip.Show();
					container.Show();
				}
				else
				{
					strip.Hide();
					container.Hide();
				}
			}
		}

		private void LoadSettings()
		{
			if (settings != null)
			{
				return;
			}
			settings = new Dictionary<Setting, string>();
			if (File.Exists("EmergencyLightsManager.txt"))
			{
				string[] array = File.ReadAllText("EmergencyLightsManager.txt").Replace("\r", string.Empty).Split('\n');
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split('=');
					if (array3.Length == 2 && !(array3[0].Trim() == string.Empty) && !(array3[1].Trim() == string.Empty))
					{
						Setting key;
						try
						{
							key = (Setting)(int)Enum.Parse(typeof(Setting), array3[0]);
						}
						catch
						{
							continue;
						}
						settings[key] = array3[1];
					}
				}
			}
			foreach (int value in Enum.GetValues(typeof(Setting)))
			{
				if (!settings.ContainsKey((Setting)value))
				{
					settings.Add((Setting)value, defaultSettings[(Setting)value]);
				}
			}
			try
			{
				_selectedPreset = Convert.ToInt32(settings[Setting.Preset]);
				_selectedPreset = ((_selectedPreset >= 0) ? ((_selectedPreset <= EmergencyLightPresets.Length - 1) ? _selectedPreset : 0) : 0);
			}
			catch
			{
				_selectedPreset = 0;
			}
		}

		public void OnCreated(ILoading loading)
		{
			LoadSettings();
		}

		public void OnReleased()
		{
			ExportSettings();
		}

		private void ExportSettings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<Setting, string> setting in settings)
			{
				stringBuilder.AppendLine(setting.Key.ToString() + "=" + setting.Value.ToString());
			}
			File.WriteAllText("EmergencyLightsManager.txt", stringBuilder.ToString());
		}

		public void OnLevelLoaded(LoadMode mode)
		{
			loaded = true;
			Apply();
		}

		public void OnLevelUnloading()
		{
			loaded = false;
		}

		private void RearFireVisibility(bool chkd)
		{
			ddFireLeftRear.parent.isVisible = chkd;
			ddFireRightRear.parent.isVisible = chkd;
		}

		private void RearAmbulanceVisibility(bool chkd)
		{
			ddAmbulanceLeftRear.parent.isVisible = chkd;
			ddAmbulanceRightRear.parent.isVisible = chkd;
		}

		public void OnSettingsUI(UIHelperBase helper)
		{
			LoadSettings();
			UIDropDown uIDropDown = (UIDropDown)helper.AddDropdown("Select Preset", EmergencyLightPresets, SelectedPreset, delegate(int sel)
			{
				if (sel == 1)
				{
					CustomSettingsVisibility = true;
				}
				else
				{
					CustomSettingsVisibility = false;
				}
				SelectedPreset = sel;
				if (loaded)
				{
					Apply();
				}
			});
			UIScrollablePanel uIScrollablePanel = ((UIHelper)helper).self as UIScrollablePanel;
			uIScrollablePanel.autoLayout = false;
			int num = 100;
			int num2 = 10;
			int num3 = 40;
			strip = uIScrollablePanel.AddUIComponent<UITabstrip>();
			strip.relativePosition = new Vector3(num2, num);
			strip.size = new Vector2(744 - num2, num3);
			container = uIScrollablePanel.AddUIComponent<UITabContainer>();
			container.relativePosition = new Vector3(num2, num3 + num);
			container.size = new Vector3(744 - num2, 713 - num);
			strip.tabPages = container;
			UIButton uIButton = (UIButton)UITemplateManager.Peek("OptionsButtonTemplate");
			UIButton uIButton2 = strip.AddTab("Police Car", uIButton, fillText: true);
			uIButton2.textColor = uIButton.textColor;
			uIButton2.pressedTextColor = uIButton.pressedTextColor;
			uIButton2.hoveredTextColor = uIButton.hoveredTextColor;
			uIButton2.focusedTextColor = uIButton.hoveredTextColor;
			uIButton2.disabledTextColor = uIButton.hoveredTextColor;
			UIPanel uIPanel = strip.tabContainer.components[0] as UIPanel;
			uIPanel.autoLayout = true;
			uIPanel.wrapLayout = true;
			uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			UIHelper uIHelper = new UIHelper(uIPanel);
			uIHelper.AddSpace(15);
			uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.PoliceLeft]), delegate(int sel)
			{
				settings[Setting.PoliceLeft] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.PoliceRight]), delegate(int sel)
			{
				settings[Setting.PoliceRight] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIButton2 = strip.AddTab("Fire Truck");
			uIButton2.textColor = uIButton.textColor;
			uIButton2.pressedTextColor = uIButton.pressedTextColor;
			uIButton2.hoveredTextColor = uIButton.hoveredTextColor;
			uIButton2.focusedTextColor = uIButton.hoveredTextColor;
			uIButton2.disabledTextColor = uIButton.hoveredTextColor;
			uIPanel = (strip.tabContainer.components[1] as UIPanel);
			uIPanel.autoLayout = true;
			uIPanel.wrapLayout = true;
			uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			uIHelper = new UIHelper(uIPanel);
			uIHelper.AddSpace(15);
			uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.FireLeft]), delegate(int sel)
			{
				settings[Setting.FireLeft] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.FireRight]), delegate(int sel)
			{
				settings[Setting.FireRight] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddSpace(15);
			chkManualRearFire = (uIHelper.AddCheckbox("Configure Rear Lights Separately", Convert.ToBoolean(settings[Setting.ManualRearFire]), delegate(bool chkd)
			{
				settings[Setting.ManualRearFire] = chkd.ToString();
				ExportSettings();
				RearFireVisibility(chkd);
				if (loaded)
				{
					Apply();
				}
			}) as UICheckBox);
			chkManualRearFire.width = 744f;
			uIHelper.AddSpace(15);
			ddFireLeftRear = (UIDropDown)uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.FireLeftRear]), delegate(int sel)
			{
				settings[Setting.FireLeftRear] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			ddFireRightRear = (UIDropDown)uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.FireRightRear]), delegate(int sel)
			{
				settings[Setting.FireRightRear] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			RearFireVisibility(Convert.ToBoolean(settings[Setting.ManualRearFire]));
			uIButton2 = strip.AddTab("Ambulance");
			uIButton2.textColor = uIButton.textColor;
			uIButton2.pressedTextColor = uIButton.pressedTextColor;
			uIButton2.hoveredTextColor = uIButton.hoveredTextColor;
			uIButton2.focusedTextColor = uIButton.hoveredTextColor;
			uIButton2.disabledTextColor = uIButton.hoveredTextColor;
			uIPanel = (strip.tabContainer.components[2] as UIPanel);
			uIPanel.autoLayout = true;
			uIPanel.wrapLayout = true;
			uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			uIHelper = new UIHelper(uIPanel);
			uIHelper.AddSpace(15);
			uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.AmbulanceLeft]), delegate(int sel)
			{
				settings[Setting.AmbulanceLeft] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.AmbulanceRight]), delegate(int sel)
			{
				settings[Setting.AmbulanceRight] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddSpace(15);
			chkManualRearAmbulance = (uIHelper.AddCheckbox("Configure Rear Lights Separately", Convert.ToBoolean(settings[Setting.ManualRearAmbulance]), delegate(bool chkd)
			{
				settings[Setting.ManualRearAmbulance] = chkd.ToString();
				ExportSettings();
				RearAmbulanceVisibility(chkd);
				if (loaded)
				{
					Apply();
				}
			}) as UICheckBox);
			chkManualRearAmbulance.width = 744f;
			uIHelper.AddSpace(15);
			ddAmbulanceLeftRear = (UIDropDown)uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.AmbulanceLeftRear]), delegate(int sel)
			{
				settings[Setting.AmbulanceLeftRear] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			ddAmbulanceRightRear = (UIDropDown)uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.AmbulanceRightRear]), delegate(int sel)
			{
				settings[Setting.AmbulanceRightRear] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			RearAmbulanceVisibility(Convert.ToBoolean(settings[Setting.ManualRearAmbulance]));
            
            uIButton2 = strip.AddTab("Rotary (e.g. Snow Plow)", uIButton, fillText: true);
			uIButton2.textColor = uIButton.textColor;
			uIButton2.pressedTextColor = uIButton.pressedTextColor;
			uIButton2.hoveredTextColor = uIButton.hoveredTextColor;
			uIButton2.focusedTextColor = uIButton.hoveredTextColor;
			uIButton2.disabledTextColor = uIButton.hoveredTextColor;
			uIPanel = strip.tabContainer.components[3] as UIPanel;
			uIPanel.autoLayout = true;
			uIPanel.wrapLayout = true;
			uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			uIHelper = new UIHelper(uIPanel);
			uIHelper.AddSpace(15);
			uIHelper.AddDropdown("Left", ColorNames, Array.IndexOf(ColorNames, settings[Setting.SnowPlowLeft]), delegate(int sel)
			{
				settings[Setting.SnowPlowLeft] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});
			uIHelper.AddDropdown("Right", ColorNames, Array.IndexOf(ColorNames, settings[Setting.SnowPlowRight]), delegate(int sel)
			{
				settings[Setting.SnowPlowRight] = ColorNames[sel];
				ExportSettings();
				if (loaded)
				{
					Apply();
				}
			});

			strip.selectedIndex = -1;
			strip.selectedIndex = 0;
			int selectedIndex = uIDropDown.selectedIndex;
			uIDropDown.selectedIndex = -1;
			uIDropDown.selectedIndex = selectedIndex;

		}

		private void Apply()
		{
			switch (SelectedPreset)
			{
			case 0:
				UpdateColorsBoth(Service.Police, Colors["Red"], Colors["Blue"]);
				UpdateColorsBoth(Service.Fire, Colors["Light Blue"], Colors["Light Blue"]);
				UpdateColorsBoth(Service.Ambulance, Colors["Red"], Colors["Blue"]);
                UpdatePlowColors(Colors["Orange"], Colors["Orange"]);
				break;
			case 1:
				UpdateColors(Service.Police, Colors[settings[Setting.PoliceLeft]], Colors[settings[Setting.PoliceRight]]);
				if (Convert.ToBoolean(settings[Setting.ManualRearFire]))
				{
					UpdateColors(Service.Fire, Colors[settings[Setting.FireLeft]], Colors[settings[Setting.FireRight]]);
					UpdateColors(Service.Fire, Colors[settings[Setting.FireLeftRear]], Colors[settings[Setting.FireRightRear]], rear: true);
				}
				else
				{
					UpdateColorsBoth(Service.Fire, Colors[settings[Setting.FireLeft]], Colors[settings[Setting.FireRight]]);
				}
				if (Convert.ToBoolean(settings[Setting.ManualRearAmbulance]))
				{
					UpdateColors(Service.Ambulance, Colors[settings[Setting.AmbulanceLeft]], Colors[settings[Setting.AmbulanceRight]]);
					UpdateColors(Service.Ambulance, Colors[settings[Setting.AmbulanceLeftRear]], Colors[settings[Setting.AmbulanceRightRear]], rear: true);
				}
				else
				{
					UpdateColorsBoth(Service.Ambulance, Colors[settings[Setting.AmbulanceLeft]], Colors[settings[Setting.AmbulanceRight]]);
				}
                UpdatePlowColors(Colors[settings[Setting.SnowPlowLeft]], Colors[settings[Setting.SnowPlowRight]]);
				break;
			case 2:
				UpdateColorsBoth(Service.Police, Colors["Off"], Colors["Off"]);
				UpdateColorsBoth(Service.Fire, Colors["Off"], Colors["Off"]);
				UpdateColorsBoth(Service.Ambulance, Colors["Off"], Colors["Off"]);
                UpdatePlowColors(Colors["Off"], Colors["Off"]);
				break;
			case 3:
				UpdateColorsBoth(Service.Police, Colors["Red"], Colors["Blue"]);
				UpdateColorsBoth(Service.Fire, Colors["Red"], Colors["Blue"]);
				UpdateColorsBoth(Service.Ambulance, Colors["Red"], Colors["Blue"]);
                UpdatePlowColors(Colors["Red"], Colors["Orange"]);
				break;
			case 4:
				UpdateColorsBoth(Service.Police, Colors["Blue"], Colors["Blue"]);
				UpdateColorsBoth(Service.Fire, Colors["Light Blue"], Colors["Light Blue"]);
				UpdateColorsBoth(Service.Ambulance, Colors["Blue"], Colors["Blue"]);
                UpdatePlowColors(Colors["Blue"], Colors["Orange"]);
				break;
			case 5:
                UpdateColorsBoth(Service.Police, Colors["Red"], Colors["Red"]);
				UpdateColorsBoth(Service.Fire, Colors["Red"], Colors["Red"]);
				UpdateColorsBoth(Service.Ambulance, Colors["Red"], Colors["Red"]);
                UpdatePlowColors(Colors["Red"], Colors["Orange"]);
				break;
            case 6:
				UpdateColorsBoth(Service.Police, Colors["Red"], Colors["Blue"]);
                UpdateColors(Service.Fire, Colors["Red"], Colors["Red"]);
    			UpdateColors(Service.Fire, Colors["Red"], Colors["Red"], rear: true);
				UpdateColors(Service.Ambulance, Colors["Red"], Colors["Red"]);
				UpdateColors(Service.Ambulance, Colors["White"], Colors["White"], rear: true);
                UpdatePlowColors(Colors["Red"], Colors["Orange"]);
				break;
			}
		}

        private void UpdatePlowColors(Color left, Color right)
        {
            ChangeLightColor("Snowplow Light 1",  left);
            ChangeLightColor("Snowplow Light 2", right);
        }


		private void UpdateColorsBoth(Service sv, Color left, Color right)
		{
			UpdateColors(sv, left, right);
			UpdateColors(sv, left, right, rear: true);
		}

		private void UpdateColors(Service sv, Color left, Color right, bool rear = false)
		{
			UpdateColor(sv, Side.Left, left, rear);
			UpdateColor(sv, Side.Right, right, rear);
		}

		private void UpdateColor(Service sv, Side s, Color c, bool rear = false)
		{
			if (!rear || sv != 0)
			{
				object obj;
				switch (sv)
				{
				case Service.Police:
					obj = "Police Car";
					break;
				case Service.Fire:
					obj = "Fire Truck";
					break;
				default:
					obj = "Ambulance";
					break;
				}
				string str = (string)obj;
				string str2 = (s != 0) ? "Right" : "Left";
				string str3 = (!rear) ? string.Empty : "2";
				ChangeLightColor(str + " Light " + str2 + str3, c);
			}
		}

		private void ChangeLightColor(string effectName, Color color)
		{
			EffectCollection effectCollection = UnityEngine.Object.FindObjectOfType<EffectCollection>();
			if (effectCollection == null)
			{
				return;
			}
			EffectInfo effectInfo = effectCollection.m_effects.FirstOrDefault((EffectInfo e) => e.name == effectName);
			if (!(effectInfo == null))
			{
				Light component = effectInfo.GetComponent<Light>();
				if (!(component == null))
				{
					component.color = color;
				}
			}
		}
	}
}
