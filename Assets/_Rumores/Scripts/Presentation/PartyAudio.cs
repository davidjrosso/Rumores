using UnityEngine;

namespace Rumores.Presentation
{
    public enum PartySound {Select,Transmit,Surprise,Panic,Reveal,Celebrate,Defeat}
    public sealed class PartyAudio : MonoBehaviour
    {
        public const string MusicKey="Rumores.Party.MusicVolume", EffectsKey="Rumores.Party.EffectsVolume";
        AudioSource music,effects;
        readonly AudioClip[] clips=new AudioClip[7];
        float lastSelect=-100,lastEffect=-100;
        public float MusicVolume {get;private set;}
        public float EffectsVolume {get;private set;}
        public bool Ready {get;private set;}
        void Awake()
        {
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;music.spatialBlend=0;
            effects=gameObject.AddComponent<AudioSource>();effects.playOnAwake=false;effects.spatialBlend=0;
            if(FindObjectOfType<AudioListener>()==null)gameObject.AddComponent<AudioListener>();
            music.clip=Resources.Load<AudioClip>("Audio/Music");
            Ready=music.clip!=null;
            for(int i=0;i<clips.Length;i++){clips[i]=Resources.Load<AudioClip>("Audio/"+(PartySound)i);Ready&=clips[i]!=null;}
            SetMusic(PlayerPrefs.GetFloat(MusicKey,.22f));SetEffects(PlayerPrefs.GetFloat(EffectsKey,.50f));
            if(music.clip!=null)music.Play();
            if(!Ready)Debug.LogWarning("Rumores: faltan recursos de audio.");
        }
        static float Volume(float value){return float.IsNaN(value)||float.IsInfinity(value)?0:Mathf.Clamp01(value);}
        public void SetMusic(float value){MusicVolume=Volume(value);music.volume=MusicVolume;PlayerPrefs.SetFloat(MusicKey,MusicVolume);}
        public void SetEffects(float value){EffectsVolume=Volume(value);effects.volume=EffectsVolume;PlayerPrefs.SetFloat(EffectsKey,EffectsVolume);}
        public void Play(PartySound sound)
        {
            int index=(int)sound;if(index<0||index>=clips.Length||clips[index]==null||EffectsVolume<=0)return;
            float now=Time.unscaledTime;
            if(sound==PartySound.Select){if(now-lastSelect<.12f||now-lastEffect<.12f)return;lastSelect=now;}
            else {if(now-lastEffect<.18f)return;lastEffect=now;effects.Stop();}
            effects.PlayOneShot(clips[index]);
        }
        public void SavePreferences(){PlayerPrefs.Save();}
        public void StopEffects(){if(effects!=null)effects.Stop();}
        void OnApplicationPause(bool paused){if(paused){effects.Stop();music.Pause();SavePreferences();}else if(music.clip!=null)music.UnPause();}
        void OnDestroy(){SavePreferences();}
    }
}
