using UnityEngine;

public class Enemy_WeaponModel : MonoBehaviour
{
    public AnimatorOverrideController OverrideController;
    public Enemy_MeleeWeaponData weaponData;
    [SerializeField] private GameObject[] trailEffects;

    public Transform[] damagePoints;
    public float attackRadius;

    public void EnableTrailEffect(bool enable)
    {
        foreach (var effect in trailEffects)
        {
            effect.SetActive(enable);
        }
    }

    private void OnDrawGizmos()
    {
        if (damagePoints != null && damagePoints.Length > 0)
        {
            foreach(Transform point in damagePoints)
            {
                Gizmos.DrawWireSphere(point.position, attackRadius);
            }
        }
    }
}
