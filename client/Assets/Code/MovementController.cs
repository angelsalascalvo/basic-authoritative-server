using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovementController : MonoBehaviour{
    
    private struct Input {
        public int tick;
        public byte direction;
    }

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
                
                Debug.Log("=============== TICK " + tick + " ===============");
                Debug.Log("ultimo tick del servidor:" + connectServer.getLastTickServer());

                //Agregar el listado de ticks sin recibir por parte del servidor
                List<Input> listInput = createListInputToServer();

                bw.Write((int)listInput.Count);
                Debug.Log("Input concatenados" + listInput.Count);
                foreach (Input input in listInput) {
                   
                    bw.Write(input.tick);
                    bw.Write(input.direction);
                    Debug.Log("agregado tick" + input.tick + " direccion " + input.direction);
                }


                Debug.Log("==========================================\n");


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
     * Comprueba cuales son los tick no recibidos por el servidor y elabora un listado con los datos de los mismos
     */
    private List<Input> createListInputToServer(){
        List<Input> listInput = new List<Input>();
        //Comprobar cuales son los tick aun no recibidos por el servidor para volver a enviarselo 
        //por si se ha producido alguna perdida de alguno de ellos.
        for (int i = connectServer.getLastTickServer(); i <= tick; i++) {
            Input input;
            input.tick = i;
            input.direction = validatePhysic.readInputsBuffer(i);
            listInput.Add(input);
        }
        return listInput;
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

