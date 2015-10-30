using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))] // because we will use rpc calls
public class NetworkManager : MonoBehaviour {

	private string ip = "127.0.0.1";
	private int port = 25001;
    private NetworkView nView;
    public static bool Connected { get; private set; }
    public int Port
    {
        get { return port; }
        set
        {
            port = value;
        }
    }
    public string IP
    {
        get { return ip; }
        set
        {
            if (value != null || value.Length == 0 || value == "")
            {
                ip = "127.0.0.1";
            } else
            {
                ip = value;
            }
        }
    }

    public GameObject prefab;
    private ArrayList prefabs;

    void Start()
    {
        nView = GetComponent<NetworkView>();
        prefabs = new ArrayList();
    }

    private void OnDisconnectedFromServer()
    {
        Connected = false;
    }

    private void OnConnectedToServer()
    {
        
    }

    private void OnServerInitialized()
    {

    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    private void StartServer()
    {
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(4, port, true);
        //MasterServer.RegisterHost("Jens", "Jens Test", "NetworkingTest");
    }

    void OnGUI()
	{
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
            //ip = GUI.TextField(new Rect(0, 20, 200, 20), ip);

            if (GUI.Button(new Rect(100, 100, 100, 25), "Start Client"))
            {
                NetworkConnectionError error = Network.Connect(IP, port);
            }

            if (GUI.Button(new Rect(100, 125, 100, 25), "Start Server"))
            {
                StartServer();
            }
        } // end if disconnected
        else
        {
            Connected = true;

            GUI.Label(new Rect(0, 0, 200, 50), "IP: " + IP);
            GUI.Label(new Rect(0, 15, 200, 50), "port: " + Network.player.port);

            if (Network.peerType == NetworkPeerType.Client)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Client");
                if (GUI.Button(new Rect(100, 125, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }
            }

            if (Network.peerType == NetworkPeerType.Server)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Server");
                GUI.Label(new Rect(100, 125, 100, 25), "Connections: " + Network.connections.Length);
                if (GUI.Button(new Rect(100, 150, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }
            }

            if (GUI.Button(new Rect(Screen.width / 2, 10f, 150f, 30f), "Spawn Object"))
            {
                GetComponent<NetworkView>().RPC("AskSpawnObject", RPCMode.All);
            }
        } // end else

    } // end OnGUI()

    /// <summary>
    /// Ask server to create numbers for a cube spawn.
    /// </summary>
    [RPC]
    void AskSpawnObject()
    {
        if (Network.isServer)
        {
            float randomX = Random.Range(Screen.width * -1, Screen.width) / 100;
            float randomY = Random.Range(Screen.height * -1, Screen.height) / 100;
            GetComponent<NetworkView>().RPC("SpawnObject", RPCMode.All, randomX, randomY);
        }
    }

    /// <summary>
    /// Spawns a cube at a random position.
    /// </summary>
    /// <param name="randomValueX">Random value for X.</param>
    /// <param name="randomValueY">Random value for Y.</param>
    [RPC]
    void SpawnObject(float randomValueX, float randomValueY)
    {
        Instantiate(prefab, new Vector3(randomValueX, randomValueY, 0), Quaternion.identity);
    }

}
