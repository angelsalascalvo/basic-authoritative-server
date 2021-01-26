using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;

/// <summary>
/// Ejecutar la fisica y procesar los inputs recibidos de los clientes
/// </summary>
public class ClientsPhysic : MonoBehaviour {

    //// REF PUB
    public ServerMain serverMain;
    public GameObject prefabPlayer;

    //// VAR PRIV
    private float timer;

    //------------------------------------------------------------->

    /// <summary>
    /// Inicializacion
    /// </summary>
    private void Start() {
        Time.fixedDeltaTime = 0.02f; //Tickrate (1s / 0.02s = 50 ticks por segundo)
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Actualizacion cada fotograma
    /// </summary>
    private void Update() {

        List<Client> clientList = serverMain.GetClientList();

        //Ejecucion cada tick (tickrate)
        timer += Time.deltaTime;
        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;


            for (int i = 0; i < clientList.Count; i++) {
                if (clientList[i].GetGameObject() != null) {
                    EnumDisplacement displacement;
                    bool jump;

                    //Obtener nuevo tick con input del cliente
                    if (clientList[i].GetTickQueue().Count > 0) {
                        InputTick inputTick = clientList[i].GetTickQueue().Dequeue();
                        clientList[i].SetLastTickExecuted(inputTick);
                        displacement = inputTick.displacement;
                        jump = inputTick.jump;

                    //Si no hay un input almacenado en la cola, repetimos ultima entrada (estimacion)
                    } else {
                        jump = clientList[i].GetLastTickExecuted().jump;
                        displacement = clientList[i].GetLastTickExecuted().displacement;

                    }

                    //Establecer velocidades
                    Rigidbody2D rb = clientList[i].GetGameObject().GetComponent<Rigidbody2D>();
                    switch (displacement) {
                        case EnumDisplacement.Left:
                            rb.velocity = new Vector2(-3f, rb.velocity.y);
                            break;
                        case EnumDisplacement.Right:
                            rb.velocity = new Vector2(3f, rb.velocity.y);
                            break;
                        case EnumDisplacement.None:
                            rb.velocity = new Vector2(0, rb.velocity.y);
                            break;
                    }
                    if (jump) {
                        rb.velocity = new Vector2(rb.velocity.x, 5f);
                    }
                }
            }

            //Ejecutar la fisica para todos los clientes
            Physics2D.Simulate(Time.fixedDeltaTime);

            //Enviar el estado de los jugadores a los clientes
            SendStatus(clientList);
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Enviar estado (posicion) de todos los jugadores a cada cliente
    /// </summary>
    /// <param name="clientList"></param>
    public void SendStatus(List<Client> clientList) {

        byte[] dataAux = new byte[508];
        BinaryWriter bw = new BinaryWriter(new MemoryStream(dataAux));
        byte length = 0;

        //Recorrer todos los jugadores
        for (int i = 0; i < clientList.Count; i++) {
            GameObject gameObject = clientList[i].GetGameObject();
            if (clientList[i].GetLastTickExecuted().tick>-1) {
                length++;
                //Almacenar datos en array auxiliar
                bw.Write(clientList[i].GetId()); // 4 bytes
                bw.Write(clientList[i].GetLastTickExecuted().tick); //4 bytes
                bw.Write(gameObject.transform.position.x); //4 bytes
                bw.Write(gameObject.transform.position.y); //4 bytes
            }
        }
        bw.Close();

        //Crear el datagrama
        byte[] dataSend = new byte[508];
        bw = new BinaryWriter(new MemoryStream(dataSend));

        bw.Write((byte)3); //ActionCode 3
        bw.Write(length); //Cantidad de jugadores de los que se envia informacion
        //Volcar array auxiliar en array de datagrama
        bw.Write(dataAux, 0, length * 16); //16 bytes por usuario
        bw.Close();

        //Enviar a todos los clientes
        for (int i = 0; i < clientList.Count; i++) {
            serverMain.SendToClientSimulate(dataSend, clientList[i].GetAddress());
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Instanciar un nuevo jugador en el escenario
    /// </summary>
    /// <param name="client"> Cliente para el que instanciar</param>
    /// <param name="position">Posicion donde instanciar</param>
    public void Instantiate(Client client, Vector2 position) {
        client.SetGameObject(Instantiate(prefabPlayer, position, Quaternion.identity));
    }
}