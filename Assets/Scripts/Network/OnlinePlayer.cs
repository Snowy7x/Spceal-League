using System;
using General;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class OnlinePlayer : NetworkBehaviour, IPlayer
    {
        [SerializeField] private GameObject camObject;
        [SerializeField] private MonoBehaviour[] controller;

        private void Awake()
        {
            camObject.SetActive(true);
            foreach (var component in controller)
            {
                component.enabled = true;
            }

            GetComponent<NetworkObject>().enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                camObject.SetActive(false);
                foreach (var component in controller)
                {
                    component.enabled = false;
                }
            }
        }
        
        public bool IsMe => IsOwner && IsLocalPlayer;
    }
}