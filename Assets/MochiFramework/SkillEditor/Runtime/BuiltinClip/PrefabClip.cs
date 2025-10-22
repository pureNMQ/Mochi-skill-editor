using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class PrefabClip : Clip
    {
        public override string ClipName => _prefab.name;
        public override int OriginalDuration => 5;
        [SerializeField] private GameObject _prefab;
    }
}
