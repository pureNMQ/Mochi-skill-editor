using System;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class EffectClip : Clip
    {
        public override string ClipName => _effectPrefab == null ? "未指定特效预制体" : _effectPrefab.name;
        public override int OriginalDuration => _effectPrefab == null ? 0 : (int)_effectPrefab.GetComponent<ParticleSystem>()?.main.duration;

        public GameObject EffectPrefab
        {
            get => _effectPrefab;
            set => _effectPrefab = value;
        }

        public SlotEntry EffectSlot
        {
            get => _effectSlot;
            set => _effectSlot = value;
        }

        public bool DestroyOnEnd
        {
            get => _destroyOnEnd;
            set => _destroyOnEnd = value;
        }

        [SerializeField] private GameObject _effectPrefab;
        [SerializeField] private SlotEntry _effectSlot;
        [SerializeField] private bool _destroyOnEnd = false;
    }
}