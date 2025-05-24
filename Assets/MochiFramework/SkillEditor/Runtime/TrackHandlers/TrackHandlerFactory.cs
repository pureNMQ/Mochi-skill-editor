namespace MochiFramework.Skill
{
    public static class TrackHandlerFactory
    {
        public static TrackHandler Create(SkillTrack skillTrack,ISkillPlayer player)
        {
            switch (skillTrack)
            {
                case SkillAnimationSkillTrack animationTrack:
                    if(player.Animator == null) return null;
                    return new AnimationTrackHandler(animationTrack, player.Animator);
            }
            
            return null;
        }
    }
}