using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class Clip : ScriptableObject
    {
        public abstract string ClipName { get; }

        public int StartFrame
        {
            get => startFrame;
            set => startFrame = value;
        }

        public int EndFrame
        {
            get => startFrame + duration;
        }

        public int Duration
        {
            get => duration;
            set => duration = value;
        }

        [SerializeField] protected int startFrame;
        [SerializeField] protected int duration;


    }
}
