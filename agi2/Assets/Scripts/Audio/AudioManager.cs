using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    private static AudioManager instance;

    [SerializeField] private Sound[] sounds;


    public static AudioManager Instance { get { return instance; } }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

        }
    }

    void Start()
    {
        
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Stop(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public void SetVolume(string name, float volume) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.volume = volume;
    }

    public void TankSound(Vector2 inputVelocity) {
        float magnitude = inputVelocity.magnitude;
        magnitude /= Mathf.Sqrt(2); // make it easier to deal with

        float slowVol;
        float mediumVol;
        float fastVol;

        if(magnitude <= 1.0f / 2.0f) {
            slowVol = Mathf.Lerp(1.0f, 0.0f, magnitude * 2.0f);
            mediumVol = 1.0f - slowVol;
            fastVol = 0.0f;
        } else {
            slowVol = 0.0f;
            mediumVol = Mathf.Lerp(1.0f, 0.0f, (magnitude - 1.0f / 2.0f) * 2.0f);
            fastVol = 1.0f - mediumVol;
        }

        SetVolume("tankSlow", slowVol);
        SetVolume("tankMedium", mediumVol);
        SetVolume("tankFast", fastVol);
    }

}
