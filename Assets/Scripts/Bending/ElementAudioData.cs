using UnityEngine;

[CreateAssetMenu(menuName = "Bending/Audio")]
public class ElementAudioData : ScriptableObject
{
    public AudioClip[] spawnClips;
    public AudioClip[] launchClips;
    public AudioClip[] impactClips;
}
