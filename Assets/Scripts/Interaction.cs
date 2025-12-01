using UnityEngine;

public class Interaction
{
    public interface IInteractor
    {
        public void Interact(object target)
        {
            IInteractable itarget = target as IInteractable;
            if (itarget != null)
            {
                itarget.OnInteract(this);
            }
        }
    }

    public interface IInteractable
    {
        // Press E to interact
        void OnInteract(IInteractor interactor);
        string GetDescription();
    }
}