using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ClientsPhysic : MonoBehaviour {
    public ServerMain serverMain;
    public GameObject prefabPlayer;

    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;

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
        // 🎲 Simulacion perdida de datagramas
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";


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
            /*
            for (int i = 0; i < clientList.Count; i++) {

                if (clientList[i].GetLastTickExecuted() != -1) {
                    Debug.Log("Enviado " + clientList[i].GetLastTickExecuted());
                    //🎲 Simulacion perdida datagramas: Probabilidad de que el datagrama se pierda
                    if (StaticMethods.percent((short)sliderLostDatagram.value)) {
                        Debug.LogWarning("Paquete perdido");
                    } else {
                        //🎲 Simulacion de latencia
                        StartCoroutine(sendToClientSimulateLatency(clientList[i].GetAddress(), clientList[i].GetLastTickExecuted(), clientList[i].GetGameObject().transform.position));
                    }
                }
            }
            */
        }


        

        

    }



    public void move(Client client, int tick, byte displacement, bool jump) {

        //🎲 Simulacion perdida datagramas: Probabilidad de que el datagrama se pierda
        if (StaticMethods.percent((short)sliderLostDatagram.value)) {
            Debug.LogWarning("Paquete perdido");
        } else {
            //🎲 Simulacion de latencia
            StartCoroutine(sendToClientSimulateLatency(client.GetAddress(), tick, client.GetGameObject().transform.position));
        }

    }


    //---------------------------------------------------------------

    /**
     * 🎲 Simulacion de latencia
     * Corrutina que espera X segundos antes de enviar los datos al servidor 
     */
    IEnumerator sendToClientSimulateLatency(IPEndPoint clientAddress, int tickExecuted, Vector2 position) {
        yield return new WaitForSeconds(sliderLatency.value / 1000);
        serverMain.sendStatusToClient(clientAddress, tickExecuted, position);
    }

}