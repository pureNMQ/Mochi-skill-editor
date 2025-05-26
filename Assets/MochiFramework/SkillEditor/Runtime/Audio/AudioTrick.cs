namespace MochiFramework.Skill.Audio
{
    public class AudioTrick : Track
    {
        public override string TrackName => "音频轨道";
        public override int ClipCount => clips.Count;
    }
}