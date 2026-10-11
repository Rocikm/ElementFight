using NUnit.Framework.Constraints;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    // Returns the current GameManager instance, accessible from anywhere
    public static GameManager Instance
    {
        get
        {
            // If the manager is in a different scene (startup), playing directly from the editor
            // skips it, unless the scene is opened manually with additive mode. This check makes sure
            // the manager exists in that particular case. Not needed right now, since the managers are
            // in the gameplay scene, but it shall remain here in case anything changes.
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return null;
            }
            if (instance == null)
            {
                // The prefab that has a GameManager attached should not contain any
                // scripts with references to scene objects that need to be set
                // through the inspector, otherwise there will be null references.
                var prefab = Resources.Load<GameManager>("Systems");
                if (prefab == null)
                {
                    Debug.LogError("Systems prefab not found in Resources!");
                    return null;
                }
                Instantiate(prefab);
            }
#endif
        return instance;
        }
    }

    [Header("Managers")]
    public EffectManager effectManager;
    public AudioManager audioManager;
    public SaveManager saveManager;

    // Setting execution order in project settings can ensure
    // GameManager's Awake() runs before all other scripts.
    // When set, the order for a script is saved in its .meta file.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeManagers();
    }

    private void InitializeManagers()
    {

    }
}
