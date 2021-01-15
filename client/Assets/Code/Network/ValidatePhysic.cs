using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ValidatePhysic : MonoBehaviour{

    public GameObject player;
    public MovementController movementController;


    private static short size = 1024;
    private Vector2[] bufferPosition;
    private Input[] bufferInputs;

    private void Start() {
        bufferPosition = new Vector2[size];
        bufferInputs = new Input[size];
    }

    //----------------------------------------------------------------------------

    /**
     * Guardar entradas ejecutadas para cada tick
     */
    public void saveInputsBuffer(int tick, byte displacement, bool jump) {
        Input input;
        input.tick = tick;
        input.displacement = displacement;
        input.jump = jump;
        bufferInputs[getIndex(tick)] = input;
    }

    //----------------------------------------------------------------------------

    /**
     * Obtener entradas ejecutadas para un determinado tick
     */
    public Input readInputsBuffer(int tick) {
       return bufferInputs[getIndex(tick)];
    }



    //----------------------------------------------------------------------------

    /**
     * Guardar posicion ejecutada en local
     */
    public void savePositionBuffer(int tick, Vector2 position) {
        bufferPosition[getIndex(tick)] = position;
    }

    //----------------------------------------------------------------------------

    /**
     * Comprobar que la posicion ejecutada en local coincide con la del servidor
     */
    public void validate(int tick, Vector2 targetPosition) {
        //Las posiciones en servidor y cliente no coinciden
        Debug.Log("recibido:" + tick + " real:" + movementController.getTick());
        if (bufferPosition[getIndex(tick)] != targetPosition) {
            //Rebobinar el cliente con la posicion real (servidor)
            UnityMainThreadDispatcher.Instance().Enqueue(() => player.transform.position = targetPosition);
            Debug.Log("rebobinado para el tick "+tick+" se esperaba:" + targetPosition.y + " pero en local se ha ejecutado " + bufferPosition[getIndex(tick)].y);
        }
    }

    //----------------------------------------------------------------------------

    /**
     * A partir de un numero de tick devuelve el indice del buffer correspondiente
     * tick 1024 = indice 1024 | tick 1025 = indice 0
     */
    public static int getIndex(int tick) {
        return tick % size;
    }
}