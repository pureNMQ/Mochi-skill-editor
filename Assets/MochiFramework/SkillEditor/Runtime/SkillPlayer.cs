using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class SkillPlayer : MonoBehaviour,ISkillPlayer
    {
        public SkillConfig CurrentSkill => currentSkill;
        public float CurrentTime => currentTime;
        public int CurrentFrame { get; }
        public bool IsPlaying { get; }

        [SerializeField] private SkillConfig currentSkill;

        private float currentTime = 0; 
        
        public void PlaySkill(SkillConfig skill)
        {
            currentSkill = skill;
            PlaySkill();
        }

        public void PlaySkill()
        {
            if (currentSkill != null)
            {
                currentTime = 0;
            }
        }

        public void StopCurrentSkill()
        {
            
        }


        private void Update()
        {
            if(IsPlaying) return;
            currentTime += Time.deltaTime;
            
        }
    }

    public interface ISkillPlayer
    {
        public SkillConfig CurrentSkill { get; }
        public float CurrentTime { get; }
        public int CurrentFrame { get; }
        public bool IsPlaying { get; }
        
        public void PlaySkill(SkillConfig skill);
        public void PlaySkill();
        public void StopCurrentSkill();
    }
}
