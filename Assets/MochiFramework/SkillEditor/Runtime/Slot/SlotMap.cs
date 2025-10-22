using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [System.Serializable]
    public class SlotMap
    {
        public SlotDefinition SlotDefinition;
        public List<Transform> SlotTransforms;

        public SlotMap(SlotDefinition slotDefinition)
        {
            SlotDefinition = slotDefinition;
            SlotTransforms = new List<Transform>(slotDefinition.SlotCount);
            for (int i = 0; i < slotDefinition.SlotCount; i++)
            {
                SlotTransforms.Add(null);
            }
        }
    }
}