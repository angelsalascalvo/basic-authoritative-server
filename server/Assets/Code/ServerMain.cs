using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
/**
* FUNCIONALIDAD DEL SCRIPT
*/
public class ServerMain : MonoBehaviour{


    public string ipServer;
    public int portServer;
    public MovePlayer movePlayer;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;


    private Socket serverSocket;
    private IPEndPoint serverAddress, clientAddress;
    private Thread thListenClients;

    /**
    * INICIALIZACION
    */
    void Start() {

        //1. Crear Socket
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverAddress = new IPEndPoint(IPAddress.Parse(ipServer), portServer);
        serverSocket.Bind(serverAddress);

        //2. Iniciar hilo de escucha
        thListenClients = new Thread(new ThreadStart(listen));
        thListenClients.Start();
    }
    
    //---------------------------------------------------------------

    private void Update() {
        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
    }


    //---------------------------------------------------------------

    /**
     * METODO PARA ESCUCHAR CONEXIONES DE CLIENTES
     */
    private void listen() {
        Debug.Log("Servidor escuchando...");
        EndPoint clientAddAux = serverAddress; //Inicializar el objeto para evitar errores
        byte[] dataRec, dataSend;

        while (true) {
            dataRec = new byte[508];
            try {
                serverSocket.ReceiveFrom(dataRec, ref clientAddAux);
            } catch (SocketException e) {
                Debug.LogError(e.ErrorCode);

            }
            clientAddress = clientAddAux as IPEndPoint;
            BinaryReader br = new BinaryReader(new MemoryStream(dataRec));

            //Leer ActionCode.
            switch (br.ReadByte()) {
                //Solicitud entrada a partida
                case 1:

                    dataSend = new byte[508];
                    BinaryWriter bw = new BinaryWriter(new MemoryStream(dataSend));
                    bw.Write((byte)2); //ActionCode 2. 
                    bw.Write(true); //Marca de admitido

                    serverSocket.SendTo(dataSend, clientAddress);
                    break;

                //Movimiento del personaje
                case 2:

                    //🎲 Simulacion perdida paquetes: Probabilidad de que el paquete se pierda
                    if (StaticMethods.percent(0)) {
                        Debug.Log("Paquete perdido");
                    } else {

                        switch (br.ReadByte()) {
                            case 1:
                                UnityMainThreadDispatcher.Instance().Enqueue(() => movePlayer.move(EnumDirection.Left));
                                break;
                            case 2:
                                UnityMainThreadDispatcher.Instance().Enqueue(() => movePlayer.move(EnumDirection.Right));
                                break;
                            case 0:
                                UnityMainThreadDispatcher.Instance().Enqueue(() => movePlayer.move(EnumDirection.None));
                                break;
                        }

                        StaticMethods.debugDatagram(dataRec);
                    }

                    break;
            }
        }
    }

    //------------------------------------------------------------------------

    /**
     * METODO ENVIAR MENSAJE AL SERVIDOR
     */
    public void sendStatusToClient(int tick, Vector2 position) {

        byte[] dataSend = new byte[508];
        BinaryWriter bw = new BinaryWriter(new MemoryStream(dataSend));
        bw.Write((byte)3); //ActionCode 3
        //Datos
        bw.Write(tick);
        bw.Write(position.x);
        bw.Write(position.y);

        //Enviar datos
        //serverSocket.SendTo(dataSend, clientAddress); //Sin simular latencia
        StartCoroutine(sendToClientSimulateLatency(dataSend, clientAddress));//🎲 Simulacion de latencia
    }


    //---------------------------------------------------------------

    /**
     * 🎲 Simulacion de latencia
     * Corrutina que espera X segundos antes de enviar los datos al servidor 
     */
    IEnumerator sendToClientSimulateLatency(byte[] dataSend, IPEndPoint clientAddress) {
        yield return new WaitForSeconds(sliderLatency.value / 1000);
        serverSocket.SendTo(dataSend, clientAddress);
    }
}

