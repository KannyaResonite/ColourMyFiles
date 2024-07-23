using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace ColourMyFiles
{
    public class FilesColours : ResoniteMod
    {
        public override string Name => "Colour My Files";

        public override string Author => "Kannya";

        public override string Version => "1.0.0";
        
        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> MeshColour = new ModConfigurationKey<colorX>("MeshColour", "The colour for mesh files in the files window.", () => new colorX(0f, 0.6f, 0.6f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> TextureColour = new ModConfigurationKey<colorX>("TextureColour", "The colour for texture files in the files window.", () => RadiantUI_Constants.Sub.PURPLE);
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> AudioColour = new ModConfigurationKey<colorX>("AudioColour", "The colour for audio files in the files window.", () => RadiantUI_Constants.Sub.GREEN);
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> MetaColour = new ModConfigurationKey<colorX>("MetaColour", "The colour for meta files in the files window.", () => RadiantUI_Constants.Sub.RED);
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> ConfigColour = new ModConfigurationKey<colorX>("ConfigColour", "The colour for config files in the files window.", () => RadiantUI_Constants.Sub.ORANGE);
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> TextColour = new ModConfigurationKey<colorX>("TextColour", "The colour for text files in the files window.", () => new colorX(0.3f, 0.3f, 0.3f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> VideoColour = new ModConfigurationKey<colorX>("VideoColour", "The colour for video files in the files window.", () => new colorX(0f, 0f, 1f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<colorX> DocumentColour = new ModConfigurationKey<colorX>("DocumentColour", "The colour for document files in the files window.", () => new colorX(0.8f, 0f, 0.8f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> HideMetaFiles = new ModConfigurationKey<bool>("HideMetaFiles", "Whether to hide meta files from the files window entirely.", () => false);
        
        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config?.Save(true);
            
            var harmony = new Harmony("com.Kannya.FilesColours");
            
            harmony.Patch(typeof(BrowserItem).GetMethod("OnChanges", AccessTools.all), new HarmonyMethod(typeof(FilesColours).GetMethod(nameof(OnChanges), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static List<string> Audio = new()
        {
            ".mp3",
            ".mpeg3",
            ".aac",
            ".ac3",
            ".aif",
            ".aiff",
            ".ape",
            ".au",
            ".it",
            ".mka",
            ".mod",
            ".mp1",
            ".mp2",
            ".opus",
            ".s3m",
            ".sid",
            ".w64",
            ".wma",
            ".xm",
            ".nsf",
            ".nsfe",
            ".gbs",
            ".vgm",
            ".gym"
        };

        private static bool OnChanges(ref BrowserItem __instance)
        {
            if (__instance is FileSystemItem fi)
            {
                var AssetType = AssetHelper.IdentifyClass($"{fi.BasePath}\\{fi.Name}");
                
                if (Path.GetExtension(fi.Name).ToLower() == ".meta")
                {
                    if (Config.GetValue(HideMetaFiles))
                    {
                        __instance.Slot.Destroy();
                    }
                    
                    __instance.SetColour(Config.GetValue(MetaColour));
                    
                    return true;
                }
                
                if (Audio.Any(o => o == Path.GetExtension(fi.Name).ToLower()))
                {
                    __instance.SetColour(Config.GetValue(AudioColour));
                    
                    return true;
                }

                var TargetColour = GetClassColour(AssetType);

                if (TargetColour == null)
                {
                    return true;
                }
                
                __instance.SetColour(TargetColour.Value);
            }
            
            return true;
        }
        
        public static colorX? GetClassColour(AssetClass assetClass)
        {
            return assetClass switch
            {
                AssetClass.Audio => Config.GetValue(AudioColour),
                AssetClass.Document => Config.GetValue(DocumentColour),
                AssetClass.Text => Config.GetValue(TextColour),
                AssetClass.Model => Config.GetValue(MeshColour),
                AssetClass.Object => Config.GetValue(ConfigColour),
                AssetClass.Texture => Config.GetValue(TextureColour),
                AssetClass.Video => Config.GetValue(VideoColour),
                _ => null,
            };
        }
    }

    public static class Extensions
    {
        public static void SetColour(this BrowserItem Item, colorX Colour) // Lazy and cleaner than repeated code or flow looking ugly from using a non-extension
        {
            InteractionElement interactionElement = Item.Button;
            
            var ColourX = Item.NormalColor;

            if (ColourX.Value.r == Colour.r && ColourX.Value.g == Colour.g && ColourX.Value.b == Colour.b && ColourX.Value.a == Colour.a) // Match only 1:1 same value, disregard loss of precision
            {
                return;
            }
            
            ColourX.Value = Colour;
            interactionElement.SetColors(ColourX);
        }
    }
}