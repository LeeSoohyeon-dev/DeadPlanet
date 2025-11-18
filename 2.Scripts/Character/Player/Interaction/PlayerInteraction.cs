using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private List<Interactable> interactables = new List<Interactable>();

    private Interactable closestInteractable;

    private void Start()
    {
        Player player = GetComponent<Player>();

        player.inputAction.Character.Interaction.performed += context => InteractWithClosest();
        UI.instance.inGameUI.LootButton.onClick.AddListener(() => InteractWithClosest());

    }
    private void InteractWithClosest()
    {
        GameEvents.OnPlaySound?.Invoke(SoundType.Loot);
        closestInteractable?.Interaction();
        interactables.Remove(closestInteractable);
        UpdateClosestInteractable();
    }

    public void UpdateClosestInteractable()
    {
        closestInteractable?.HighlightActive(false);
        closestInteractable = null;

        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in interactables)
        {
            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }
        closestInteractable?.HighlightActive(true);

    }

    public List<Interactable> GetInteractables() => interactables;
}
