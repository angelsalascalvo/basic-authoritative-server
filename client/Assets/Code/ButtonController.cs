using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

/**
 * SCRIPT GENERICO EFECTUAR UN MOVIMIENTO AL PULSAR UN ELEMENTO O BOTON TACTIL
 */
public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    //// REF PUB
    public MovementController movementScript;
    public bool jump = false;
    public EnumDisplacement displacement;

    /**
     * Al pulsar el elemento
     */
    public void OnPointerDown(PointerEventData eventData) {
        if (jump) {
            movementScript.setJump(true);
        } else {
            movementScript.setMovement(displacement);
        }
    }

    /**
     * Al soltar el elemento
     */
    public void OnPointerUp(PointerEventData eventData) {
        if (jump) {
            movementScript.setJump(false);
        } else {
            movementScript.setMovement(EnumDisplacement.None);
        }
    }
}