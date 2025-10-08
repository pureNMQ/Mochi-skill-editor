using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MochiFramework.Skill;
using UnityEditor;
using UnityEngine;
using AudioClip = UnityEngine.AudioClip;

public static class EditorAudioUnility
{
    private static MethodInfo playClipMethod;
    private static MethodInfo stopClipMethod;

    static EditorAudioUnility()
    {
        Assembly editorAssembly = typeof(AudioImporter).Assembly;
        //UnityEditor.AudioUtil
        Type audioUtilType = editorAssembly.GetType("UnityEditor.AudioUtil");
        playClipMethod = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null,
            new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
        stopClipMethod = audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public);
    }

    [InitializeOnLoadMethod]
    private static void Ready()
    {
        Debug.Log("初始化");
        AudioTrackHandler.PlayPreviewClip += (sender, clip) =>
        {
            PlayPreviewClip(clip,0);
        };

        AudioTrackHandler.StopPreviewClip += (sender, _) =>
        {
            StopAllPreviewClips();
        };

    }
    
    public static void PlayPreviewClip(AudioClip clip, float start)
    {
        playClipMethod.Invoke(null, new object[] { clip, (int)(start * clip.frequency), false });
    }

    public static void StopAllPreviewClips()
    {
        stopClipMethod.Invoke(null, null);
    }
}
