using UnityEngine;
using System.Collections.Generic;

public class Chat : MonoBehaviour {

    public List<string> chatHistory = new List<string>();

    private string currentMessage = string.Empty;

    NetworkView nView;

    void Start()
    {
        nView = GetComponent<NetworkView>();
    }

    private void OnGUI()
    {

        if (!NetworkManager.Connected)
        {
            return;
        }

        DisplayChat();
    } // end OnGUI()

    public void SendMessage(string message)
    {
        nView.RPC("ChatMessage", RPCMode.AllBuffered, new object[] { message });
        currentMessage = string.Empty;
    }

    [RPC]
    public void ChatMessage(string message)
    {
        chatHistory.Add(message);
    }



    private void DisplayChat()
    {
        currentMessage = GUI.TextField(new Rect(0, Screen.height - 20, 200, 20), currentMessage);

        if (GUI.Button(new Rect(200, Screen.height - 20, 75, 20), "Send"))
        {
            SendMessage(currentMessage);
        }

        for (int i = chatHistory.Count - 1; i >= 0; i--)
        {
            //GUILayout.Label(chatHistory[i]);
            GUI.Label(new Rect(0, Screen.height - 40 - (25 * i), 200, 20), chatHistory[i]);
        }
    }
}
