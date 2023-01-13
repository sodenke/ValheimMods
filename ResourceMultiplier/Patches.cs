using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DropTable;

namespace ResourceMultiplierMod
{
    public class Patches
    {
        [HarmonyPatch(typeof(DropTable), "GetDropList", new Type[] { })]
        [HarmonyPrefix]
        private static bool MultiplyResources(DropTable __instance, ref List<GameObject> __result)
        {
            List<DropTable.DropData> list = new List<DropTable.DropData>(__instance.m_drops);
            int amount;
            float multiplier = ResourceMultiplier.resourceMultiplier.Value;

            List<GameObject> dropList = new List<GameObject>();
            
            if (ResourceMultiplier.whiteListEnabled.Value)
            {
                foreach (DropTable.DropData item in list)
                {
                    if (item.m_item.name == null)
                    {
                        continue;
                    }
                    var itemName = item.m_item.name.ToLower();
                    if (ResourceMultiplier.whitelist.Contains(itemName))
                    {
                        amount = (int)Math.Ceiling(UnityEngine.Random.Range(__instance.m_dropMin, __instance.m_dropMax + 1) * multiplier);
                        dropList.AddRange(GetDropList(__instance, amount));

                        // some special code for honey and queen bee
                        foreach (DropTable.DropData item2 in list)
                        {
                            int num = (int)Math.Ceiling(UnityEngine.Random.Range(item.m_stackMin, item.m_stackMax) * multiplier);
                            if (item.m_item.name.Equals("Honey") || item.m_item.name.Equals("QueenBee"))
                            {
                                for (int j = 0; j < num; j++)
                                {
                                    dropList.Add(item.m_item);
                                }
                            }
                        }
                    }
                    else
                    {
                        amount = UnityEngine.Random.Range(__instance.m_dropMin, __instance.m_dropMax + 1);
                        dropList.AddRange(GetDropList(__instance, amount));
                    }
                }
            }
            else
            {
                // calc and add multiplied loot
                amount = (int)Math.Ceiling(UnityEngine.Random.Range(__instance.m_dropMin, __instance.m_dropMax + 1) * multiplier);
                dropList.AddRange(GetDropList(__instance, amount));

                // some special code for honey and queen bee
                foreach (DropTable.DropData item in list)
                {
                    int num = (int)Math.Ceiling(UnityEngine.Random.Range(item.m_stackMin, item.m_stackMax) * multiplier);
                    if (item.m_item.name.Equals("Honey") || item.m_item.name.Equals("QueenBee"))
                    {
                        for (int j = 0; j < num; j++)
                        {
                            dropList.Add(item.m_item);
                        }
                    }
                }
            }

            __result = dropList;
            return false;
        }

        private static List<GameObject> GetDropList(DropTable __instance, int amount)
        {
            List<GameObject> list = new List<GameObject>();
            if (__instance.m_drops.Count == 0)
            {
                return list;
            }
            if (UnityEngine.Random.value > __instance.m_dropChance)
            {
                return list;
            }
            List<DropData> list2 = new List<DropData>(__instance.m_drops);
            float num = 0f;
            foreach (DropData item in list2)
            {
                num += item.m_weight;
            }
            for (int i = 0; i < amount; i++)
            {
                float num2 = UnityEngine.Random.Range(0f, num);
                bool flag = false;
                float num3 = 0f;
                foreach (DropData item2 in list2)
                {
                    num3 += item2.m_weight;
                    if (num2 <= num3)
                    {
                        flag = true;
                        int num4 = UnityEngine.Random.Range(item2.m_stackMin, item2.m_stackMax);
                        for (int j = 0; j < num4; j++)
                        {
                            list.Add(item2.m_item);
                        }
                        if (__instance.m_oneOfEach)
                        {
                            list2.Remove(item2);
                            num -= item2.m_weight;
                        }
                        break;
                    }
                }
                if (!flag && list2.Count > 0)
                {
                    list.Add(list2[0].m_item);
                }
            }
            return list;
        }

        [HarmonyPatch(typeof(Pickable), "Drop")]
        [HarmonyPrefix]
        private static bool MultiplyPickables(Pickable __instance, GameObject __0, int __1, int __2)
        {
            var item = __0;
            var offset = __1;
            var stack = __2;
            var multilpier = ResourceMultiplier.pickableMultiplier.Value;

            if (ResourceMultiplier.whiteListEnabled.Value)
            {
                // set multiplier to 1 since whitelist is enabled
                multilpier = 1;
                if (item.name != null)
                {
                    var itemName = item.name.ToLower();
                    if (ResourceMultiplier.whitelist.Contains(itemName))
                    {
                        // if in whitelist set to multiplier in config
                        multilpier = ResourceMultiplier.pickableMultiplier.Value;
                    }
                }
            }

            DropItems(__instance, item, offset, (int)Math.Ceiling(stack * multilpier));

            return false;
        }

        private static void DropItems(Pickable __instance, GameObject item, int offset, int stack)
        {
            Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.2f;
            Vector3 position = __instance.gameObject.transform.position + Vector3.up * __instance.m_spawnOffset + new Vector3(vector.x, 0.5f * (float)offset, vector.y);
            Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);
            GameObject obj = UnityEngine.Object.Instantiate(item, position, rotation);
            obj.GetComponent<ItemDrop>().SetStack(stack);
        }
    }
}
