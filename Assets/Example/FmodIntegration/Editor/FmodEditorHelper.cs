using FMODUnity;
using MochiFramework.Skill;
using UnityEditor;

public static class FmodEditorHelper
{
    [InitializeOnLoadMethod]
    public static void Startup()
    {
        //FmodTrack.ConvertCondition += (obj) => obj is;
        FmodTrack.ConvertCondition += obj => obj is EditorEventRef;
    }
}