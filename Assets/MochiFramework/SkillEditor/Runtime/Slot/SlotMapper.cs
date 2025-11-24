using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class SlotMapper : MonoBehaviour
    {
        public List<SlotMap> SlotMaps => slotMaps;

        [SerializeField] private List<SlotMap> slotMaps = new List<SlotMap>();

        public void AddSlotMap(SlotDefinition slotDefinition)
        {
            if (slotMaps.Find(x => x.SlotDefinition == slotDefinition) == null)
            {
                slotMaps.Add(new SlotMap(slotDefinition));
            }
            else
            {
                Debug.LogWarning("SlotMap already exists: " + slotDefinition.name);
            }
        }

        public Transform GetSlotTransform(SlotEntry slotEntry)
        {
            for (int i = 0; i < slotMaps.Count; i++)
            {
                if (slotMaps[i].SlotDefinition != slotEntry.SlotDefinition) continue;
                if (slotMaps[i].SlotTransforms.Count > slotEntry.SlotIndex)
                {
                    return slotMaps[i].SlotTransforms[slotEntry.SlotIndex];
                }
            }

            return null;
        }

        //在编辑器中打印SlotMap
        [ContextMenu("打印SlotMap")]
        public void PrintSlotMaps()
        {
            if (slotMaps.Count <= 0)
            {
                Debug.LogWarning("No Slot Map");
                return;
            }

            for (int i = 0; i < slotMaps.Count; i++)
            {
                string info = "";
                info += "SlotMap: " + slotMaps[i].SlotDefinition.name;
                for (int j = 0; j < slotMaps[i].SlotTransforms.Count; j++)
                {
                    if (slotMaps[i].SlotTransforms[j] != null)
                    {
                        info += "\n" + slotMaps[i].SlotDefinition.SlotNames[j] + ": " + slotMaps[i].SlotTransforms[j].name;
                    }
                    else
                    {
                        info += "\n" + slotMaps[i].SlotDefinition.SlotNames[j] + ": null";
                    }
                }
                Debug.Log(info);

            }
        }
    }

}
