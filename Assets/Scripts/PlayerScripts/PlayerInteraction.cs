
using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PlayerInteraction : MonoBehaviour
    {
        public float interactDistance = 3f;
        public Text interactionText;
        public Text LockerExitText;
        private LockerComponent currentLocker;
        private bool isInsideLocker = false;
        private LayerMask interactableLayer;
        private bool showKeyMessage = false;  
        private float messageTimer = 0f;


        void Start()
        {
            interactionText.enabled = false;
            LockerExitText.enabled = false;
            interactableLayer = LayerMask.GetMask("Key", "Locker", "Door");
        }

        void Update()
        {
            if (showKeyMessage)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f)
                {
                    HideInteractionText();
                    showKeyMessage = false;
                }
                return; 
            }

            if (isInsideLocker)
            {
                HandleInsideLocker();
                return;
            }

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            // Raycast only against objects in the specified interactable layers
            if (!Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
            {
                interactionText.enabled = false;
                return;
            }

            // Check if the player is looking at a key
            KeyComponent key = hit.collider.GetComponent<KeyComponent>();
            if (key != null)
            {
                interactionText.text = "Press E to pick up key";
                interactionText.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    key.PickUpKey();
                }
                return;  
            }

            DoorComponent door = hit.collider.GetComponent<DoorComponent>();
            if (door != null && !door.isOpen)
            {
                interactionText.text = "Press E to open door";
                interactionText.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!door.TryUnlockDoor())
                    {
                        interactionText.text = "You do not have the required key";
                        showKeyMessage = true;  
                        messageTimer = 3f;
                    }
                    else
                    {
                        interactionText.enabled = false;
                    }
                }
                return;
            }

            if (hit.collider.CompareTag("LockerDoor"))
            {
                currentLocker = hit.collider.GetComponentInParent<LockerComponent>();

                if (currentLocker != null)
                {
                    interactionText.text = "Press E to Hide";
                    interactionText.enabled = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        EnterLocker();
                    }
                }
                else
                {
                    interactionText.enabled = false;
                }
            }
            else
            {
                interactionText.enabled = false;
            }
        }

        private void HideInteractionText()
        {
            interactionText.enabled = false;
        }
        private void HandleInsideLocker()
        {
            LockerExitText.text = "Press E to Exit";
            LockerExitText.enabled = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitLocker();
            }
        }

        private void EnterLocker()
        {
            if (currentLocker != null)
            {
                currentLocker.EnterLocker();
                isInsideLocker = true;
                interactionText.enabled = false;
            }
        }

        private void ExitLocker()
        {
            if (currentLocker != null)
            {
                currentLocker.ExitLocker();
                isInsideLocker = false;
                LockerExitText.enabled = false;
            }
        }
    }
}
