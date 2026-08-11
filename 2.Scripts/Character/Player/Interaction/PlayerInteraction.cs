using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteraction : MonoBehaviour
{
    private List<Interactable> interactables = new List<Interactable>();

    private Interactable closestInteractable;

    private UnityAction lootButtonAction;

    private void Start()
    {
        Player player = GetComponent<Player>();

        player.inputAction.Character.Interaction.performed += context => InteractWithClosest();

        lootButtonAction = () => InteractWithClosest();
        UI.instance.inGameUI.LootButton.onClick.AddListener(lootButtonAction);
    }

    private void OnDestroy()
    {
        if (lootButtonAction != null && UI.instance != null && UI.instance.inGameUI != null)
            UI.instance.inGameUI.LootButton.onClick.RemoveListener(lootButtonAction);
    }
    private void InteractWithClosest()
    {
        if (closestInteractable == null)
            return;

        GameEvents.RaisePlaySound(SoundType.Loot);
        closestInteractable.Interaction();
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
