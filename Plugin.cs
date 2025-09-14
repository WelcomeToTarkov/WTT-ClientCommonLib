using BepInEx;
using System;
using System.IO;
using WTTClientCommonLib.CustomVoices;
using WTTClientCommonLib.UI;

namespace WTTClientCommonLib
{
    [BepInPlugin("com.aaaWTT.ClientCommonLib", "WTT-ClientCommonLib", "1.0.0")]
    public class WTTClientCommonLib : BaseUnityPlugin
    {
        private static readonly string PluginsDirectory = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins");

        private void Start()
        {
            string modDirectory = "WTT-ClientCommonLib";
            
            string layoutsDir = Path.Combine(PluginsDirectory, modDirectory, "RigLayouts");
            string slotImagesDir = Path.Combine(PluginsDirectory, modDirectory, "SlotImages");
            string voicesDir = Path.Combine(PluginsDirectory, modDirectory, "Voices");

            RigLayoutManager.RegisterDirectory(layoutsDir);
            SlotImageManager.RegisterDirectory(slotImagesDir);
            VoiceManager.RegisterDirectory(voicesDir);
        }
    }
}
