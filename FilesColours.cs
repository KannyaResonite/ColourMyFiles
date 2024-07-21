using System.IO;
using System.Reflection;
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
        private static ModConfigurationKey<float4> TextureColour = new ModConfigurationKey<float4>("TextureColour", "The colour for texture files in the files window.", () => new float4(0.79f, 0.76f, 0.89f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> AudioColour = new ModConfigurationKey<float4>("AudioColour", "The colour for audio files in the files window.", () => new float4(0.26f, 1f, 0.48f, 1f));
        
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float4> MetaColour = new ModConfigurationKey<float4>("MetaColour", "The colour for meta files in the files window.", () => new float4(0.26f, 0.08f, 0f, 1f));
        
        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config?.Save(true);
            
            var harmony = new Harmony("com.Kannya.FilesColours");
            
            harmony.Patch(typeof(BrowserItem).GetMethod("OnChanges", AccessTools.all), new HarmonyMethod(typeof(FilesColours).GetMethod(nameof(SetColours), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static bool SetColours(ref BrowserItem __instance)
        {
            switch (Path.GetExtension(__instance.Button.Label.Content).ToLower())
            {
                // folders are yellow, so i chose a different colour
                case ".fbx":
                case ".obj":
                case ".dae":
                case ".gtlf":
                    __instance.SetColour(MeshColour);
                    break;
                
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".dds":
                case ".jfif":
                case ".webp":
                    __instance.SetColour(TextureColour);
                    break;
                
                case ".wav":
                case ".ogg":
                case ".mp3":
                case ".wma":
                    __instance.SetColour(AudioColour);
                    break;
                
                case ".meta":
                    __instance.SetColour(MetaColour);
                    break;
            }
            
            return true;
        }
    }

    public static class Extensions
    {
        public static void SetColour(this BrowserItem Item, ModConfigurationKey<float4> ConfigColour) // Lazy and cleaner than repeated code or flow looking ugly from using a non-extension
        {
            InteractionElement interactionElement = Item.Button;
            
            var ColourX = Item.NormalColor;
            var TargetColour = FilesColours.Config.GetValue(ConfigColour);

            if (ColourX.Value.r == TargetColour[0] && ColourX.Value.g == TargetColour[1] && ColourX.Value.b == TargetColour[2] && ColourX.Value.a == TargetColour[3]) // Match only 1:1 same value, disregard loss of precision
            {
                return;
            }
            
            ColourX.Value = new colorX(TargetColour[0], TargetColour[1], TargetColour[2], TargetColour[3]);
            interactionElement.SetColors(ColourX);
        }
    }
}