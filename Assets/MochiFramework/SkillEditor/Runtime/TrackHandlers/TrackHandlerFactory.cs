namespace MochiFramework.Skill
{
    public static class TrackHandlerFactory
    {
        public static TrackHandler Create(Track track,ISkillPlayer player)
        {
            switch (track)
            {
                case AnimationTrack animationTrack:
                    if(player.Animator == null) return null;
                    return new AnimationTrackHandler(animationTrack, player.Animator);
                
                case AudioTrack audioTrack:
                    return new AudioTrackHandler(audioTrack);
            }
            
            return null;
        }
    }
}