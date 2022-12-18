using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace MonsterLootMultiplierMod
{
    [BepInPlugin("MonsterLootMultiplier", "MonsterLootMultiplier", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class MonsterLootMultiplier : BaseUnityPlugin
    {
        public const string MODNAME = "MonsterLootMultiplier";

        public const string AUTHOR = "sodenke";

        public const string GUID = "MonsterLootMultiplier";

        public const string VERSION = "1.0.0";

        internal readonly ManualLogSource log;

        internal readonly Harmony harmony;

        internal readonly Assembly assembly;

        public readonly string modFolder;

        public static ConfigEntry<int> lootMultiplier;

        public MonsterLootMultiplier()
        {
            log = base.Logger;
            harmony = new Harmony("MonsterLootMultiplier");
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
            Harmony.CreateAndPatchAll(typeof(MonsterLootMultiplierMod.Patches));
        }

        public void Awake()
        {
            lootMultiplier = base.Config.Bind("General", "Multiplier for monster drops", 1, " Monster Drop Multiplier");
        }
    }
}