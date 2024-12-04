using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class NavMeshBaker : MonoBehaviour
    {
        private NavMeshSurface surface;

        private void Awake()
        {
            surface = GetComponent<NavMeshSurface>();
        }

        public void BakeNavMesh()
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh baked");
        }
    }
    
}