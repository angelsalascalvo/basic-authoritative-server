using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovePlayer : MonoBehaviour{
    Rigidbody2D rb;
    public int tickExecuted = -1;
    public ServerMain serverMain;

    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;

    public bool DEL=true;
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // 🎲 Simulacion perdida de datagramas
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
    }

    public void move(int tick, EnumDisplacement direction, bool jump) {
        
        //Comprobar si el tick en cuestion se ha ejecutado en el servidor
        //El envío redundante puede causarse debido a que el cliente cuando envia su mensaje de tick 
        //aun no haya recibido el acuse de recibo de un tick ejecutado en el servidor, o incluso que 
        //este mensaje se haya perdido en la comunicacion
        if (tick > tickExecuted) {
            tickExecuted = tick;
            switch (direction) {
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
                rb.AddForce(Vector2.up * 30f);
            }
            Physics2D.Simulate(Time.fixedDeltaTime);
            Debug.Log("Procesado" + tick);

            //🎲 Simulacion perdida datagramas: Probabilidad de que el datagrama se pierda
            if (StaticMethods.percent((short)sliderLostDatagram.value)) {
                Debug.LogWarning("Paquete perdido");
            } else {
                //🎲 Simulacion de latencia
                StartCoroutine(sendToClientSimulateLatency(tickExecuted, rb.position));
            }
        }
    }


    //---------------------------------------------------------------

    /**
     * 🎲 Simulacion de latencia
     * Corrutina que espera X segundos antes de enviar los datos al servidor 
     */
    IEnumerator sendToClientSimulateLatency(int tickExecuted, Vector2 position) {
        yield return new WaitForSeconds(sliderLatency.value / 1000);
        Debug.Log("enviado " + position.y);
        serverMain.sendStatusToClient(tickExecuted, position);
    }


}