using UnityEngine;
using UnityEngine.EventSystems;

public class DronesUIExpandible : MonoBehaviour, IPointerClickHandler
{
    public GameObject panel;
    private bool _isPanelVisible;

    private void Start()
    {
        SetPanelVisibility(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetPanelVisibility(!_isPanelVisible);
    }

    // Metodo per impostare la visibilità del Panel e dei suoi figli
    public void SetPanelVisibility(bool isVisible)
    {
        panel.SetActive(isVisible); // Cambia lo stato attivo del Panel (e di tutti i suoi figli)
        _isPanelVisible = isVisible; // Aggiorna lo stato del Panel
    }
}