using UnityEngine;
using System.Collections.Generic;

namespace MochiFramework.Skill
{
    public class PrefabTrackHandler : TrackHandler
    {
        private PrefabTrack _prefabTrack;
        private Dictionary<int, GameObject> _instantiatedPrefabs = new Dictionary<int, GameObject>();
        private SlotMapper _slotMapper;

        public PrefabTrackHandler(ITrack track, GameObject gameObject) : base(track)
        {
            _prefabTrack = track as PrefabTrack;
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
                PrefabClip prefabClip = track[i] as PrefabClip;
                if (prefabClip == null) continue;
                if (prefabClip.startFrame == currentFrame && prefabClip.Prefab != null)
                {
                    Debug.Log($"实例化预制体 {prefabClip.Prefab.name} 在帧 {currentFrame}");
                    InstantiatePrefab(prefabClip, i);
                }
                else if (currentFrame == prefabClip.EndFrame)
                {
                    // 销毁超出生命周期的预制体
                    if (prefabClip.DestroyOnEnd)
                    {
                        DestroyPrefab(i);
                    }
                }
            }
        }

        public override void Evaluate(int currentFrame)
        {
            // 预览模式下，先清理所有已实例化的预制体
            ClearAllPrefabs();

            if (_slotMapper == null) return;
            // 然后根据当前帧实例化应该显示的预制体
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (currentFrame >= track[i].startFrame && currentFrame <= track[i].EndFrame &&
                    track[i] is PrefabClip prefabClip && prefabClip.Prefab != null)
                {

                    InstantiatePrefab(prefabClip, i);
                }
            }
        }

        public override void Stop()
        {

        }

        private void InstantiatePrefab(PrefabClip prefabClip, int clipIndex)
        {
            // 如果已经实例化过，先销毁旧的
            DestroyPrefab(clipIndex);
            Transform transform = _slotMapper.GetSlotTransform(prefabClip.PrefabSlot);
            if (transform == null)
            {
                Debug.LogWarning($"在技能配置{track.SkillConfig.name} 未找到 {prefabClip.ClipName} 对应的PrefabSlot,无法进行实例化");
                return;
            }
            // 实例化新的预制体
            GameObject instance = Object.Instantiate(prefabClip.Prefab, transform.position, Quaternion.identity, transform);
            _instantiatedPrefabs[clipIndex] = instance;
        }

        private void DestroyPrefab(int clipIndex)
        {
            if (_instantiatedPrefabs.TryGetValue(clipIndex, out GameObject instance))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(instance);
                }
                else
                {
                    Object.DestroyImmediate(instance);
                }
                _instantiatedPrefabs.Remove(clipIndex);
            }
        }

        private void ClearAllPrefabs()
        {
            foreach (var instance in _instantiatedPrefabs.Values)
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
            _instantiatedPrefabs.Clear();
        }
    }
}