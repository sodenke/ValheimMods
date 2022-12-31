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
                    continue;
                }
                float num2 = drop.m_chance;
                if (drop.m_levelMultiplier)
                {
                    num2 *= (float)num;
                }
                if ((Random.value <= num2))
                {
                    int num3 = Random.Range(drop.m_amountMin, drop.m_amountMax);
                    if (drop.m_levelMultiplier)
                    {
                        num3 *= num;
                    }
                    if (drop.m_onePerPlayer)
                    {
                        num3 = ZNet.instance.GetNrOfPlayers();
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

                        if (MonsterLootMultiplier.whiteListEnabled.Value)
                        {
                            bool itemInWhiteList = MonsterLootMultiplier.whitelist.Contains(drop.m_prefab.name.ToLower());
                            if (itemInWhiteList)
                            {
                                // do extra drops
                                DropItems(drop, list, isBossTrophy, num3);
                            }
                            else
                            {
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
            }
            __result = list;
            return false;
        }

        private static void DropItems(CharacterDrop.Drop drop, List<KeyValuePair<GameObject, int>> list, bool isBossTrophy, int originalNumberToDrop)
        {
            // if option is turned on and its a boss trophy use orginal drop code to prevent more than 1
            if (MonsterLootMultiplier.disableForBossTrophies.Value && isBossTrophy)
            {
                list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, originalNumberToDrop));
            }
            else
            {
                // Flow when not a boss trophy
                // Added Math.Ceiling otherwise a values like 0.5 would disable all drops that are originally 1
                list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, (int)Math.Ceiling(originalNumberToDrop * MonsterLootMultiplier.lootMultiplier.Value)));
            }
        }
    }
}
