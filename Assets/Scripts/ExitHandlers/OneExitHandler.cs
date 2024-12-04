using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.MapObjects;

namespace Assets.Scripts.Exithandlers
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
