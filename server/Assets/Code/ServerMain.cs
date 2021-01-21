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
    public ClientsPhysic clientsPhysic;

    [Header("Clients Options")]
    public short maxClients = 2;
    public List<Transform> instantiationPoints = new List<Transform>();

    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;

    /// VAR PRI
    private List<Client> clientList = new List<Client>();

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
                    if (clientList.Count < maxClients) {
                        Debug.Log("Admitido " + clientAddress.Address.ToString() + ":" + clientAddress.Port);
                        bw.Write(true); //Marca de admitido
                        //Agregar cliente al listado
                        Client newClient = new Client(clientList.Count + 1, clientAddress);
                        clientList.Add(newClient);
                        bw.Write(newClient.GetId()); //Id del cliente
                        //Instanciar
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            clientsPhysic.instantiate(newClient, instantiationPoints[clientList.Count - 1].position)
                        );


                    } else {
                        bw.Write(false); //Marca de no admitido
                    }

                    serverSocket.SendTo(dataSend, clientAddress);
                    break;

                //Movimiento del personaje
                case 2:

                    Client client = searchClient(clientAddress);

                    short lengthTicks = br.ReadInt16(); //Cantidad de ticks enviados por el datagrama

                    //Recorrer cada tick recibido
                    for (int i = 0; i < lengthTicks; i++) {
                           
                        //Leer datos del datagrama
                        int tick = br.ReadInt32();
                        //Parsear byte to enum desplazamiento
                        byte displacement = br.ReadByte();
                        //Leer bool salto
                        bool jump = br.ReadBoolean();

                        //Comprobar si el tick en cuestion se ha ejecutado en el servidor
                        //El envío redundante puede causarse debido a que el cliente cuando envia su mensaje de tick 
                        //aun no haya recibido el acuse de recibo de un tick ejecutado en el servidor, o incluso que 
                        //este mensaje se haya perdido en la comunicacion
                        if (tick > client.GetLastTickQueue()) {
                            InputTick inputTick;
                            inputTick.tick = tick;
                            inputTick.displacement = (EnumDisplacement) Enum.ToObject(typeof(EnumDisplacement), displacement);
                            inputTick.jump = jump;
                            client.AddInputTick(inputTick);
                        }
                    }

                    break;
            }
        }
    }
    

    //------------------------------------------------------------------------

    private void Update() {
        // 🎲 Simulacion perdida de datagramas
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
        //🎲 Simulacion de latencia
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";
    }

    //---------------------------------------------------------------

    /**
    * 🎲 Simulacion de latencia + perdida de paquetes
    * Corrutina que espera X segundos antes de enviar los datos al servidor 
    */
    public void SendToClientSimulate(byte[] data, IPEndPoint clientAddress) {
        if (StaticMethods.percent((short)sliderLostDatagram.value)) {
            Debug.LogWarning("Paquete perdido");
        } else {
            //🎲 Simulacion de latencia
            StartCoroutine(CorrutineSimulateLatency(data, clientAddress));
        }
    }
    IEnumerator CorrutineSimulateLatency(byte[]data, IPEndPoint clientAddress) {    
        //🎲 Simulacion de latencia
        yield return new WaitForSeconds(sliderLatency.value / 1000);
        SendToClient(data, clientAddress);
    }



    /**
     * METODO ENVIAR MENSAJE AL CLIENTE
       */
    public void SendToClient(byte[] data, IPEndPoint clientAddress) {
        serverSocket.SendTo(data, clientAddress);
    }





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

    public List<Client> GetClientList() {
        return clientList;
    }
   

}

