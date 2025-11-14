using UnityEngine;
using VInspector;

public class Enemy_BossVisuals : MonoBehaviour
{
    private Enemy_Boss enemy;

    [SerializeField] private float landingOffset = 1;
    [SerializeField] private ParticleSystem landindZoneFx;

    private void Awake()
    {
        enemy = GetComponent<Enemy_Boss>();

        landindZoneFx.transform.parent = null;
        landindZoneFx.Stop();
    }

    public void PlaceLandindZone(Vector3 target)
    {

        Vector3 dir = target - transform.position;
        Vector3 offset = dir.normalized * landingOffset;
        landindZoneFx.transform.position = target + offset + new Vector3(0, 0.3f,0);
        landindZoneFx.Clear();

        var mainModule = landindZoneFx.main;
        mainModule.startLifetime = enemy.travelTimeToTarget * 2;

        landindZoneFx.Play();
    }

}
