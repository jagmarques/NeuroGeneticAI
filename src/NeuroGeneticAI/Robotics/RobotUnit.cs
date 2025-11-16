using NeuroGeneticAI.Sensors;
using UnityEngine;

namespace NeuroGeneticAI.Robotics
{
    
    // Represents the physical robot controlled by the neural network.  The class
    // tracks collisions with relevant game objects and exposes a reference to the
    // sensor subsystem.
    
    public class RobotUnit : MonoBehaviour
    {
        // Number of touches on the ball.
        public int hitTheBall;

        // Number of collisions with the arena wall.
        public int hitTheWall;

        // Rigid body used to apply forces.
        public Rigidbody rb;

        // Movement speed multiplier.
        public float speed;

        // Simulation start timestamp.
        public float startTime;

        // Elapsed simulation time.
        public float timeElapsed = 0.0f;

        // Detector that exposes visible objects.
        public DetectorScript objectsDetector;

        // When true the robot prints debugging information.
        public bool debugMode = true;

        private void Start()
        {
            hitTheBall = 0;
            rb = GetComponent<Rigidbody>();
            startTime = Time.time;
            timeElapsed = Time.time - startTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.tag.Equals("ball"))
            {
                hitTheBall++;
            }
            else if (collision.collider.tag.Equals("Wall"))
            {
                hitTheWall++;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.tag.Equals("ball"))
            {
                hitTheBall++;
            }
            else if (collision.collider.tag.Equals("Wall"))
            {
                hitTheWall++;
            }
        }
    }
}
