using UnityEngine;
using UnityEngine.EventSystems;

public class DronesUIExpandible : MonoBehaviour, IPointerClickHandler
{
    public GameObject panel;
    private bool _isPanelVisible = false;

    void Start()
    {
        SetPanelVisibility(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isPanelVisible)
        {
            SetPanelVisibility(false);
        }
        else
        {
            SetPanelVisibility(true);
        }
    }

    // Metodo per impostare la visibilità del Panel e dei suoi figli
    public void SetPanelVisibility(bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible); // Cambia lo stato attivo del Panel (e di tutti i suoi figli)
            _isPanelVisible = isVisible; // Aggiorna lo stato del Panel
        }
    }
}
