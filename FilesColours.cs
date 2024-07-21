using System.IO;
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
        private static ModConfigurationKey<float4> MeshColour = new ModConfigurationKey<float4>("MeshColour", "The colour for mesh files in the files window.", () => new float4(0f, 0.6f, 0.6f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> TextureColour = new ModConfigurationKey<float4>("TextureColour", "The colour for texture files in the files window.", () => new float4(RadiantUI_Constants.Sub.PURPLE.r, RadiantUI_Constants.Sub.PURPLE.g, RadiantUI_Constants.Sub.PURPLE.b, RadiantUI_Constants.Sub.PURPLE.a));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> AudioColour = new ModConfigurationKey<float4>("AudioColour", "The colour for audio files in the files window.", () => new float4(RadiantUI_Constants.Sub.GREEN.r, RadiantUI_Constants.Sub.GREEN.g, RadiantUI_Constants.Sub.GREEN.b, RadiantUI_Constants.Sub.GREEN.a));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> MetaColour = new ModConfigurationKey<float4>("MetaColour", "The colour for meta files in the files window.", () => new float4(RadiantUI_Constants.Sub.RED.r, RadiantUI_Constants.Sub.RED.g, RadiantUI_Constants.Sub.RED.b, RadiantUI_Constants.Sub.RED.a));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> ConfigColour = new ModConfigurationKey<float4>("ConfigColour", "The colour for config files in the files window.", () => new float4(RadiantUI_Constants.Sub.ORANGE.r, RadiantUI_Constants.Sub.ORANGE.g, RadiantUI_Constants.Sub.ORANGE.b, RadiantUI_Constants.Sub.ORANGE.a));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> TextColour = new ModConfigurationKey<float4>("TextColour", "The colour for text files in the files window.", () => new float4(0.3f, 0.3f, 0.3f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> VideoColour = new ModConfigurationKey<float4>("VideoColour", "The colour for video files in the files window.", () => new float4(0f, 0f, 1f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> DocumentColour = new ModConfigurationKey<float4>("DocumentColour", "The colour for document files in the files window.", () => new float4(0.8f, 0f, 0.8f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> HideMetaFiles = new ModConfigurationKey<bool>("HideMetaFiles", "Whether to hide meta files from the files window entirely.", () => false);
        
        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config?.Save(true);
            
            var harmony = new Harmony("com.Kannya.FilesColours");
            
            harmony.Patch(typeof(BrowserItem).GetMethod("OnChanges", AccessTools.all), new HarmonyMethod(typeof(FilesColours).GetMethod(nameof(OnChanges), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static bool OnChanges(ref BrowserItem __instance)
        {
            if (__instance is FileSystemItem fi)
            {
                var AssetType = AssetHelper.IdentifyClass($"{fi.BasePath}\\{fi.Name}");
                
                if (Config.GetValue(HideMetaFiles) && Path.GetExtension(fi.Name).ToLower() == ".meta")
                {
                    __instance.Slot.Destroy();
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
                AssetClass.Audio => new colorX(Config.GetValue(AudioColour)),
                AssetClass.Document => new colorX(Config.GetValue(DocumentColour)),
                AssetClass.Text => new colorX(Config.GetValue(TextColour)),
                AssetClass.Model => new colorX(Config.GetValue(MeshColour)),
                AssetClass.Object => new colorX(Config.GetValue(ConfigColour)),
                AssetClass.Texture => new colorX(Config.GetValue(TextureColour)),
                AssetClass.Video => new colorX(Config.GetValue(VideoColour)),
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