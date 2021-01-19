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
    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;

    //// VAR PRI
    private Rigidbody2D rb;
    private EnumDisplacement displacement = EnumDisplacement.None;
    private bool jump = false;

    private float timer;
    private int tick = 0;
    private short limitsInputTicks = 80;

    //---------------------------------------------------------------

    /**
     * INICIALIZACION
     */
    void Start(){
        rb = GetComponent<Rigidbody2D>();
        Debug.Log(Time.fixedDeltaTime);
    }

    //---------------------------------------------------------------

    void Update() {
        timer += Time.deltaTime;

        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;
            // Debug.Log("recibido:" + tick + " real:" + connectServer.getLastTickServer() + "desfase de: " + (tick - connectServer.getLastTickServer()));

            //int gap = (tick - connectServer.getLastTickServer())-3;
            //&& !StaticMethods.percent((short)Mathf.Clamp(gap,0,100))



            if (connectServer.isConnected() ){
                byte[] data = new byte[508];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(data));

                bw.Write((byte)2); //ActionCode 2.
                validatePhysic.saveInputTicksBuffer(tick, (byte)displacement, jump);
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
                    rb.AddForce(Vector2.up * 30f);
                }

                //                Debug.Log("=============== TICK " + tick + " ===============");
                //                Debug.Log("ultimo tick del servidor:" + connectServer.getLastTickServer());

                //Agregar el listado de ticks sin recibir por parte del servidor
                List<InputTick> listInputTick = createListInputTickToServer();

                short countInputTicks = (short) Mathf.Min((short)listInputTick.Count, limitsInputTicks); //Obtener el numero de inputs a enviar con un maximo de {limitsInputTicks}
                bw.Write(countInputTicks); //
//              Debug.Log("InputTick concatenados" + countInputTicks);
                //Obtener los n ultimos resultamos del listado para evitar desbordar el array de bytes del datagrama 
                //con una cantidad demasiado grande de inputs
                for (int i = Mathf.Max(0, listInputTick.Count - limitsInputTicks); i < listInputTick.Count; ++i) {
                    bw.Write(listInputTick[i].tick);
                    bw.Write(listInputTick[i].displacement);
                    bw.Write(listInputTick[i].jump);
//                    Debug.Log("agregado tick" + listInputTick[i].tick + " direccion " + listInputTick[i].direction);
                }


                Physics2D.Simulate(Time.fixedDeltaTime); //¿Es realmente necesario simular fisica en cliente? Si no se simula podemos migrar código a Fixed update

                //🎲 Simulacion perdida datagramas: Probabilidad de que el datagrama se pierda
                if (StaticMethods.percent((short)sliderLostDatagram.value)) {
                    Debug.LogWarning("Paquete perdido");
                } else {
                    //🎲 Simulacion de latencia
                    StartCoroutine(sendToServerSimulateLatency(data));
                }

                validatePhysic.savePositionBuffer(tick, transform.position);
                tick++;
            }
        }

        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
        // 🎲 Simulacion perdida de datagramas
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
    }

    //---------------------------------------------------------------

    /**
     * Comprueba cuales son los tick no recibidos por el servidor y elabora un listado con los datos de los mismos
     */
    private List<InputTick> createListInputTickToServer(){
        List<InputTick> listInputTick = new List<InputTick>();
        //Comprobar cuales son los tick aun no recibidos por el servidor para volver a enviarselo 
        //por si se ha producido alguna perdida de alguno de ellos.
        for (int i = connectServer.getLastTickServer(); i<=tick; i++) {
            InputTick input;
            input.tick = i;
            input.displacement = validatePhysic.readInputTicksBuffer(i).displacement;
            input.jump = validatePhysic.readInputTicksBuffer(i).jump;
            listInputTick.Add(input);
        }
        return listInputTick;
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


    public void correctionPosition(Vector2 serverPosition) {
        float grap = (rb.position - serverPosition).sqrMagnitude;
        if (grap > 6) {
            rb.position = serverPosition;
        }
        Debug.Log("current " + rb.position.x + " server " + serverPosition.x + " Magnitud:" + grap);

    }

    //---------------------------------------------------------------

    public void setMovement(EnumDisplacement displacement) {
        this.displacement = displacement;
    }

    public void setJump(bool jump) {
        this.jump = jump;
    }

    public int getTick() {
        return tick;
    }

    public Vector2 getPosition() {
        return rb.position;
    }

}