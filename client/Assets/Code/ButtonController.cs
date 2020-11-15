using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 1

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    //// REF PUB
    public MovementController movementScript;
    public EnumDirection direction;

    public void OnPointerDown(PointerEventData eventData) {
        movementScript.setMovement(direction);

    }

    public void OnPointerUp(PointerEventData eventData) {
        Debug.Log("solta");
        movementScript.setMovement(EnumDirection.None);
    }
}