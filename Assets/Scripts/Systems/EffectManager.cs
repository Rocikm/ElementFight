using UnityEngine;
using System.Collections;
using TMPro;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;

    /// <summary>
    /// Spawn a temporary particle effect (detached from parent) with custom options
    /// </summary>
    public void Play(ParticleSystem prefab, Vector3 position, Quaternion rotation, EffectOptions options)
    {
        ParticleSystem ps = Instantiate(prefab, position, rotation);

        SetupAndPlay(ps, options, true);
    }

    /// <summary>
    /// Play a temporary particle effect that follows a parent transform
    /// </summary>
    public void PlayAttached(ParticleSystem prefab, Transform parent, EffectOptions options)
    {
        var ps = Instantiate(prefab, parent);
        ps.transform.localPosition = Vector3.zero;

        SetupAndPlay(ps, options, true);
        
    }

    /// <summary>
    /// Play an existing particle effect with custom options
    /// </summary>
    public void PlayExisting(ParticleSystem particleSystem, EffectOptions options)
    {
        SetupAndPlay(particleSystem, options, false);
    }

    public void SpawnText(Transform transform, string text, Color color, float duration)
    {
        GameObject obj = Instantiate(textPrefab, transform);

        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>();
        tmp.text = text;
        tmp.color = color;
        
        // tmp.fontMaterial.SetColor("_OutlineColor", Color.black);
        // tmp.fontMaterial.SetFloat("_OutlineWidth", 0.3f);

        StartCoroutine(FadeAndDestroy(tmp, duration));
    }

    private void ApplyOptions(ParticleSystem ps, EffectOptions options)
    {
        var main = ps.main;

        if (options.color.HasValue)
            main.startColor = options.color.Value;
        if (options.duration.HasValue)
            StartCoroutine(StopAfterTime(ps, options.duration.Value));
            // main.duration = options.duration.Value;
    }

    private void SetupAndPlay(ParticleSystem ps, EffectOptions options, bool destroyAfterPlay)
    {
        var main = ps.main;

        if (destroyAfterPlay)
            main.stopAction = ParticleSystemStopAction.Destroy;

        ApplyOptions(ps, options);

        ps.Play();
    }

    private IEnumerator StopAfterTime(ParticleSystem ps, float t)
    {
        yield return new WaitForSeconds(t);

        if (ps == null)
        yield break;
        
        ps.Stop();
    }

    private IEnumerator FadeAndDestroy(TMP_Text tmp, float duration)
{
    Color color = tmp.color;

    float fadeInTime = 0.2f;
    float fadeOutTime = 0.2f;

    // Start invisible
    tmp.color = new Color(color.r, color.g, color.b, 0f);

    // Fade in
    float time = 0f;

    while (time < fadeInTime)
    {
        time += Time.deltaTime;

        float alpha = Mathf.Lerp(0f, 1f, time / fadeInTime);
        tmp.color = new Color(color.r, color.g, color.b, alpha);

        yield return null;
    }

    // Wait
    yield return new WaitForSeconds(duration);

    // Fade out
    time = 0f;

    while (time < fadeOutTime)
    {
        time += Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, time / fadeOutTime);
        tmp.color = new Color(color.r, color.g, color.b, alpha);

        yield return null;
    }

    Destroy(tmp.gameObject);
}
}

public struct EffectOptions
{
    public Color? color;
    public float? duration;
}
