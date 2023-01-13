using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceMultiplierMod
{
    [BepInPlugin("ResourceMultiplier", "ResourceMultiplier", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ResourceMultiplier : BaseUnityPlugin
    {
        public const string MODNAME = "ResourceMultiplier";

        public const string AUTHOR = "sodenke";

        public const string GUID = "ResourceMultiplier";

        public const string VERSION = "1.0.0";

        internal readonly ManualLogSource log;

        internal readonly Harmony harmony;

        internal readonly Assembly assembly;

        public readonly string modFolder;

        public static ConfigEntry<float> resourceMultiplier;

        public static ConfigEntry<float> pickableMultiplier;

        public static ConfigEntry<bool> whiteListEnabled;

        public static List<string> whitelist;

        public ResourceMultiplier()
        {
            log = base.Logger;
            harmony = new Harmony("ResourceMultiplier");
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
            Harmony.CreateAndPatchAll(typeof(ResourceMultiplierMod.Patches));
        }
        public void Awake()
        {
            resourceMultiplier = base.Config.Bind("General", "Multiplier for resources", 1.0f, " Resource Multiplier");
            pickableMultiplier = base.Config.Bind("General", "Multiplier for pickables", 1.0f, " Pickable Multiplier");

            whiteListEnabled = base.Config.Bind("General", "Whitelist enabled", false, " Whitelist enabled");
            try
            {
                whitelist = File.ReadAllLines(Path.GetDirectoryName(assembly.Location) + "\\resourceMultiplierWhitelist.txt").Distinct().ToList();
                for (int i = 0; i < whitelist.Count; i++)
                {
                    whitelist[i] = whitelist[i].ToLower().Trim();
                }
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    try
                    {
                        File.Create(Path.GetDirectoryName(assembly.Location) + "\\resourceMultiplierWhitelist.txt");
                    }
                    catch
                    {
                        //do nothing 
                    }
                }

                whitelist = new List<string>();
            }

        }
    }
}
