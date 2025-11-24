using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [Serializable]
    public class PrefabClip : Clip
    {
        public override string ClipName => _prefab == null ? "未指定预制体" : _prefab.name;
        public override int OriginalDuration => 5;

        public GameObject Prefab
        {
            get => _prefab;
            set => _prefab = value;
        }

        [SerializeField] private GameObject _prefab;
        [SerializeField] private SlotEntry _prefabSlot;
    }
}
