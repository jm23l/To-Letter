using UnityEngine;
namespace LetterRun2D
{
    public enum AudioCue { Pickup, Delivery, Speed, Shield, Hit, SlowStart, SlowEnd, Start, Win, Lose, Pause, Resume, Warning, BicycleBell, Button }
    public class GameAudio : MonoBehaviour
    {
        [Range(0,1)] public float effectsVolume=.55f;
        public bool muted;
        private AudioSource source;
        private AudioClip[] clips;
        public void Initialize()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;
            source.spatialBlend=0;source.loop=false;
            string[] names={"pickup","delivery","speed","shield","hit","slow_start","slow_end","start","win","lose","pause","resume","warning","bicycle_bell","button"};
            clips=new AudioClip[names.Length];
            for(int i=0;i<names.Length;i++)clips[i]=Resources.Load<AudioClip>("Audio/"+names[i]);
        }
        public void Play(AudioCue cue)
        {
            if(source==null || clips==null || muted || effectsVolume<=0)return;
            int index=(int)cue;if(index<0 || index>=clips.Length || clips[index]==null)return;
            source.PlayOneShot(clips[index],Mathf.Clamp01(effectsVolume));
        }
        public void Stop(){if(source!=null)source.Stop();}
    }
}
