using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class AIDetection : MonoBehaviour
    {
        public Transform player;         
        public float viewRadius = 10f;   
        [Range(0, 360)] public float viewAngle = 90f;  

        public float proximityRadius = 2f;  
        public LayerMask obstacleMask;      
        public LayerMask playerMask;      

        public NavMeshAgent agent;         
        public float chaseSpeed = 6f;    
        public float patrolSpeed = 3.5f;

        private bool isChasing = false;    
        private Renderer aiRenderer;        
        public GameManager gameManager;   
		public Animator animator;

		void Start()
        {
            agent = GetComponent<NavMeshAgent>();  
            agent.speed = patrolSpeed;            
        }

        void Update()
        {
            DetectPlayer();

            if (isChasing)
            {
                agent.SetDestination(player.position);
            }
        }

        void DetectPlayer()
        {

            if (Vector3.Distance(transform.position, player.position) < proximityRadius)
            {
                StartChase();
				if (Vector3.Distance(transform.position, player.position) < 1f)  
				{
					gameManager.PlayerCaught(); 
				}
				return;
            }

            // Check if the player is within the view radius
            if (Vector3.Distance(transform.position, player.position) < viewRadius)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;

                // Check if the player is within the field of view angle
                if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
                {
                    // Check for obstacles between the AI and the player using a raycast
                    if (!Physics.Raycast(transform.position, directionToPlayer, Vector3.Distance(transform.position, player.position), obstacleMask))
                    {
                        StartChase();
                    }
                    else
                    {
                        StopChase();
                    }
                }
                else
                {
                    StopChase();
                }
            }
            else
            {
                StopChase();
            }
        }
        void StartChase()
        {
            if (!isChasing)
            {
				animator.SetBool("isChasing", true);
				isChasing = true;
                agent.speed = chaseSpeed;      
            }
        }
        void StopChase()
        {
            if (isChasing)
            {
				animator.SetBool("isChasing", false);
				isChasing = false;
                agent.speed = patrolSpeed;    
            }
        }
    }
}