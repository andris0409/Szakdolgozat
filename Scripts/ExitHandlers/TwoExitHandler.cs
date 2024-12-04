using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.MapObjects;

namespace Assets.Scripts.Exithandlers
{
    public class TwoExitHandler : MonoBehaviour, IExitHandler
    {
        public GameObject exit1;
        public GameObject exit2;
        public GameObject GetExit(Exit exitDirection)
        {
            return exitDirection switch
            {
                Exit.NORTH => exit1,
                Exit.SOUTH => exit2,
                Exit.EAST => exit1,
                Exit.WEST => exit2,
                _ => null,
            };
        }

    }
}

