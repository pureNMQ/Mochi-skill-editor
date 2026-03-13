using UnityEngine;
using System.Collections.Generic;

namespace MochiFramework.Skill
{
    public class EffectTrackHandler : TrackHandler
    {
        private EffectTrack _effectTrack;
        private Dictionary<int, GameObject> _instantiatedEffects = new Dictionary<int, GameObject>();
        private Dictionary<int, ParticleSystem> _particleSystems = new Dictionary<int, ParticleSystem>();
        private SlotMapper _slotMapper;

        public EffectTrackHandler(ITrack track, GameObject gameObject) : base(track)
        {
            _effectTrack = track as EffectTrack;
            _slotMapper = gameObject.GetComponentInChildren<SlotMapper>();

            if (_slotMapper == null)
            {
                Debug.LogError($"GameObject {gameObject.name} 上没有找到 SlotMapper 组件");
            }
        }

        public override void Play(int currentFrame = 0)
        {
            // 播放模式下，从当前帧开始处理
            Evaluate(currentFrame);
        }

        public override void Update(int currentFrame)
        {
            if (_slotMapper == null) return;
            // 运行模式下，根据当前帧更新轨道上的状态
            for (int i = 0; i < track.ClipCount; i++)
            {
                EffectClip effectClip = track[i] as EffectClip;
                if (effectClip == null) continue;
                if (effectClip.startFrame == currentFrame && effectClip.EffectPrefab != null)
                {
                    Debug.Log($"实例化特效 {effectClip.EffectPrefab.name} 在帧 {currentFrame}");
                    InstantiateEffect(effectClip, i);
                    PlayParticleSystem(i);
                }
                else if (currentFrame == effectClip.EndFrame)
                {
                    // 停止并销毁超出生命周期的特效
                    StopParticleSystem(i);
                    if (effectClip.DestroyOnEnd)
                    {
                        DestroyEffect(i);
                    }
                }
            }
        }

        public override void Evaluate(int currentFrame)
        {
            // 预览模式下，先清理所有已实例化的特效
            ClearAllEffects();

            if (_slotMapper == null) return;
            // 然后根据当前帧实例化应该显示的特效
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (currentFrame >= track[i].startFrame && currentFrame <= track[i].EndFrame &&
                    track[i] is EffectClip effectClip && effectClip.EffectPrefab != null)
                {
                    InstantiateEffect(effectClip, i);
                    PlayParticleSystem(i);
                }
            }
        }

        public override void Stop()
        {
            // 停止所有粒子系统
            foreach (var particleSystem in _particleSystems.Values)
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                }
            }
        }

        private void InstantiateEffect(EffectClip effectClip, int clipIndex)
        {
            // 如果已经实例化过，先销毁旧的
            DestroyEffect(clipIndex);
            Transform transform = _slotMapper.GetSlotTransform(effectClip.EffectSlot);
            if (transform == null)
            {
                Debug.LogWarning($"在技能配置{track.SkillConfig.name} 未找到 {effectClip.ClipName} 对应的EffectSlot,无法进行实例化");
                return;
            }
            // 实例化新的特效预制体
            GameObject instance = Object.Instantiate(effectClip.EffectPrefab, transform.position, Quaternion.identity, transform);
            _instantiatedEffects[clipIndex] = instance;

            // 获取粒子系统组件
            ParticleSystem particleSystem = instance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                _particleSystems[clipIndex] = particleSystem;
            }
        }

        private void PlayParticleSystem(int clipIndex)
        {
            if (_particleSystems.TryGetValue(clipIndex, out ParticleSystem particleSystem))
            {
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
            }
        }

        private void StopParticleSystem(int clipIndex)
        {
            if (_particleSystems.TryGetValue(clipIndex, out ParticleSystem particleSystem))
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                }
            }
        }

        private void DestroyEffect(int clipIndex)
        {
            if (_instantiatedEffects.TryGetValue(clipIndex, out GameObject instance))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(instance);
                }
                else
                {
                    Object.DestroyImmediate(instance);
                }
                _instantiatedEffects.Remove(clipIndex);
                _particleSystems.Remove(clipIndex);
            }
        }

        private void ClearAllEffects()
        {
            foreach (var instance in _instantiatedEffects.Values)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(instance);
                }
                else
                {
                    Object.DestroyImmediate(instance);
                }
            }
            _instantiatedEffects.Clear();
            _particleSystems.Clear();
        }
    }
}