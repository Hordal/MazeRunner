using UnityEngine;

public class MazeAudio : MonoBehaviour
{
    public static MazeAudio Instance { get; private set; }
    private AudioSource source;
    private void Awake() { Instance = this; source = gameObject.AddComponent<AudioSource>(); }
    public void PlayTone(float frequency, float seconds, float volume = .18f)
    {
        int rate = 22050; int count = Mathf.CeilToInt(rate * seconds); AudioClip clip = AudioClip.Create("효과음", count, 1, rate, false); float[] data = new float[count];
        for (int i = 0; i < count; i++) data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * volume * (1f - (float)i / count);
        clip.SetData(data, 0); source.PlayOneShot(clip);
    }
}
