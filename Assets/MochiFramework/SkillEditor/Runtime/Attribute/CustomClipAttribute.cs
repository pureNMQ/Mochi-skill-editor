using System;

namespace MochiFramework.Skill
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited = true)]
    public class CustomClipAttribute : Attribute
    {
        public string HexColor = "#757575";
    }
}
