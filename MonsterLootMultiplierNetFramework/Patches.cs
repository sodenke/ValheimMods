using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonsterLootMultiplierMod
{
    [RequireComponent(typeof(Character))]
    public class Patches
    {
        [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
        [HarmonyPrefix]
        private static bool LootMultiplierCustom(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
        {
            List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
            // m_character is no longer public so have to get the monster character seperatly and then check
            //int num = ((!__instance.m_character) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, __instance.m_character.GetLevel() - 1)));
            Character monster = __instance.GetComponent<Character>();
            int num = ((!monster) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, monster.GetLevel() - 1)));

            foreach (CharacterDrop.Drop drop in __instance.m_drops)
            {
                
                if (drop.m_prefab == null)
                {
                    //Console.Log("Drop m_prefab is null so continuing for:  " + drop.ToString());
                    continue;
                }
                //Console.Log("START Calculating drop for: " + drop.m_prefab.name);
                float num2 = drop.m_chance;
                //Console.Log("Drop chance: " + num2);
                if (drop.m_levelMultiplier)
                {
                    num2 *= (float)num;
                    Console.Log("Drop chance multiplied by level multiplyer " + num + " and is now: " + num2);
                }
                if ((Random.value <= num2))
                {
                    //Console.Log("Drop chance success");
                    int num3 = Random.Range(drop.m_amountMin, drop.m_amountMax);
                    //Console.Log("Drop num: " + num3);
                    if (drop.m_levelMultiplier)
                    {
                        num3 *= num;
                        //Console.Log("Drop num by level multiplier: " + num3);
                    }
                    if (drop.m_onePerPlayer)
                    {
                        num3 = ZNet.instance.GetNrOfPlayers();
                        //Console.Log("Drop num due to one per player: " + num3);
                    }
                    if (num3 > 0)
                    {   
                        //check if the drop is a boss trophy
                        bool isBossTrophy = false;
                        foreach(var trophy in MonsterLootMultiplier.bossTrophies)
                        {
                            if(drop.m_prefab.name.Contains(trophy))
                            {
                                isBossTrophy = true;
                            }
                        }
                        //Console.Log("Boss trpohy: " + isBossTrophy);

                        if (MonsterLootMultiplier.whiteListEnabled.Value)
                        {
                            //Console.Log("WhiteList is enabled:" + MonsterLootMultiplier.whiteListEnabled.Value);
                            bool itemInWhiteList = MonsterLootMultiplier.whitelist.Contains(drop.m_prefab.name.ToLower());
                            if (itemInWhiteList)
                            {
                                //Console.Log("Drop is in whitelist");
                                // do extra drops
                                DropItems(drop, list, isBossTrophy, num3);
                            }
                            else
                            {
                                //Console.Log("Drop is not in whitelist so droping original game num of: " + num3);
                                // original drop code
                                list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, num3));
                            }
                        }
                        else
                        {
                            // do extra drops
                            DropItems(drop, list, isBossTrophy, num3);
                        }
                    }
                }
                //Console.Log("END Calculating drop for: " + drop.m_prefab.name);
            }
            __result = list;
            return false;
        }

        private static void DropItems(CharacterDrop.Drop drop, List<KeyValuePair<GameObject, int>> list, bool isBossTrophy, int originalNumberToDrop)
        {
            // if option is turned on and its a boss trophy use orginal drop code to prevent more than 1
            if (MonsterLootMultiplier.disableForBossTrophies.Value && isBossTrophy)
            {
                //Console.Log("Dropping num: " + originalNumberToDrop);
                list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, originalNumberToDrop));
            }
            else
            {
                // Flow when not a boss trophy
                // Added Math.Ceiling otherwise a values like 0.5 would disable all drops that are originally 1
                int numberToDrop = (int)Math.Ceiling(originalNumberToDrop * MonsterLootMultiplier.lootMultiplier.Value);
                list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, numberToDrop));  
                //Console.Log("Dropping num: " + numberToDrop);
            }
        }
    }
}
