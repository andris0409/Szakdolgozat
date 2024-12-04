using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class OneExitHandler : MonoBehaviour, IExitHandler
    {
        public GameObject exit;
        public GameObject GetExit(Exit exitDirection)
        {
            return exit;
        }

    }
}
