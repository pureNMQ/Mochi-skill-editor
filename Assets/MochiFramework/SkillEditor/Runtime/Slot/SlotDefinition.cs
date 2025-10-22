using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [CreateAssetMenu(menuName = "SlotDefinition", fileName = "NewSlotDefinition")]
    public class SlotDefinition : ScriptableObject
    {
        public string[] SlotNames => slots; 
        public int SlotCount => slots.Length;
        
        [SerializeField] private string[] slots;

        public int? GetSlotIdFromName(string slotName)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == slotName)
                {
                    return i;
                }
            }
            
            Debug.LogWarning($"{this.name}中未找到{slotName}");
            return null;
        }
    }
}
