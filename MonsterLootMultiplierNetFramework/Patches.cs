using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonsterLootMultiplierMod
{
    public class Patches
    {
        [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
        [HarmonyPrefix]
        private static bool LootMultiplierCustom(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
        {
            List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
            // m_character is no longer public so no level multiplier for this mod.
            //int num = ((!__instance.m_character) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, __instance.m_character.GetLevel() - 1)));
            int num = 1;
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
                        list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, num3 * MonsterLootMultiplier.lootMultiplier.Value));
                    }
                }
            }
            __result = list;
            return false;
        }
    }
}
