using System.Collections;
using UnityEngine;

public class Enemy_Ragdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollParent;

    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;

    private void Awake()
    {
        ragdollColliders = ragdollParent.GetComponentsInChildren<Collider>(true);
        ragdollRigidbodies = ragdollParent.GetComponentsInChildren<Rigidbody>(true);

        RagdollActive(false);
    }

    public void RagdollActive(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !active;
        }
    }

    public void CollidersActive(bool active)
    {
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = active;
        }
    }

}
