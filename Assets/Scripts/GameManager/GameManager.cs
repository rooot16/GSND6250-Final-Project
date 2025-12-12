using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {
        get {
            if(GameManager._instance == null) {
                GameManager._instance = new GameManager();
            }
            return GameManager._instance;
        }
    }
    private static GameManager _instance;
    
    void Awake() {
        if(GameManager._instance != null && GameManager._instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            GameManager._instance = this;
        }
    }

    public static void ResetLevel() {
        IResettable script;
        foreach(MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if((script = mb as IResettable) != null) {
                script.OnReset();
            }
        }
    }

    public static void PauseGame() {
        foreach(ZombieMono zb in FindObjectsByType<ZombieMono>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            zb.StopBehaviour();
        }
    }
}
