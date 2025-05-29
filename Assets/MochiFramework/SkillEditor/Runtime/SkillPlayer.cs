using System;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill.Editor
{
    public class SkillPlayer : MonoBehaviour,ISkillPlayer
    {
        public SkillConfig CurrentSkill => currentSkill;
        public float CurrentTime => currentTime;
        public int CurrentFrame { get; }
        public bool IsPlaying => isPlaying;
        public Animator Animator => animator;

        [SerializeField] private SkillConfig currentSkill;

        private float currentTime = 0;
        private int currentFrame = 0;
        private bool isPlaying = false;
        private List<TrackHandler> trackHandlers;
        
        private Animator animator;
        
        
        public void PlaySkill(SkillConfig skill)
        {
            currentSkill = skill;
            PlaySkill();
        }
        
        [ContextMenu("播放技能")]
        public void PlaySkill()
        {
            if (currentSkill != null)
            {
                currentTime = 0;
                Debug.Log($"开始播放技能:{currentTime}");
                Rebuild();

                foreach (var handler in trackHandlers)
                {
                    handler.Play();
                }
                isPlaying = true;
            }
            else
            {
                isPlaying = false;
                
            }
        }

        public void StopCurrentSkill()
        {
            foreach (var handler in trackHandlers)
            {
                handler.Stop();
            }

            isPlaying = false;
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            isPlaying = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                PlaySkill();
            }
            
            
            if(!IsPlaying) return;
            
            currentTime += Time.deltaTime;
            currentFrame = Convert.ToInt32(currentTime * currentSkill.frameRate);
            foreach (var handler in trackHandlers)
            {
                handler.Update(Time.deltaTime,currentFrame,currentTime);
                Debug.Log($"轨道更新:{currentFrame},{currentTime}");
            }

            if (currentFrame >= currentSkill.frameCount)
            {
                StopCurrentSkill();
                Debug.Log("技能播放结束");
            }
        }

        private void OnDestroy()
        {
            DestroyTrackHandlers();
        }


        public void Rebuild()
        {
            DestroyTrackHandlers();
            trackHandlers = new List<TrackHandler>();
            foreach (var track in currentSkill.tracks)
            {
                TrackHandler handler = TrackHandlerFactory.Create(track,this);
                if (handler is not null)
                {
                    trackHandlers.Add(handler);
                }
            }
        }

        private void DestroyTrackHandlers()
        {
            if(trackHandlers is null) return;
            foreach (var handler in trackHandlers)
            {
                handler.Dispose();
            }
            trackHandlers = null;
        }
    }
    
}
