using UnityEngine;

public class HandDamage : MonoBehaviour, IDamageable
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private Fighter player;
    [SerializeField] private PlayerData playerData;

    public void TakeDamage(ElementData elementData)
    {
        if (gestureInput.velocity.magnitude > playerData.minBlockSpeed)
            return;

        player.TakeDamage(elementData);
    }
}
