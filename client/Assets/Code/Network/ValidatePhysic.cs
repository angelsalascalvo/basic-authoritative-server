using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ValidatePhysic : MonoBehaviour{

    public GameObject player;
    public MovementController movementController;
    public GameObject prefabRivalPlayer;

    private static short size = 1024;
    private Vector2[] bufferPosition;
    private InputTick[] bufferInputTicks;

    private short correction = 0;


    private List<RivalPlayer> rivalsList = new List<RivalPlayer>();
    private int myID=-1;

    private void Start() {
        bufferPosition = new Vector2[size];
        bufferInputTicks = new InputTick[size];
    }

    //----------------------------------------------------------------------------

    /**
     * Guardar entradas ejecutadas para cada tick
     */
    public void saveInputTicksBuffer(int tick, byte displacement, bool jump) {
        InputTick input;
        input.tick = tick;
        input.displacement = (EnumDisplacement)Enum.ToObject(typeof(EnumDisplacement), displacement);
        input.jump = jump;
        bufferInputTicks[getIndex(tick)] = input;
    }

    //----------------------------------------------------------------------------

    /**
     * Obtener entradas ejecutadas para un determinado tick
     */
    public InputTick readInputTicksBuffer(int tick) {
       return bufferInputTicks[getIndex(tick)];
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
        correction++;
        if (correction >= 30) {
            correction = 0;
            //Las posiciones en servidor y cliente no coinciden
            if (bufferPosition[getIndex(tick)] != targetPosition) {
                //Rebobinar el cliente con la posicion real (servidor)
                UnityMainThreadDispatcher.Instance().Enqueue(() => player.transform.position = targetPosition);
                Debug.LogWarning("rebobinado para el tick " + tick + " se esperaba:" + targetPosition.y + " pero en local se ha ejecutado " + bufferPosition[getIndex(tick)].y);
            }


            UnityMainThreadDispatcher.Instance().Enqueue(() => movementController.correctionPosition(targetPosition));

               
            /*
            //Las posiciones en servidor y cliente no coinciden
            if (movementController.getPosition() - targetPosition) {
                //Rebobinar el cliente con la posicion real (servidor)
                UnityMainThreadDispatcher.Instance().Enqueue(() => player.transform.position = targetPosition);
                Debug.Log("rebobinado para el tick " + tick + " se esperaba:" + targetPosition.y + " pero en local se ha ejecutado " + bufferPosition[getIndex(tick)].y);
            }
            */
        }
    }


    private void Update() {
        for (int i = 0; i < rivalsList.Count; i++) {
            if (rivalsList[i].GetGameObject() != null) {
                
            }
        }
    }


    public void ProcessStatusRival(int id, Vector2 position, Vector2 velocity) {
        //Comprobar si esta instanciado
        bool instantiated = false;
        for (int i = 0; i < rivalsList.Count; i++) {
            if (rivalsList[i].GetId() == id){
                instantiated = true;
                
                GameObject gameObject = rivalsList[i].GetGameObject();
                Rigidbody2D rb = rivalsList[i].GetRigidbody2D();

                if (gameObject != null) {
                    //Establecer velocidad de movimiento
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        MoveRivalPlayer(rb, velocity)
                    );
                    //Aplicar correcciones de posicion
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        CorrectRivalPlayer(gameObject, rb, position)
                    );
                }
            }
        }

        //No esta instanciado
        if (!instantiated) {
            RivalPlayer rivalPlayer = new RivalPlayer(id);
            rivalsList.Add(rivalPlayer);
            UnityMainThreadDispatcher.Instance().Enqueue(() => 
                rivalPlayer.SetGameObject(Instantiate(prefabRivalPlayer, position, Quaternion.identity))
            );
        }
    
    }


    public void CorrectRivalPlayer(GameObject gameObject, Rigidbody2D rb, Vector2 serverPosition) {

        Vector2 diff = (Vector2)gameObject.transform.position - serverPosition;

        Debug.Log("local:" + (Vector2)gameObject.transform.position + " server: " + serverPosition + " (" + diff + ")");

        if ((Mathf.Abs(diff.x) > 0.07f || Mathf.Abs(diff.y) > 0.07f)) {
            Debug.Log("correccion");
            rb.position = serverPosition;
        }
    }

    public void MoveRivalPlayer(Rigidbody2D rb, Vector2 velocity) {

        //Si no se aplica velocidad en el eje y dejamos caer el cuerpo rigido con la gravedad local para evitar cortes
        if (velocity.y == 0) {
            rb.velocity = new Vector2(velocity.x, rb.velocity.y);
        } else {
            rb.velocity = velocity;
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



    public void SetMyID(int id) { 
        myID = id;
    }
}