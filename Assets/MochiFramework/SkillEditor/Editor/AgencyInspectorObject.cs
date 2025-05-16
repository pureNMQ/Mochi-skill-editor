using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class AgencyInspectorObject : ScriptableObject
    {
        [SerializeReference]public object target;
    }
}
