using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.MapObjects;

namespace Assets.Scripts.Exithandlers
{
    public class TwoExitHandlerLShape : MonoBehaviour, IExitHandler
    {
        public List<GameObject> Exits;
        public float InitialYRotation;

        public GameObject GetExit(Exit exitDirection)
        {
            float yRotation = (transform.rotation.eulerAngles.y-InitialYRotation+360)%360;

            float tolerance = 1.0f;

            int rotation = 0;

            for (int i = 0; i<360; i+=90)
            {
                if (Mathf.Abs(yRotation - i) < tolerance)
                {
                    rotation = i;
                }
            }

            return rotation switch
            {
                0 => exitDirection switch
                {
                    Exit.NORTH => Exits[0],
                    Exit.EAST => Exits[1],
                    _ => null,
                },
                90 => exitDirection switch
                {
                    Exit.EAST => Exits[0],
                    Exit.SOUTH => Exits[1],
                    _ => null,
                },
                180 => exitDirection switch
                {
                    Exit.SOUTH => Exits[0],
                    Exit.WEST => Exits[1],
                    _ => null,
                },
                270 => exitDirection switch
                {
                    Exit.WEST => Exits[0],
                    Exit.NORTH => Exits[1],
                    _ => null,
                },
                _ => null,
            };
        }
    }
}
