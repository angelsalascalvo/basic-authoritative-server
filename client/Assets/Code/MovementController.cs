using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovementController : MonoBehaviour{

    //// REF PUB
    public ConnectServer connectServer;
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

            if (connectServer.isConnected()) {
                byte[] data = new byte[508];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(data));

                Debug.Log("=============== TICK " + tick+" ===============");

                bw.Write((byte)2); //ActionCode 2.

                switch (direction) {
                    case EnumDirection.Left:
                        data[1] = 1;
                        validatePhysic.saveInputsBuffer(tick, 1);
                        rb.velocity = new Vector2(-3f, rb.velocity.y);
                        break;
                    case EnumDirection.Right:
                        validatePhysic.saveInputsBuffer(tick, 2);
                        rb.velocity = new Vector2(3f, rb.velocity.y);
                        break;
                    case EnumDirection.None:
                        validatePhysic.saveInputsBuffer(tick, 0);
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        break;
                }

                //Comprobar cuales son los tick aun no recibidos por el servidor para volver a enviarselo 
                //por si se ha producido alguna perdida de alguno de ellos.
                Debug.Log("ultimo tick del servidor:" + connectServer.getLastTickServer());
                for (int i=connectServer.getLastTickServer()+1; i <= tick; i++) {
                    bw.Write(i);
                    bw.Write(validatePhysic.readInputsBuffer(i));
                    Debug.Log("agregado tick" + i + " input " + validatePhysic.readInputsBuffer(i));
                }
                bw.Write((byte)255); //Marca final para detectar cuando se acaba el listado de inputs
                Debug.Log("enviar paquete del tick "+tick);

           
                Physics2D.Simulate(Time.fixedDeltaTime);
                //conectServer.sendDataToServer(data); //Sin simular latencia
                StartCoroutine(sendToServerSimulateLatency(data));//🎲 Simulacion de latencia
                validatePhysic.savePositionBuffer(tick, transform.position);
                Debug.Log("==========================================\n");
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
        connectServer.sendDataToServer(data);
    }


    //---------------------------------------------------------------

    public void setMovement(EnumDirection direction) {
        this.direction = direction;
    }
}

