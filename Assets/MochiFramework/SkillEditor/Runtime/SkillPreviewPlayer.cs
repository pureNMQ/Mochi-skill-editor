using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class SkillPreviewPlayer : MonoBehaviour
    {
        private GameObject previewObject;
        private SkillConfig _skillConfig;
        
        public void ShowPreviewCharacter(GameObject previewPrefab)
        {

            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            if (previewPrefab != null)
            {
                previewObject = Instantiate(previewPrefab, transform);
            }
        }

        public void SetSkillConfig(SkillConfig skillConfig)
        {
            _skillConfig = skillConfig;
        }

        public void PreviewUpdate(float currentTime,int currentFrame,bool isPlaying = true)
        {
            if(_skillConfig is null) return;
            _skillConfig.PreviewUpdate(currentTime,currentFrame,previewObject,isPlaying);
        }

        public void PreviewStop()
        {
            if(_skillConfig is null) return;
            _skillConfig.PreviewStop(previewObject);
        }
        
    }
}
