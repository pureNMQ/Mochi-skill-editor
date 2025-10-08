using System;
using System.Reflection;
using UnityEngine;

namespace MochiFramework.Skill.MochiFramework.SkillEditor.Editor
{
    public static class TrackBuilder
    {
        public static ITrack Build(Type type,SkillConfig skillConfig)
        {
            if(skillConfig == null) return null;
            if (type == null) return null;
            if (!(typeof(ITrack).IsAssignableFrom(type))) return null;
            Debug.Log("通过类型检查");
            //创建对应类型的Track
            ITrack track =  (ITrack)Activator.CreateInstance(type);
            //注入skillConfig
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo skillConfigField  = type.GetField("skillConfig",flags);
            skillConfigField.SetValue(track, skillConfig);
            
            //完成自定义的初始化方法
            track.Initialize();
            
            Debug.Log($"创建新的Track{track}");
            return track;
        }
    }
}