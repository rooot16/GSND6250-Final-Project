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
        while (blackScreen.color.a < 100)
        {
            blackScreen.color = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, blackScreen.color.a + fadeSpeed);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    void Interaction.IInteractable.OnInteract(Interaction.IInteractor interactor)
    {
        Debug.Log("Interacted with");
        StartCoroutine(FadeToBlack());
    }
}
