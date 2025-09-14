using System;
using System.Collections.Generic;
using System.IO;
using EFT.UI.DragAndDrop;
using UnityEngine;
using WTTClientCommonLib.Helpers;

namespace WTTClientCommonLib.UI
{
    public static class RigLayoutManager
    {
        private static readonly List<string> RegisteredDirectories = new();
        private static readonly Dictionary<string, ContainedGridsView> RigEntries = new();
        private static readonly object LockObject = new();

        /// <summary>
        /// Registers a directory containing rig layout bundles and loads them immediately.
        /// Can be called by other mods.
        /// </summary>
        public static void RegisterDirectory(string path)
        {
            lock (LockObject)
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Invalid or missing rig layout path: {path}");
                    return;
                }

                if (RegisteredDirectories.Contains(path))
                    return;

                RegisteredDirectories.Add(path);
                LoadFromDirectory(path);
            }
        }

        /// <summary>
        /// Loads all .bundle files from a directory.
        /// </summary>
        private static void LoadFromDirectory(string directory)
        {
            foreach (var bundlePath in Directory.GetFiles(directory, "*.bundle"))
            {
                LoadBundle(bundlePath);
            }
        }

        /// <summary>
        /// Loads a single rig layout bundle.
        /// </summary>
        private static void LoadBundle(string bundlePath)
        {
            string bundleName = Path.GetFileNameWithoutExtension(bundlePath);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Console.WriteLine($"[WTT-ClientCommonLib] Failed to load rig layout bundle: {bundleName}");
                return;
            }

            foreach (var prefab in bundle.LoadAllAssets<GameObject>())
            {
                var gridView = prefab?.GetComponent<ContainedGridsView>();
                if (gridView == null)
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Prefab {prefab?.name ?? "null"} missing ContainedGridsView.");
                    continue;
                }

                if (!RigEntries.ContainsKey(prefab.name))
                {
                    RigEntries[prefab.name] = gridView;
                    ResourceHelper.AddEntry($"UI/Rig Layouts/{prefab.name}", gridView);
#if DEBUG
                    Console.WriteLine($"[WTT-ClientCommonLib] Added rig layout: {prefab.name}");
#endif
                }
                else
                {
#if DEBUG
                    Console.WriteLine($"[WTT-ClientCommonLib] Skipped duplicate rig layout: {prefab.name}");
#endif
                }
            }

            bundle.Unload(false);
        }

    }
}
