using System;

namespace MochiFramework.Skill
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited = true)]
    public class CustomTrackAttribute : Attribute
    {
        public string HexColor = "#757575";
        public string DefaultName = null;
    }
}