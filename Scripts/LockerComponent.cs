using Assets.Scripts.MapObjects;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ModularFirstPersonController;

namespace Assets.Scripts
{
    public class LockerComponent : MonoBehaviour
    {
        public Locker lockerData;
        public Camera playerCamera;
        public Camera lockerCamera;
        private GameObject playerObject;  
        private MonoBehaviour playerController;
        FirstPersonController firstPersonController;

        void Start()
        {
            playerCamera = Camera.main;
            lockerCamera = GetComponentInChildren<Camera>();
            playerObject = GameObject.FindGameObjectWithTag("Player");
            firstPersonController = playerObject.GetComponent<FirstPersonController>();



            if (playerObject == null)
            {
                Debug.LogError("Player object not found!");
            }
            else if (firstPersonController == null)
            {
                Debug.LogError("First person controller not found!");
            }
            else if (playerCamera == null)
            {
                Debug.LogError("Player camera not found!");
            }
            else if (lockerCamera == null)
            {
                Debug.LogError("Locker camera not found!");
            }
            else
            {
                lockerCamera.enabled = false;  
            }
        }

        public void InitializeLocker(Locker locker)
        {
            lockerData = locker;
        }

        public void EnterLocker()
        {
            Debug.Log("camera should switch inside");
            playerCamera.enabled = false;
            MeshRenderer playerRenderer = playerObject.GetComponent<MeshRenderer>();
            playerRenderer.enabled = false;
            firstPersonController.playerCanMove = false;
            firstPersonController.cameraCanMove = false;
            lockerCamera.enabled = true;
        }

        public void ExitLocker()
        {
            Debug.Log("camera should switch outside");
            lockerCamera.enabled = false;
            MeshRenderer playerRenderer = playerObject.GetComponent<MeshRenderer>();
            playerRenderer.enabled = true;
            firstPersonController.playerCanMove = true;
            firstPersonController.cameraCanMove = true;
            playerCamera.enabled = true;

        }

    }
}
