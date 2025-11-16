using UnityEngine;

namespace NeuroGeneticAI.Utils
{
    
    // Keeps a sensor rig positioned relative to the robot body.
    
    public class SensorController : MonoBehaviour
    {
        private Vector3 offset;

        // Robot being tracked.
        public GameObject player;

        private void Start()
        {
            offset = transform.position - player.transform.position;
        }

        private void LateUpdate()
        {
            transform.position = player.transform.position + offset;
        }
    }
}
