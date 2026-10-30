using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Hover and keyboard focus reveal the same details; clicking never reapplies a buff.
public class PauseBuffListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Action Reveal;
    public Action Clear;
    public void OnPointerEnter(PointerEventData eventData) => Reveal?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => Clear?.Invoke();
    public void OnSelect(BaseEventData eventData) => Reveal?.Invoke();
    public void OnDeselect(BaseEventData eventData) => Clear?.Invoke();
}
