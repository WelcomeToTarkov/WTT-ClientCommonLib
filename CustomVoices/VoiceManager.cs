using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WTTClientCommonLib.CustomVoices
{
    public static class VoiceManager
    {
        private static readonly List<string> RegisteredDirectories = new();
        private static readonly Dictionary<string, string> VoiceEntries = new();
        private static readonly object LockObject = new();

        /// <summary>
        /// Registers a new directory and immediately loads its JSON voice entries.
        /// Can be called by other mods.
        /// </summary>
        public static void RegisterDirectory(string path)
        {
            lock (LockObject)
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Invalid or missing voice path: {path}");
                    return;
                }

                if (RegisteredDirectories.Contains(path))
                    return;

                RegisteredDirectories.Add(path);
                LoadFromDirectory(path);
            }
        }

        /// <summary>
        /// Loads all .json voice mappings from a directory.
        /// </summary>
        private static void LoadFromDirectory(string directory)
        {
            foreach (var jsonFile in Directory.GetFiles(directory, "*.json"))
            {
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(jsonFile));
                    foreach (var kvp in dict)
                    {
                        if (!VoiceEntries.ContainsKey(kvp.Key))
                        {
                            VoiceEntries[kvp.Key] = kvp.Value;
                            AddToResources(kvp.Key, kvp.Value);
                        }
#if DEBUG
                        else
                        {
                            Console.WriteLine($"[WTT-ClientCommonLib] Skipped duplicate voice key: {kvp.Key}");
                        }
#endif
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WTT-ClientCommonLib] Error processing {jsonFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Adds a single key/value to EFT's resource key manager dictionary.
        /// </summary>
        private static void AddToResources(string key, string value)
        {
            if (!ResourceKeyManagerAbstractClass.dictionary_0.ContainsKey(key))
            {
                ResourceKeyManagerAbstractClass.dictionary_0[key] = value;
#if DEBUG
                Console.WriteLine($"[WTT-ClientCommonLib] Added voice key: {key}");
#endif
            }
        }

    }
}
