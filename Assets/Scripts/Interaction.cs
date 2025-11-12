using UnityEngine;

public class Interaction
{
    public interface IInteractor {
        public virtual void Interact(object target) {
            IInteractable itarget = target as IInteractable;
            if(itarget != null) itarget.OnInteract(this);
        }
    }
    public interface IInteractable {
        public virtual void OnInteract(IInteractor interactor) {
            Debug.Log("Interacted");
        }
    }
}
