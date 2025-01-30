using UnityEngine;

public class QuitGameHandler : MonoBehaviour
{
    public GameObject exitButton;
    private DronesUIExpandible _dronesUIExpandible;

    private void Start()
    {
        _dronesUIExpandible = exitButton.GetComponent<DronesUIExpandible>();
    }

    public void QuitGame()
    {
        Debug.Log("Uscita dal gioco.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ClosePanel()
    {
        _dronesUIExpandible.SetPanelVisibility(false);
    }
}
