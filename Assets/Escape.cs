using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Escape : MonoBehaviour, Interaction.IInteractable
{
    [SerializeField]
    private Image blackScreen;

    [SerializeField]
    float fadeSpeed = 0.005f;

    IEnumerator FadeToBlack()
    {
        while (blackScreen.color.a < 1f)
        {
            Color c = blackScreen.color;
            c.a += fadeSpeed; 
            blackScreen.color = c;
            yield return null; 
        }
        ;
    }

    void Interaction.IInteractable.OnInteract(Interaction.IInteractor interactor)
    {
        Debug.Log("Interacted with");
        StartCoroutine(FadeToBlack());
    }

    public void OnInteract(Interaction.IInteractor interactor)
    {
        Debug.Log("Player Escaped! Level Complete.");
        if (blackScreen != null)
        {
            StartCoroutine(FadeToBlack());
        }
    }

    public string GetDescription()
    {
        return "Escape / Finish Level"; 
    }

}
