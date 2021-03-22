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

using HarmonyLib;
using PixelPrinter.Behavior;
using UnityEngine;

namespace PixelPrinter.Patch
{
	[HarmonyPatch(typeof(scnEditor), "Update")]
	internal static class MainPatch
	{
		private static GameObject _gameObject;
		private static MainBehavior _mainBehavior;

		private static void Prefix(scnEditor __instance)
		{
			if (!Main.IsEnabled ||
				!scrController.instance.paused ||
				GCS.standaloneLevelMode)
			{
				if (_gameObject == null)
				{
					return;
				}
				Object.DestroyImmediate(_gameObject);
				Object.DestroyImmediate(_mainBehavior);
				_gameObject = null;
				_mainBehavior = null;

				return;
			}

			if (_gameObject != null ||
				!Input.GetKeyDown(KeyCode.F9) ||
				Input.GetKey(KeyCode.LeftAlt) ||
				Input.GetKey(KeyCode.RightAlt) ||
				Input.GetKey(KeyCode.LeftControl) ||
				Input.GetKey(KeyCode.RightControl) ||
				Input.GetKey(KeyCode.LeftCommand) ||
				Input.GetKey(KeyCode.RightCommand))
			{
				return;
			}

			_gameObject = new GameObject();
			_mainBehavior = _gameObject.AddComponent<MainBehavior>();
		}
	}
}
