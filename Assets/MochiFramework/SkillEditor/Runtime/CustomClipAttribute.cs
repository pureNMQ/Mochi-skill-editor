using System;
using UnityEngine;

namespace MochiFramework.Skill
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited = true)]
    public class CustomClipAttribute : Attribute
    {
        public readonly string HexColor;
        public CustomClipAttribute(string hexColor = "#757575")
        {
            HexColor = hexColor;
        }
    }
}
