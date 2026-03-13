using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [Serializable]
    public struct SlotEntry
    {
        public SlotDefinition SlotDefinition
        {
            get => _slotDefinition;
            set => _slotDefinition = value;
        }
        
        public int SlotIndex
        {
            get => _slotIndex;
            set => _slotIndex = value;
        }
        
        [SerializeField] private SlotDefinition _slotDefinition;
        [SerializeField] private int _slotIndex;

        public SlotEntry(SlotDefinition slotDefinition, int slotIndex)
        {
            _slotDefinition = slotDefinition;
            _slotIndex = slotIndex;
        }
    }
}
