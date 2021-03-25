using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private Camera _camera;
    [SerializeField] private TMP_Text _nameText;
    
    private bool _isTagged;
    [SerializeField] private Color _taggedColour;
    private Color _initialColour;
    private float _touchbackCoutdown;
    [SerializeField] private float _touchbackDuration;
    private float _timeSpentTagged;

    void Awake()
    {
        // If this is not the local player
        if (!photonView.IsMine)
        {
            Destroy(_camera.gameObject);
        }
        
        // Update the player's name display
        UpdateNameDisplay();
        
        // Store the initial colour
        _initialColour = GetComponentInChildren<SkinnedMeshRenderer>().material.color;
    }

    [PunRPC]
    public void OnTagged()
    {
        // Flag the player as tagged
        _isTagged = true;
        
        // Start the touchbacks countdown
        _touchbackCoutdown = _touchbackDuration;
        
        // Change the colour of the player to be the tagged colour
        GetComponentInChildren<SkinnedMeshRenderer>().material.color = _taggedColour;
    }
    
    [PunRPC]
    public void OnUntagged()
    {
        // Flag the player as untagged
        _isTagged = false;
        
        // Restore the colour of the player to be the initial colour
        GetComponentInChildren<SkinnedMeshRenderer>().material.color = _initialColour;
    }

    private void Update()
    {
        UpdateNameDisplay();

        // Only run this for our local player
        if (photonView.IsMine)
        {
            // Reduce our touchbacks timer
            if (_touchbackCoutdown > 0f)
            {
                _touchbackCoutdown -= Time.deltaTime;
            }
            
            // If we're tagged
            if (_isTagged)
            {
                // Increase the total time spent tagged
                _timeSpentTagged += Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // Check if the thing that hit us is a player
        var otherPlayer = other.collider.GetComponent<PlayerController>();
        if (otherPlayer != null)
        {
            // If we're tagged and we're not under the no touchbacks rule
            if (_isTagged && _touchbackCoutdown <= 0f)
            {
                // Untag ourselves
                photonView.RPC("OnUntagged", RpcTarget.AllBuffered);
                
                // Tag the other player
                otherPlayer.photonView.RPC("OnTagged", RpcTarget.AllBuffered);
            }
        }
    }

    private void UpdateNameDisplay()
    {
        // If this player is winning
        var isWinning = FindObjectsOfType<PlayerController>().OrderBy(p => p._timeSpentTagged).First() == this;

        if (isWinning)
        {
            _nameText.text = $"<color=green>{photonView.Owner.NickName}</color>\n<size=50%>{_timeSpentTagged:F1} sec</size>";
        }
        else
        {
            _nameText.text = $"{photonView.Owner.NickName}\n<size=50%>{_timeSpentTagged:F1} sec</size>";
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(_isTagged);
            stream.SendNext(_timeSpentTagged);
        }
        
        if(stream.IsReading)
        {
            //_isTagged = (bool) stream.ReceiveNext();
            _timeSpentTagged = (float)stream.ReceiveNext();

            UpdateNameDisplay();
        }
    }
}
