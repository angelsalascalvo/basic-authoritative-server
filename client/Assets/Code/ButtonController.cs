using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

/// <summary>
/// Script generico para configurar botones de movimiento tactiles
/// </summary>
public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    //// REF PUB
    public MovementController movementScript;
    
    //// VAR PUB
    public bool jump = false; //¿Es el boton de salto?
    public EnumDisplacement displacement;

    //------------------------------------------------------------->

    /// <summary>
    /// Al pulsar el objeto llamar al metodo correspondiente segun configuracion
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData) {
        if (jump) {
            movementScript.setJump(true);
        } else {
            movementScript.setMovement(displacement);
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Al soltar el objeto llamar al metodo correspondiente segun configuracion
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData) {
        if (jump) {
            movementScript.setJump(false);
        } else {
            movementScript.setMovement(EnumDisplacement.None);
        }
    }
}