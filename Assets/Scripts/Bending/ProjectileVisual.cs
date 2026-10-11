using UnityEngine;

public abstract class ProjectileVisual : MonoBehaviour
{
    public virtual void OnSpawn() {}
    public virtual void OnLaunch() {}
    public virtual void OnImpact() {}
    public virtual void OnDestroy() {}
}
