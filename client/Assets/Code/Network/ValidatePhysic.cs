using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ValidatePhysic : MonoBehaviour{

    private static short size = 1024;
    public int lastTickExecutedServer = 0;
    public GameObject player;
    Vector2[] bufferPosition;

    private void Start() {
        bufferPosition = new Vector2[size];
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
            Debug.Log("rebobina se esperaba:"+targetPosition.x+" ejecutado "+bufferPosition[getIndex(tick)].x);
            player.transform.position = targetPosition;
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