namespace MochiFramework.Skill
{

    public class AnimationTrack : Track
    {
        public override string TrackName => "动画轨道";
        public override int ClipCount => clips.Count;
       //[SerializeField,SerializeReference] protected List<AnimationClip> clips = new List<AnimationClip>();
        
        public static AnimationTrack CreateAnimationTrack(SkillConfig skillConfig)
        {
            AnimationTrack animationTrack = new AnimationTrack();
            animationTrack.skillConfig = skillConfig;
            return animationTrack;
        }
        
    }
}
