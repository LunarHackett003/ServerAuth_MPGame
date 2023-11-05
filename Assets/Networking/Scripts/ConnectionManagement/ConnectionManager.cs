using Eflatun.SceneReference;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eclipse.Networks {
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager instance;
        public string hostedJoinCode, targetJoinCode;
        public int maxConnections;
        public string preferredRegion;
        public SceneReference chosenMap;
        public SceneReference menuScene;
        public TextMeshProUGUI tmp;
        public UnityTransport localTransport, relayTransport;
        private void Awake()
        {
            Debug.Log($"Checking singleton on {name}...");
            if (!instance)
                instance = this;
            else
                Destroy(gameObject);
            Debug.Log($"Successfully created singleton instance of {name}!");
        }
        public static async Task<RelayServerData> AllocateRelayAndGetJoinCode(int maxConnections, string region = null)
        {
            Allocation allocation;
            string createJoinCode;
            try
            {
                allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay create allocation request failed {e.Message}");
                throw;
            }

            Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"server: {allocation.AllocationId}");

            try
            {
                createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                instance.hostedJoinCode = createJoinCode;
            }
            catch
            {
                Debug.LogError("Relay create join code request failed");
                throw;
            }
            instance.tmp.text = createJoinCode;
            return new RelayServerData(allocation, "dtls");
        }
        public void SetJoinCode(string joinCode)
        {
            targetJoinCode = joinCode;
        }
        public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
        {
            JoinAllocation allocation;
            try
            {
                allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch
            {
                Debug.LogError("Relay create join code request failed");
                throw;
            }

            Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
            Debug.Log($"client: {allocation.AllocationId}");

            return new RelayServerData(allocation, "dtls");
        }
        public async void JoinGame()
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;
            if(UnityServices.State == ServicesInitializationState.Uninitialized || !AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                   await AuthServiceSignIn();
                }
                catch
                {
                    Debug.LogWarning("Failed to sign in! Please check your connection.");
                }
            }
            
            try
            {
                RelayServerData joinedRelayServerData = await JoinRelayServerFromJoinCode(targetJoinCode);
                relayTransport.SetRelayServerData(joinedRelayServerData);
                NetworkManager.Singleton.StartClient();
                instance.tmp.text = targetJoinCode;
                NetworkManager.Singleton.OnClientStopped += Singleton_OnClientStopped;
            }
            catch
            {

            }
        }

        private void Singleton_OnClientStopped(bool obj)
        {
            ReturnToMenu();
            NetworkManager.Singleton.OnClientStopped -= Singleton_OnClientStopped;
        }
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(menuScene.BuildIndex);
        }
        public async void CreateGame()
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;
            if (UnityServices.State == ServicesInitializationState.Uninitialized || !AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await AuthServiceSignIn();
                }
                catch
                {
                    Debug.LogWarning("Failed to sign in! Please check your connection.");
                }
            }
            try
            {
                RelayServerData hostedRelayServerData = await AllocateRelayAndGetJoinCode(maxConnections);
                relayTransport.SetRelayServerData(hostedRelayServerData);
                NetworkManager.Singleton.StartHost();
                NetworkManager.Singleton.SceneManager.LoadScene(chosenMap.Name, LoadSceneMode.Single);
                NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
            }
            catch
            {

            }
        }

        private void Singleton_OnServerStopped(bool obj)
        {
            ReturnToMenu();
            NetworkManager.Singleton.OnServerStopped -= Singleton_OnServerStopped;
        }
        public void DisconnectFromGame()
        {
            NetworkManager.Singleton.Shutdown();
        }
        public async Task AuthServiceSignIn()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.LogException(e);
            }
        }

        public void StartLocalGame()
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = localTransport;
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(chosenMap.Name, LoadSceneMode.Single);
            NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
        }
    }
}