
using System.Collections;
using UnityEngine;

public class VRScreenFade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(1f, duration);
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, duration);
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        float startAlpha = canvasGroup.alpha;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        yield return StartCoroutine(FadeRoutine(
            startAlpha, targetAlpha, duration
        ));
    }

    private IEnumerator FadeRoutine(
        float startAlpha,
        float targetAlpha,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
