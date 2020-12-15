using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ValidatePhysic : MonoBehaviour{

    private static short size = 1024;
    public GameObject player;
    Vector2[] bufferPosition;
    byte[] bufferInputs;

    private void Start() {
        bufferPosition = new Vector2[size];
        bufferInputs = new byte[size];
    }

    //----------------------------------------------------------------------------

    /**
     * Guardar entradas ejecutadas para cada tick
     */
    public void saveInputsBuffer(int tick, byte input) {
        bufferInputs[getIndex(tick)] = input;
    }

    //----------------------------------------------------------------------------

    /**
     * Obtener entradas ejecutadas para un determinado tick
     */
    public byte readInputsBuffer(int tick) {
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
        if (bufferPosition[getIndex(tick)] != targetPosition) {
            //Rebobinar el cliente con la posicion real (servidor)
            UnityMainThreadDispatcher.Instance().Enqueue(() => player.transform.position = targetPosition);
            Debug.Log("rebobinado para el tick "+tick+" se esperaba:" + targetPosition.x + " pero en local se ha ejecutado " + bufferPosition[getIndex(tick)].x);
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