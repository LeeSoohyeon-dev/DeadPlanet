using UnityEngine;
using VInspector;

public class Interactable : MonoBehaviour
{
    protected PlayerWeaponController weaponController;
    protected MeshRenderer mesh;
    [SerializeField] private Material highlightMaterial;
    private Material defaultMaterial;

    private void Start()
    {
        if (mesh == null)
        {
            mesh = GetComponentInChildren<MeshRenderer>();
        }

        defaultMaterial = mesh.sharedMaterial;

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
        if (active)
        {
            mesh.material = highlightMaterial;
            GameEvents.OnLootButtonUpdate?.Invoke(true);
        }
        else
        {
            mesh.material = defaultMaterial;
            GameEvents.OnLootButtonUpdate?.Invoke(false);
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (weaponController == null)
        {
            weaponController = other.GetComponent<PlayerWeaponController>();
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
