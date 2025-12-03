using System;
using System.Threading.Tasks;
using UnityEngine;

public class IcePackItem : MonoBehaviour, Interaction.IInteractable
{
    [Header("Interaction Settings")]
    public string objectName = "Pick up Ice Pack";

    public async void OnInteract(Interaction.IInteractor interactor)
    {
        MonoBehaviour playerObj = interactor as MonoBehaviour;


        // Get the parent's Transform
        Transform parentTransform = transform.parent;

        // Check if a parent exists
        if (parentTransform != null)
        {
            // Get a reference to the script on the parent (replace ParentScriptType with your actual parent script's class name)
            OpenLid parentScript = parentTransform.GetComponent<OpenLid>();

            // Check if the script exists on the parent
            if (parentScript != null)
            {
                // Call a public method on the parent script
                parentScript.ToggleLid();

                await Task.Delay(TimeSpan.FromSeconds(1));

                if (playerObj != null)
                {
                    IcePackController controller = playerObj.GetComponent<IcePackController>();
                    if (controller != null)
                    {
                        controller.ActivateIcePack();
                        parentScript.ToggleLid();
                        Destroy(gameObject);

                    }
                }
            }
            else
            {
                Debug.LogWarning("ParentScriptType not found on the parent GameObject.");
            }
        }


    }

    public string GetDescription()
    {
        return objectName;
    }
}