using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public GameObject panel;
    private bool isPanelVisible = false;

    void Start()
    {
        SetPanelVisibility(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPanelVisible)
        {
            SetPanelVisibility(false);
        }
        else
        {
            SetPanelVisibility(true);
        }
    }

    // Metodo per impostare la visibilità del Panel e dei suoi figli
    void SetPanelVisibility(bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible); // Cambia lo stato attivo del Panel (e di tutti i suoi figli)
            isPanelVisible = isVisible; // Aggiorna lo stato del Panel
        }
    }
}
