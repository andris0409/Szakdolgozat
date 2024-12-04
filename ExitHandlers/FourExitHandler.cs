using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public  class FourExitHandler : MonoBehaviour, IExitHandler
    {
        public List<GameObject> exitList;

        public GameObject GetExit(Exit exitDirection)
        {
            return exitDirection switch
            {
                Exit.NORTH => exitList[0],
                Exit.EAST => exitList[1],
                Exit.SOUTH => exitList[2],
                Exit.WEST => exitList[3],
                _ => null,
            };
        }
    }
}
