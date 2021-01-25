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


    private static int limitTickQueueSize = 5;
    private Queue<InputTick> tickQueue;

    //---------------------------------------------------------------

    /**
     * INICIALIZACION
     */
    void Start(){
        Time.fixedDeltaTime = 0.02f;
        rb = GetComponent<Rigidbody2D>();
        tickQueue = new Queue<InputTick>();
        Debug.Log(Time.fixedDeltaTime);
    }

    //---------------------------------------------------------------

    void Update() {

        if (Input.GetKey("z")) {
            displacement = EnumDisplacement.Left;
        } else if (Input.GetKey("x")) {
            displacement = EnumDisplacement.Right;
        } else {
            displacement = EnumDisplacement.None;
        }
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
                        //Variamos la velocidad para corregir distancia de diferencia entre cliente y servidor
                        if (validatePhysic.diff > 0.02f) {
                            Debug.Log("CORRER MAS <-");
                            rb.velocity = new Vector2(-3.5f, rb.velocity.y);
                        } else {
                            rb.velocity = new Vector2(-3f, rb.velocity.y);
                        }
                        break;
                    case EnumDisplacement.Right:
                        if (validatePhysic.diff < -0.02f) {
                            Debug.Log("CORRER MAS ->");
                            rb.velocity = new Vector2(3.5f, rb.velocity.y);
                        } else {
                            rb.velocity = new Vector2(3f, rb.velocity.y);
                        }
                        break;
                    case EnumDisplacement.None:
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        break;
                }

                if (jump) {
                    rb.velocity = new Vector2(rb.velocity.x, 5f);
                }

                InputTick inputTick;
                inputTick.tick = tick;
                inputTick.displacement = displacement;
                inputTick.jump = jump;
                AddInputTick(inputTick);
              
                //Escribir los datos a enviar
                bw.Write((short)tickQueue.Count);
                foreach (InputTick it in tickQueue) {
                    bw.Write(it.tick);
                    bw.Write((byte)it.displacement);
                    bw.Write(it.jump);
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

    public void AddInputTick(InputTick inputTick) {
        //Controlar tamanno maximo de la cola
        if (tickQueue.Count > limitTickQueueSize)
            tickQueue.Dequeue();

        tickQueue.Enqueue(inputTick);
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