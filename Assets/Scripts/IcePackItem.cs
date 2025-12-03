using System;
using System.Threading.Tasks;
using UnityEngine;

public class IcePackItem : MonoBehaviour, Interaction.IInteractable
{
    [Header("Interaction Settings")]
    public string objectName = "Pick up Ice Pack";

    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfx_ice;

    public async void OnInteract(Interaction.IInteractor interactor)
    {
        MonoBehaviour playerObj = interactor as MonoBehaviour;

        Transform parentTransform = transform.parent;

        if (parentTransform != null)
        {
            OpenLid parentScript = parentTransform.GetComponent<OpenLid>();

            if (parentScript != null)
            {
                parentScript.ToggleLid();

                await Task.Delay(TimeSpan.FromSeconds(1));

                if (playerObj != null)
                {
                    IcePackController controller = playerObj.GetComponent<IcePackController>();
                    if (controller != null)
                    {
                        controller.ActivateIcePack();
                        if (audioSource != null && sfx_ice != null)
                        {
                            audioSource.PlayOneShot(sfx_ice);
                        }
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