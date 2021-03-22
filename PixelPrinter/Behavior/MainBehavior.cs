/*
 * Copyright (C) 2021 PatrickKR
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using ADOFAI;
using DG.Tweening;
using SFB;
using UnityEngine;

namespace PixelPrinter.Behavior
{
	internal class MainBehavior : MonoBehaviour
	{
		private Bitmap _image;
		private bool _enabled;
		private int _width;
		private string _widthString;
		private int _height;
		private string _heightString;

		private void OnGUI()
		{
			GUI.Box(new Rect(20, 20, 240, 200), "Pixel Printer v1.0");

			GUI.Label(new Rect(40, 180, 140, 20), "Made by PatrickKR, Idea by 보성녹차");

			if (GUI.Button(new Rect(40, 60, 200, 20), "Open Image"))
			{
				var files = StandaloneFileBrowser.OpenFilePanel("Open Image", Persistence.GetLastUsedFolder(), new[] {
					new ExtensionFilter(RDString.Get("editor.dialog.imageFileFormat"), GCS.SupportedImageFiles)
				}, false);

				if (files.Length == 0 || string.IsNullOrEmpty(files[0]))
				{
					_image.Dispose();
					_image = null;
					enabled = false;
				}
				else
				{
					_image = new Bitmap(File.OpenRead(files[0]));
					_widthString = _image.Width.ToString();
					_heightString = _image.Height.ToString();
					_enabled = true;
				}
			}

			if (_image == null)
			{
				return;
			}

			GUI.Label(new Rect(40, 90, 140, 20), "Width (1 ~ " + _image.Width + ")");

			_width = 0;
			_widthString = GUI.TextField(new Rect(190, 90, 40, 20), _widthString);
			int.TryParse(_widthString, out _width);

			GUI.Label(new Rect(40, 120, 140, 20), "Height (1 ~ " + _image.Height + ")");

			_height = 0;
			_heightString = GUI.TextField(new Rect(190, 120, 40, 20), _heightString);
			int.TryParse(_heightString, out _height);

			if (_width < 1 || _width > _image.Width || _height < 1 || _height > _image.Height)
			{
				return;
			}

			if (GUI.Button(new Rect(40, 150, 200, 20), "Run!") && _enabled)
			{
				GC.Collect();
				Resources.UnloadUnusedAssets();

				var instance = scnEditor.instance;
				var levelData = instance.levelData;

				instance.InvokeMethod("ClearAllFloorOffsets");
				instance.InvokeMethod("DeselectAnyUIGameObject");

				levelData.pathData = new StringBuilder(_width * _height - 1).Insert(0, "R", _width * _height - 1).ToString();

				var settingsInfo = GCS.settingsInfo;
				levelData.songSettings = new LevelEvent(0, LevelEventType.SongSettings, settingsInfo["SongSettings"]);
				levelData.levelSettings = new LevelEvent(0, LevelEventType.LevelSettings, settingsInfo["LevelSettings"]);
				levelData.trackSettings = new LevelEvent(0, LevelEventType.TrackSettings, settingsInfo["TrackSettings"]);
				levelData.backgroundSettings = new LevelEvent(0, LevelEventType.BackgroundSettings, settingsInfo["BackgroundSettings"]);
				levelData.cameraSettings = new LevelEvent(0, LevelEventType.CameraSettings, settingsInfo["CameraSettings"]);
				levelData.miscSettings = new LevelEvent(0, LevelEventType.MiscSettings, settingsInfo["MiscSettings"]);

				levelData.cameraSettings.data["relativeTo"] = CamMovementType.Tile;
				levelData.cameraSettings.data["position"] = new Vector2(_width / 2f - 0.5f, -_height / 2f + 0.5f);
				levelData.cameraSettings.data["zoom"] = Math.Max(_width * 9, _height * 16);

				levelData.miscSettings.data["stickToFloors"] = Toggle.Enabled;

				instance.events.Clear();

				instance.events.Add(new LevelEvent(0, LevelEventType.MoveTrack) {
					data = new Dictionary<string, object> {
						{
							"startTile", new Tuple<int, TileRelativeTo>(0, TileRelativeTo.Start)
						},
						{
							"endTile", new Tuple<int, TileRelativeTo>(0, TileRelativeTo.End)
						},
						{
							"duration", 0.0f
						},
						{
							"positionOffset", Vector2.zero
						},
						{
							"rotationOffset", 0.0f
						},
						{
							"scale", 200
						},
						{
							"opacity", 100
						},
						{
							"angleOffset", 0.0f
						},
						{
							"ease", Ease.Linear
						},
						{
							"eventTag", ""
						}
					}
				});

				for (var floor = 1; floor < _height; floor++)
				{
					instance.events.Add(new LevelEvent(floor * _width, LevelEventType.PositionTrack) {
						data = new Dictionary<string, object> {
							{
								"positionOffset", new Vector2(-_width, -1)
							},
							{
								"editorOnly", Toggle.Disabled
							}
						}
					});
				}

				using (var newImage = new Bitmap(_width, _height))
				{
					using (var graphics = System.Drawing.Graphics.FromImage(newImage))
					{
						graphics.SmoothingMode = SmoothingMode.AntiAlias;
						graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
						graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
						// graphics.SmoothingMode = SmoothingMode.None;
						// graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
						// graphics.PixelOffsetMode = PixelOffsetMode.None;
						graphics.DrawImage(_image, new Rectangle(0, 0, _width, _height));

						var lastHex = "debb7bff";

						for (var y = 0; y < _height; y++)
						{
							for (var x = 0; x < _width; x++)
							{
								var hex = newImage.GetPixel(x, y).ToHex();

								if (hex == lastHex)
								{
									continue;
								}

								lastHex = hex;

								instance.events.Add(new LevelEvent(x + y * _width, LevelEventType.ColorTrack) {
									data = new Dictionary<string, object> {
										{
											"trackColorType", TrackColorType.Single
										},
										{
											"trackColor", lastHex
										},
										{
											"secondaryTrackColor", "ffffff"
										},
										{
											"trackColorAnimDuration", 2.0f
										},
										{
											"trackColorPulse", TrackColorPulse.None
										},
										{
											"trackPulseLength", 10
										},
										{
											"trackStyle", TrackStyle.Standard
										}
									}
								});
							}
						}

						instance.RemakePath();
						instance.InvokeMethod("SelectFirstFloor");
						instance.UpdateSongAndLevelSettings();
						instance.customLevel.ReloadAssets();
						instance.customLevel.UpdateDecorationSprites();
						instance.ShowFileActionsPanel(false);
						instance.ShowShortcutsPanel(false);
					}
				}
			}
		}
	}
}