using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class AIMovement : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Transform currenttarget;

        private Transform room1;
        private Transform room2;

        public void Initialize(Transform startRoom, Transform patrolRoom)
        {
            agent = GetComponent<NavMeshAgent>();
            room1 = startRoom;
            room2 = patrolRoom;
            currenttarget = room1;
            agent.SetDestination(room2.position);
        }

        private void Update()
        {
            if (agent != null)
            {
                if (agent.remainingDistance < 0.5f && !agent.pathPending)
                {
                    currenttarget = currenttarget == room1 ? room2 : room1;
                    agent.SetDestination(currenttarget.position);
                }
            }

        }
    }

}

