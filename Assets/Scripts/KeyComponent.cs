using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class KeyComponent : MonoBehaviour
    {
        public string keyName; 

        public void PickUpKey()
        {
            gameObject.SetActive(false);

           PlayerInventory.Instance.AddKey(this.keyName);
        }
    }
}
