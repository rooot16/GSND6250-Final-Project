using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiPanel;      
    public TextMeshProUGUI promptText; 

    [Header("Raycast Settings")]
    public float detectRange = 2f;
    public LayerMask interactableLayer;
    public Transform playerCameraEye; // Player's eys

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        if (playerCameraEye == null) return;

        RaycastHit hit;
 
        if (Physics.Raycast(playerCameraEye.position, playerCameraEye.forward, out hit, detectRange, interactableLayer))
        {
            Interaction.IInteractable interactable = null;

            if (hit.rigidbody != null)
            {
                interactable = hit.rigidbody.GetComponent<Interaction.IInteractable>();
            }
            else
            {
                 interactable = hit.collider.GetComponent<Interaction.IInteractable>();
            }

            if (interactable != null)
            {
                // Display UI
                ShowPrompt(interactable.GetDescription());
            }
            else
            {
                HidePrompt();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    void ShowPrompt(string message)
    {
        if (uiPanel != null) uiPanel.SetActive(true);
        if (promptText != null) promptText.text = message + "\n[E]";
    }

    void HidePrompt()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
}