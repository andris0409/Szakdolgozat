using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class InventoryUI : MonoBehaviour
    {
        public Text keysText;

        // Update is called once per frame
        void Update()
        {
            List<string> keys = PlayerInventory.Instance.GetAllKeys();
            keysText.text = "Keys: " + string.Join(", ", keys);
        }
    }
}
