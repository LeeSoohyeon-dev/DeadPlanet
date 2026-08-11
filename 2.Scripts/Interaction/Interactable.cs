using UnityEngine;

public class Interactable : MonoBehaviour
{
    protected PlayerWeaponController weaponController;
    protected MeshRenderer mesh;
    [SerializeField] private Material highlightMaterial;
    private Material defaultMaterial;

    protected virtual void Start()
    {
        if (mesh == null)
        {
            mesh = GetComponentInChildren<MeshRenderer>();
        }

        // 이미 캡처된 기본 머티리얼을 덮어쓰지 않음 (하이라이트 상태로 굳는 것 방지)
        if (mesh != null && defaultMaterial == null)
        {
            defaultMaterial = mesh.sharedMaterial;
        }
    }

    protected void UpdateMeshAndMaterials(MeshRenderer newMesh)
    {
        mesh = newMesh;
        defaultMaterial = newMesh.sharedMaterial;
    }

    public virtual void Interaction()
    {
    }
    public void HighlightActive(bool active)
    {
        if (mesh != null)
        {
            mesh.sharedMaterial = active ? highlightMaterial : defaultMaterial;
        }

        GameEvents.RaiseLootButtonUpdate(active);
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (weaponController == null)
        {
            weaponController = other.GetComponentInParent<PlayerWeaponController>();
        }
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        playerInteraction.GetInteractables().Add(this);
        playerInteraction.UpdateClosestInteractable();

    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        playerInteraction.GetInteractables().Remove(this);
        playerInteraction.UpdateClosestInteractable();

    }
}
