using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{


    public class DoorComponent : MonoBehaviour
    {

        public bool isOpen = false;
        public string RequiredKey { get; set; }
        private Animator doorAnimator;
        public Text messageText;

        // Start is called before the first frame update
        void Start()
        {
            doorAnimator = GetComponent<Animator>();
        }

        public bool TryUnlockDoor()
        {
            if (PlayerInventory.Instance.HasKey(RequiredKey))
            {
                if (!isOpen)
                {
                    OpenDoor();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OpenDoor()
        {
            Debug.Log("most kene animacio");
            doorAnimator.SetTrigger("OpenDoor");
            isOpen = true;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
