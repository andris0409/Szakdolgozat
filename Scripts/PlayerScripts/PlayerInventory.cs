using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance;

        private List<string> collectedKeys = new List<string>();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddKey(string name)
        {
            if (!collectedKeys.Contains(name))
            {
                collectedKeys.Add(name);
            }
        }
        public bool HasKey(string name)
        {
            if(collectedKeys.Contains(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<string> GetAllKeys()
        {
            return new List<string>(collectedKeys);
        }
    }
}
