using UnityEngine;

public class IcePackItem : MonoBehaviour, Interaction.IInteractable
{
    [Header("Interaction Settings")]
    public string objectName = "Pick up Ice Pack";

    public void OnInteract(Interaction.IInteractor interactor)
    {
        MonoBehaviour playerObj = interactor as MonoBehaviour;

        if (playerObj != null)
        {
            IcePackController controller = playerObj.GetComponent<IcePackController>();
            if (controller != null)
            {
                controller.ActivateIcePack();
                Destroy(gameObject);
            }
        }
    }

    public string GetDescription()
    {
        return objectName;
    }
}