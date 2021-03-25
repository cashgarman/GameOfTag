using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionScript : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _playButton;

    void Start()
    {
        // Clear out the status
        SetStatus("");

        SetInputsEnabled(false);
        
        // Restore the player name
        _nameInput.text = PlayerPrefs.GetString("playername");
        
        // Config Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Connection to the Photon master server
        PhotonNetwork.ConnectUsingSettings();
    }

    private void SetInputsEnabled(bool enabled)
    {
        _nameInput.interactable = enabled;
        _playButton.interactable = enabled;
    }

    public override void OnConnectedToMaster()
    {
        SetStatus("Connected to master");
        
        SetInputsEnabled(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetStatus($"Disconnected: {cause}");
    }

    public void OnCLick_Play()
    {
        // Make sure the player has entered a name
        if (string.IsNullOrEmpty(_nameInput.text))
        {
            SetStatus("Please enter a name");
            return;
        }
        
        // Set the player's nickname
        PhotonNetwork.NickName = _nameInput.text;
        
        // Store my name for the next run
        PlayerPrefs.SetString("playername", _nameInput.text);
        
        // Attempt to join a random room
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // There aren't any games running, so start one
        PhotonNetwork.CreateRoom("GameOfTag", new RoomOptions
        {
            MaxPlayers = 20
        });
    }

    public override void OnJoinedRoom()
    {
        // If we're the host
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    // Update is called once per frame
    private void SetStatus(string message)
    {
        _statusText.text = message;
    }
}
