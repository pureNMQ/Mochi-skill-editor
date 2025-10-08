using UnityEngine;

namespace MochiFramework.Skill
{
    public interface ISkillPlayer
    {
        public SkillConfig CurrentSkill { get; }
        public float CurrentTime { get; }
        public int CurrentFrame { get; }
        public bool IsPlaying { get; }
        public Animator Animator { get; }
    
        public void PlaySkill(SkillConfig skill);
        public void PlaySkill();
        public void StopCurrentSkill();

        public void Rebuild();
    }
}