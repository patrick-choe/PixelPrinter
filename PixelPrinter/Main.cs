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

using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace PixelPrinter
{
	internal static class Main
	{
		private static Harmony _harmony;
		private static UnityModManager.ModEntry _mod;
		internal static bool IsEnabled { get; private set; }

		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			_mod = modEntry;
			_mod.OnToggle = OnToggle;

			return true;
		}

		private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
		{
			_mod = modEntry;
			IsEnabled = value;

			if (IsEnabled)
			{
				StartTweaks();
			}
			else
			{
				StopTweaks();
			}

			return true;
		}

		private static void StartTweaks()
		{
			_harmony = new Harmony(_mod.Info.Id);
			_harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		private static void StopTweaks()
		{
			_harmony.UnpatchAll(_harmony.Id);
		}
	}
}
