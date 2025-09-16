namespace MochiFramework.Skill.Audio
{
    public class AudioTrick : Track
    {
        public override string TrackName => "音频轨道";

        public override void Initialize()
        {
            
        }

        public override bool CanConvertToClip(object obj)
        {
            if (obj is UnityEngine.AudioClip)
            {
                return true;
            }
            return false;
        }

        public override Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AudioClip audioClip)
            {
                return InsertAudioClipAtFrame(startFrame, audioClip);
            }

            return null;
        }

        private Clip InsertAudioClipAtFrame(int startFrame, UnityEngine.AudioClip clip)
        {
             //TODO 插入音频剪辑

             return null;
        }
        
        
    }
}