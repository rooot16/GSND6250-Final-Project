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

    // 物品 (IcePack, Escape Door) 实现这个接口
    public interface IInteractable
    {
        // Press E to interact
        void OnInteract(IInteractor interactor);
        string GetDescription();
    }
}