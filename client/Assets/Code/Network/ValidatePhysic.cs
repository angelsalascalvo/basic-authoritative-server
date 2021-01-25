using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ValidatePhysic : MonoBehaviour {

    public Text diffCount;

    private Rigidbody2D rb2D;
    public GameObject player;
    public MovementController movementController;
    public GameObject prefabRivalPlayer;

    private static short size = 1024;
    private Vector2[] bufferPosition;
    private InputTick[] bufferInputTicks;

    private short correction = 0;


    private List<RivalPlayer> rivalsList = new List<RivalPlayer>();
    private int myID = -1;

    public float diff;

    private float t = 0, target;
    private void Start() {
        rb2D = player.GetComponent<Rigidbody2D>();
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


    


    public void ProcessStatusRival(int id, Vector2 position, Vector2 velocity) {
        //Comprobar si esta instanciado
        bool instantiated = false;
        for (int i = 0; i < rivalsList.Count; i++) {
            if (rivalsList[i].GetId() == id) {
                instantiated = true;

                GameObject gameObject = rivalsList[i].GetGameObject();
                Rigidbody2D rb = rivalsList[i].GetRigidbody2D();
                RivalPlayer rival = rivalsList[i];
                if (gameObject != null) {

                       

                    if (rivalsList[i].GetPositionsQueue().Count > 5)
                        rivalsList[i].GetPositionsQueue().Dequeue();


                    rivalsList[i].GetPositionsQueue().Enqueue(position);
                    Debug.Log(rivalsList[i].GetPositionsQueue().Count);

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


    public void NewMovementRival(RivalPlayer rival, Vector2 position) {
        rival.SetStartPosition(rival.GetGameObject().transform.position);
        rival.SetTargetPosition(position);
        rival.SetTime(0);
    }


    private void Update() {
        for (int i = 0; i < rivalsList.Count; i++) {
            RivalPlayer rival = rivalsList[i];

            if (rival.GetPositionsQueue().Count > 1) {
                if ((Vector2)rival.GetGameObject().transform.position == rival.GetTargetPosition() || rival.GetTime()==-1) {
                    rival.SetStartPosition(rival.GetGameObject().transform.position);
                    Vector2 target = rival.GetPositionsQueue().Dequeue();
                    rival.SetTargetPosition(target);
                    rival.SetTime(0);

                }
                
                rival.SetTime(rival.GetTime() + Time.deltaTime / 0.04f);
                rival.GetGameObject().transform.position = Vector3.Lerp(rival.GetStartPosition(), rival.GetTargetPosition(), rival.GetTime());
            }
        }
    }

    public void CorrectPlayer(int tick, Vector2 serverPosition){
        //Debug.Log(bufferPosition[getIndex(tick)].x + " : " + serverPosition.x +" |=> "+ Mathf.Abs(bufferPosition[getIndex(tick)].x - serverPosition.x));
        diff = (bufferPosition[getIndex(tick)].x - serverPosition.x);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
            diffCount.text =  "Diferencia: x => "+ (bufferPosition[getIndex(tick)].x - serverPosition.x)
        );

        //Corrección grande
        if (Mathf.Abs(bufferPosition[getIndex(tick)].x - serverPosition.x) > 4 || (Mathf.Abs(bufferPosition[getIndex(tick)].y - serverPosition.y) > 3 )) {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                player.transform.position = serverPosition
            );
        }
        

    }


    public void CorrectRivalPlayer(GameObject gameObject, Vector2 serverPosition) {

        Vector2 diff = (Vector2)gameObject.transform.position - serverPosition;

        Debug.Log("local:" + (Vector2)gameObject.transform.position + " server: " + serverPosition + " (" + diff + ")");

      // if ((Mathf.Abs(diff.x) > 0.07f || Mathf.Abs(diff.y) > 0.07f)) {
        //    Debug.Log("correccion");
            gameObject.transform.position = serverPosition;
        //}
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