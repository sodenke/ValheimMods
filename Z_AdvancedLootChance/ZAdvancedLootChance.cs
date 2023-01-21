using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jotunn.Managers;
using UnityEngine;
using System.Linq;

namespace Z_AdvancedLootChance
{
    [BepInPlugin("ZAdvancedLootChance", "ZAdvancedLootChance", "1.0.0")]
    [BepInProcess("valheim.exe")]
    [BepInDependency("com.jotunn.jotunn", "2.7.0")]
    public class ZAdvancedLootChance : BaseUnityPlugin
    {
        public const string MODNAME = "ZAdvancedLootChance";

        public const string AUTHOR = "sodenke";

        public const string GUID = "ZAdvancedLootChance";

        public const string VERSION = "1.0.0";

        internal readonly ManualLogSource log;

        internal Harmony harmony;

        internal readonly Assembly assembly;

        public readonly string modFolder;

        public static List<string> lootChance;

        public ZAdvancedLootChance()
        {
            log = base.Logger;
            harmony = new Harmony("ZAdvancedLootChance");
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
        }

        public void Awake()
        {
            ItemManager.OnItemsRegistered += (Action)SetDropChances;
            Harmony.CreateAndPatchAll(typeof(ZAdvancedLootChance).Assembly);

            try
            {
                lootChance = File.ReadAllLines(Path.GetDirectoryName(assembly.Location) + "\\ZAdvancedLootChance.txt").Distinct().ToList();
                for (int i = 0; i < lootChance.Count; i++)
                {
                    lootChance[i] = lootChance[i].Trim();
                }
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    try
                    {
                        File.Create(Path.GetDirectoryName(assembly.Location) + "\\ZAdvancedLootChance.txt");
                    }
                    catch
                    {
                        //do nothing 
                    }
                }

                lootChance = new List<string>();
            }
        }

        public void SetDropChances()
        {
            foreach (var line in lootChance)
            {
                string[] splitLine = line.Split(',');
                int chanceAmount;
                if (splitLine.Length == 2
                    && !string.IsNullOrEmpty(splitLine[0])
                    && !string.IsNullOrEmpty(splitLine[1])
                    //&& !string.IsNullOrEmpty(splitLine[2])
                    && int.TryParse(splitLine[1], out chanceAmount))
                {
                    log.LogInfo("ZAdvancedLootChance successfully parsed file");
                    var resource = splitLine[0];
                    //var itemCode = splitLine[1];

                    GameObject prefab = null;
                    try
                    {
                        log.LogInfo("ZAdvancedLootChance getting prefab for: " + resource);
                        prefab = PrefabManager.Instance.GetPrefab(resource);
                    }
                    catch (Exception)
                    {
                        log.LogWarning("Resource name " + resource + " in ZAdvancedLootChance.txt is invalid, it will be skipped.");
                    }

                    if (prefab != null)
                    {
                        log.LogInfo("ZAdvancedLootChance found game resource: " + resource);

                        DropOnDestroyed component = prefab.GetComponent<DropOnDestroyed>();

                        log.LogInfo("ZAdvancedLootChance setting chances to drop to: " + chanceAmount);
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // tried to do it by item but for some reason when setting m_stackMin and m_stackMax, the changes aren't respected
                        // also tried adding more of the same item to the drops, but that also doesn't work
                        // so just adding more chances for all drops from the resource or monster
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        component.m_dropWhenDestroyed.m_dropMax = chanceAmount;
                        component.m_dropWhenDestroyed.m_dropMin = chanceAmount;
                        component.m_dropWhenDestroyed.m_oneOfEach = false;
                        
                        //var dropItems = component.m_dropWhenDestroyed.m_drops;
                        //var newDropItems = new List<DropTable.DropData>();
                        //for (int i = 0; i < dropItems.Count; i++)
                        //{
                        //    var item = dropItems[i];
                        //    newDropItems.Add(item);
                        //    if (item.m_item.name.ToLower() == "rk_egg")
                        //    {
                        //        log.LogInfo("ZAdvancedLootChance Found item: rk_egg setting chances to drop to: " + chanceAmount);
                        //        //log.LogInfo("ZAdvancedLootChance Found item: " + itemCode + " setting chances to drop to: " + chanceAmount);
                        //        component.m_dropWhenDestroyed.m_oneOfEach = false;
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //        newDropItems.Add(item);
                        //    }
                        //}
                        //component.m_dropWhenDestroyed.m_drops = newDropItems;
                    }
                    else
                    {
                        log.LogInfo("ZAdvancedLootChance prefab for " + resource + " not found.");
                    }
                }
                else
                {
                    log.LogError("ZAdvancedLootChance.txt in invalid. Correct format is <resource>,<intNumberOfChances>");
                }
            }
        
            ItemManager.OnItemsRegistered -= (Action)SetDropChances;
        }
    }
}
