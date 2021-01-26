using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Funcionalidad principal de la comunicación por socket con los clientes.
/// Envio y recepcion de datagramas.
/// </summary>
public class ServerMain : MonoBehaviour{

    //// VAR PUB
    public int portServer;
    //Informacion de los clientes
    [Header("Clients Options")]
    public short maxClients = 2;

    //// REF PUB
    public List<Transform> instantiationPoints = new List<Transform>();
    public ClientsPhysic clientsPhysic;
    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;
   
    //// VAR PRI
    private List<Client> clientList = new List<Client>();
    private Socket serverSocket;
    private IPEndPoint serverAddress, clientAddress;
    private Thread thListenClients;

    //------------------------------------------------------------->

    /// <summary>
    /// Inicializacion
    /// </summary>
    void Start() {
        //1. Crear Socket UDP
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverAddress = new IPEndPoint(IPAddress.Any, portServer); //Abrir socket con la ip local
        serverSocket.Bind(serverAddress);

        //2. Iniciar hilo de escucha
        thListenClients = new Thread(new ThreadStart(Listen));
        thListenClients.Start();
    }

    //---------------------------------------------------------------

    /// <summary>
    /// Recepcion y procesado de datagramas de los clientes
    /// </summary>
    private void Listen() {
        Debug.Log("Servidor escuchando...");
        EndPoint clientAddAux = serverAddress; //Inicializar el objeto para evitar errores
        byte[] dataRec, dataSend;

        while (true) {
            dataRec = new byte[508];
            try {
                //Recibir datos
                serverSocket.ReceiveFrom(dataRec, ref clientAddAux);
            } catch (SocketException e) {
                Debug.LogError(e.ErrorCode);
            }

            //Almacenar direccion recepcion
            clientAddress = clientAddAux as IPEndPoint;

            //Leer y procesar datos
            BinaryReader br = new BinaryReader(new MemoryStream(dataRec));
            //Leer ActionCode.
            switch (br.ReadByte()) {
                //Solicitud entrada a partida
                case 1:
                    //Crear datagrama de respuesta
                    dataSend = new byte[508];
                    BinaryWriter bw = new BinaryWriter(new MemoryStream(dataSend));
                    bw.Write((byte)2); //ActionCode 2. 

                    //El nivel admite más usuario
                    if (clientList.Count < maxClients) {
                     
                        //Nuevo cliente
                        Client newClient = new Client(clientList.Count + 1, clientAddress);
                        clientList.Add(newClient);

                        //Instanciar
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            clientsPhysic.Instantiate(newClient, instantiationPoints[clientList.Count - 1].position)
                        );

                        //Respuesta al usuario
                        bw.Write(true); //Marca de admitido
                        bw.Write(newClient.GetId());

                        Debug.Log("Admitido " + clientAddress.Address.ToString() + ":" + clientAddress.Port);

                    //El nivel esta completo no admite mas usuarios
                    } else {
                        bw.Write(false); //Marca de no admitido
                    }

                    //Enviar respuesta
                    serverSocket.SendTo(dataSend, clientAddress);
                    break;

                //Input del cliente
                case 2:

                    Client client = searchClient(clientAddress);
                    short lengthTicks = br.ReadInt16(); //Cantidad de ticks enviados por el datagrama

                    //Recorrer cada tick recibido
                    for (int i = 0; i < lengthTicks; i++) {
                           
                        //Leer datos del datagrama
                        int tick = br.ReadInt32();
                        byte displacement = br.ReadByte();
                        bool jump = br.ReadBoolean();

                        //Descartar ticks ya ejecudos, se pueden recibir ticks duplicados debido al sistema 
                        //de proteccion frente a la perdida de datagramas en la comunicacion (cliente envía siempre sus 5 ultimos ticks/inputs)
                        if (tick > client.GetLastTickQueue()) {
                            InputTick inputTick;
                            inputTick.tick = tick;
                            inputTick.displacement = (EnumDisplacement) Enum.ToObject(typeof(EnumDisplacement), displacement);
                            inputTick.jump = jump;
                            client.AddInputTick(inputTick); //Agregar a la cola
                        }
                    }

                    break;
            }
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Al finalizar la ejecucion
    /// </summary>
    private void OnDestroy() {
        //Finalizar hilo
        thListenClients.Abort();
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Obtener un cliente del listado por su direccion IP
    /// </summary>
    /// <param name="address"></param>
    public Client searchClient(IPEndPoint address) {
        for (int i = 0; i < clientList.Count; i++) {
            if (clientList[i].GetAddress().Equals(address))
                return clientList[i];
        }
        return null;
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// Actualizacion cada fotograma
    /// </summary>
    private void Update() {
        //🎲 Simulacion. Actualizar datos de la interfaz gráfica
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
    }

    //---------------------------------------------------------------

    /// <summary>
    /// 🎲 Simulación del envío de información en una conexión UDP entre nodos remotos.
    /// Posible perdida de datagramas
    /// </summary>
    /// <param name="data"></param>
    /// <param name="clientAddress"></param>
    public void SendToClientSimulate(byte[] data, IPEndPoint clientAddress) {
        if (StaticMethods.Percent((short)sliderLostDatagram.value)) {
            Debug.LogWarning("Paquete perdido");
        } else {
            //🎲 Simulacion de latencia
            StartCoroutine(CorrutineSimulateLatency(data, clientAddress));
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// 🎲 Simulación del envío de información en una conexión UDP entre nodos remotos.
    /// Posible latencia en el envío
    /// </summary>
    /// <param name="data"></param>
    /// <param name="clientAddress"></param>
    /// <returns></returns>
    IEnumerator CorrutineSimulateLatency(byte[]data, IPEndPoint clientAddress) {    
        //🎲 Simulacion de latencia
        yield return new WaitForSeconds(sliderLatency.value / 1000);
        SendToClient(data, clientAddress);
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Enviar un datagrama a un cliente
    /// </summary>
    /// <param name="data"></param>
    /// <param name="clientAddress"></param>
    public void SendToClient(byte[] data, IPEndPoint clientAddress) {
        serverSocket.SendTo(data, clientAddress);
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public List<Client> GetClientList() {
        return clientList;
    }
}