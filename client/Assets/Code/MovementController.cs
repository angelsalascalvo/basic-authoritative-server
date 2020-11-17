using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovementController : MonoBehaviour{

    //// REF PUB
    public ConnectServer conectServer;
    public ValidatePhysic validatePhysic;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;


    //// VAR PRI
    private Rigidbody2D rb;
    private EnumDirection direction = EnumDirection.None;

    private float timer;
    private int tick = 0;

    //---------------------------------------------------------------

    /**
     * INICIALIZACION
     */
    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    //---------------------------------------------------------------

    void Update() {
        timer += Time.deltaTime;

        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;

            byte[] data = new byte[508];
            data[0] = 2; //ActionCode 2.

            switch (direction) {
                case EnumDirection.Left:
                    data[1] = 1;
                    rb.velocity = new Vector2(-3f, rb.velocity.y);
                    break;
                case EnumDirection.Right:
                    data[1] = 2;
                    rb.velocity = new Vector2(3f, rb.velocity.y);
                    break;
                case EnumDirection.None:
                    data[1] = 0;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    break;
            }

            if (conectServer.isConnected()) {
                Physics2D.Simulate(Time.fixedDeltaTime);
                //conectServer.sendDataToServer(data); //Sin simular latencia
                StartCoroutine(sendToServerSimulateLatency(data));//🎲 Simulacion de latencia
                validatePhysic.savePositionBuffer(tick, transform.position);
                tick++;
            }
        }

        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
    }

    //---------------------------------------------------------------

    /**
     * 🎲 Simulacion de latencia
     * Corrutina que espera X segundos antes de enviar los datos al servidor 
     */
    IEnumerator sendToServerSimulateLatency(byte[] data) {
        yield return new WaitForSeconds(sliderLatency.value/1000);
        conectServer.sendDataToServer(data);
    }


    //---------------------------------------------------------------

    public void setMovement(EnumDirection direction) {
        this.direction = direction;
    }
}

