using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using WTTClientCommonLib.Helpers;

namespace WTTClientCommonLib.UI
{
    public static class SlotImageManager
    {
        private static readonly List<string> RegisteredDirectories = new();
        private static readonly Dictionary<string, Sprite> SlotEntries = new();
        private static readonly object LockObject = new();

        /// <summary>
        /// Registers a directory containing slot images and loads them immediately.
        /// Uses the name of the image file as the slot name
        /// Can be called by other mods.
        /// </summary>
        public static void RegisterDirectory(string path)
        {
            lock (LockObject)
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Invalid or missing slot image path: {path}");
                    return;
                }

                if (RegisteredDirectories.Contains(path))
                    return;

                RegisteredDirectories.Add(path);
                LoadFromDirectory(path);
            }
        }

        /// <summary>
        /// Loads all supported image files from a directory.
        /// </summary>
        private static void LoadFromDirectory(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".bmp")
                {
                    RegisterSlotImage(file);
                }
            }
        }

        /// <summary>
        /// Registers a single slot image from disk.
        /// Can be called by other mods.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterSlotImage(string imagePath, string slotID = null)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                Console.WriteLine($"[WTT-ClientCommonLib] Invalid or missing image file: {imagePath}");
                return;
            }

            try
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                CreateAndRegister(imageData, slotID ?? Path.GetFileNameWithoutExtension(imagePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WTT-ClientCommonLib] Error loading image {imagePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers a slot image embedded in an assembly resource.
        /// Can be called by other mods.
        /// </summary>
        public static void RegisterSlotImageFromResource(Assembly assembly, string resourcePath, string slotName = null)
        {
            if (assembly == null || string.IsNullOrWhiteSpace(resourcePath))
            {
                Console.WriteLine("[WTT-ClientCommonLib] Invalid parameters for resource loading");
                return;
            }

            try
            {
                using var stream = assembly.GetManifestResourceStream(resourcePath);
                if (stream == null)
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Resource {resourcePath} not found in {assembly.FullName}");
                    return;
                }

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                byte[] imageData = ms.ToArray();

                CreateAndRegister(imageData, slotName ?? Path.GetFileNameWithoutExtension(resourcePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WTT-ClientCommonLib] Error loading resource {resourcePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a sprite from raw image data and registers it in EFT's resources.
        /// </summary>
        private static void CreateAndRegister(byte[] data, string slotName)
        {
            try
            {
                if (SlotEntries.ContainsKey(slotName))
                {
#if DEBUG
                    Console.WriteLine($"[WTT-ClientCommonLib] Skipped duplicate slot key: {slotName}");
#endif
                    return;
                }

                Texture2D texture = new Texture2D(2, 2);
                if (!texture.LoadImage(data))
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Failed to create texture for {slotName}");
                    return;
                }

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                SlotEntries[slotName] = sprite;
                ResourceHelper.AddEntry($"Slots/{slotName}", sprite);
#if DEBUG
                Console.WriteLine($"[WTT-ClientCommonLib] Added slot sprite: {slotName}");
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WTT-ClientCommonLib] Error creating sprite: {ex.Message}");
            }
        }

    }
}
