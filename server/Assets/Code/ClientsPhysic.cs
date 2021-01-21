using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ClientsPhysic : MonoBehaviour {
    public ServerMain serverMain;
    public GameObject prefabPlayer;

    

    private float timer;

    private bool physicExecute=false;

    //------------------------------------------------------------------------------------------------

    /// <summary>
    /// INSTANCIAR UN NUEVO JUGADOR EL ESCENARIO
    /// </summary>
    /// <param name="client"></param>
    /// <param name="position"></param>
    public void instantiate(Client client, Vector2 position) {
        client.SetGameObject(Instantiate(prefabPlayer, position, Quaternion.identity));
    }


    private void Start() {
        Time.fixedDeltaTime = (0.02f);
        Debug.Log(Time.fixedDeltaTime);
    }

    //------------------------------------------------------------------------------------------------

    private void Update() {



        //FISICA
        List<Client> clientList = serverMain.GetClientList();

        timer += Time.deltaTime;
        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;


            for (int i = 0; i < clientList.Count; i++) {
                if (clientList[i].GetGameObject() != null) {
                    Rigidbody2D rb = clientList[i].GetGameObject().GetComponent<Rigidbody2D>();

                    if (clientList[i].GetTickQueue().Count > 0) {

                        InputTick inputTick = clientList[i].GetTickQueue().Dequeue();

                        switch (inputTick.displacement) {
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

                        if (inputTick.jump) {
                            rb.velocity = new Vector2(rb.velocity.x, 5f);
                        }

                        clientList[i].SetLastTickExecuted(inputTick.tick);

                    } else {
                        rb.velocity = new Vector2(0, 0);
                    }
                }
            }

            Physics2D.Simulate(Time.fixedDeltaTime);


            //Enviar estado
            SendStatus(clientList);
        }
    }


    public void SendStatus(List<Client> clientList) {

        byte[] dataAux = new byte[508];
        BinaryWriter bw = new BinaryWriter(new MemoryStream(dataAux));

        byte length = 0;
        for (int i = 0; i < clientList.Count; i++) {
            GameObject gameObject = clientList[i].GetGameObject();
            if (clientList[i].GetGameObject() != null) {
                length++;
                //Datos
                bw.Write(clientList[i].GetId()); // 4 bytes
                bw.Write(gameObject.transform.position.x); //4 bytes
                bw.Write(gameObject.transform.position.y); //4 bytes
            }
        }
        bw.Close();


        byte[] dataSend = new byte[508];
        bw = new BinaryWriter(new MemoryStream(dataSend));
        bw.Write((byte)3); //ActionCode 3

        bw.Write(length);
        //Volcar las posiciones en el array de datos
        bw.Write(dataAux, 0, length * 12);
        bw.Close();


        //Enviar a todos los clientes
        for (int i = 0; i < clientList.Count; i++) {
            serverMain.SendToClientSimulate(dataSend, clientList[i].GetAddress());
        }

    }
    
}