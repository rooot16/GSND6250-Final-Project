using UnityEngine;

public class PickupableObject: MonoBehaviour, Interaction.IInteractable
{
    public void OnInteract(Interaction.IInteractor target = null) {
        Destroy(this.gameObject);
    }
}
