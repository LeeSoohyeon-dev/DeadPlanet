using UnityEngine;

public class Enemy_BossVisuals : MonoBehaviour
{
    private Enemy_Boss enemy;

    [SerializeField] private ParticleSystem landindZoneFx;

    private void Awake()
    {
        enemy = GetComponent<Enemy_Boss>();

        landindZoneFx.transform.parent = null;
        landindZoneFx.Stop();
    }

    public void PlaceLandindZone(Vector3 target)
    {
        landindZoneFx.transform.position = target + new Vector3(0, 0.3f, 0);
        landindZoneFx.Clear();

        var mainModule = landindZoneFx.main;
        mainModule.startLifetime = enemy.travelTimeToTarget * 2;

        landindZoneFx.Play();
    }

    private void OnDestroy()
    {
        if (landindZoneFx != null)
            Destroy(landindZoneFx.gameObject);
    }

}
