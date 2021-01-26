using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aplicar correcciones en la posicion del jugador principal e interpolar 
/// entre los movimientos recibidos para cada cliente
/// </summary>
public class CorrectionsPhysic : MonoBehaviour {

    //// REF PUB
    public DebugInfoScreen debugInfoScreen;
    public GameObject player;
    public MovementController movementController;
    public GameObject prefabRivalPlayer;

    //// VAR PRI
    private Vector2[] bufferPosition;
    private List<RivalPlayer> rivalsList = new List<RivalPlayer>();
    private Vector2 diff;

    //VAR STA
    private static readonly short size = 1024;

    //------------------------------------------------------------->

    /// <summary>
    /// Inicializacion
    /// </summary>
    private void Start() {
        Application.targetFrameRate = 60; //Fijar FPS a 60
        bufferPosition = new Vector2[size];
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Guardar una posicion asociada a un numero de tick
    /// </summary>
    /// <param name="tick"></param>
    /// <param name="position"></param>
    public void savePositionBuffer(int tick, Vector2 position) {
        bufferPosition[TickToIndex(tick)] = position;
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Procesar recepcion del estado (posicion) asociado a un rival
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    public void ProcessStatusRival(int id, Vector2 position) {
        //Si el rival esta instanciado
        bool instantiated = false;
        for (int i = 0; i < rivalsList.Count; i++) {
            if (rivalsList[i].GetId() == id) {
                instantiated = true;
                if (rivalsList[i].GetGameObject() != null) {
                    //Almacenar la posicion recibida
                    rivalsList[i].SetTargetPosition(position);
                    rivalsList[i].SetTime(0);
                }
            }
        }

        //Si el rival No esta instanciado
        if (!instantiated) {
            RivalPlayer rivalPlayer = new RivalPlayer(id);
            rivalsList.Add(rivalPlayer);
            //Instanciar
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                rivalPlayer.SetGameObject(Instantiate(prefabRivalPlayer, position, Quaternion.identity))
            );
        }

    }

    //------------------------------------------------------------->

    /// <summary>
    /// Ejecucion en cada fotograma
    /// </summary>
    private void Update() {

        //Recorrer todos los rivales
        for (int i = 0; i < rivalsList.Count; i++) {
            RivalPlayer rival = rivalsList[i];
            if (rival.GetGameObject() != null) {
                //Interpolar entre la posicion actual y la almacenada como "destino/objetivo"
                rival.SetTime(rival.GetTime() + Time.deltaTime / Time.fixedDeltaTime);
                rival.GetGameObject().transform.position = Vector3.Lerp(rival.GetGameObject().transform.position, rival.GetTargetPosition(), rival.GetTime());
            }
            
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Aplicar una corrección al personaje (local) del jugador si es necesario
    /// </summary>
    /// <param name="tick"></param>
    /// <param name="serverPosition"></param>
    public void CorrectPlayer(int tick, Vector2 serverPosition){
        //Distancia de diferencia entre el jugador local y su representacion en servidor
        float diffX = bufferPosition[TickToIndex(tick)].x - serverPosition.x;
        float diffY = bufferPosition[TickToIndex(tick)].y - serverPosition.y;
        diff = new Vector2(diffX, diffY);
        
        //Corregir la posicion
        if (Mathf.Abs(diffX) > 2 || Mathf.Abs(diffY) > 3 ) {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                player.transform.position = serverPosition
            );
        }

        //Enviar info para la interfaz grafica
        debugInfoScreen.SetDiff(new Vector2(diffX, diffY));
    }

    //------------------------------------------------------------->

    /// <summary>
    ///  A partir de un numero de tick devuelve el indice del buffer correspondiente 
    ///  tick 1024 = indice 1024 | tick 1025 = indice 0
    /// </summary>
    /// <param name="tick"></param>
    /// <returns></returns>
    public static int TickToIndex(int tick) {
        return tick % size;
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public Vector2 GetDiff() {
        return diff;
    }
}