using UnityEngine;

namespace NeuroGeneticAI.Scoring
{
    
    // Detects ball triggers and increments the scoreboard accordingly.
    
    public class Goal : MonoBehaviour
    {
        // Ball spawner used to reset state between goals.
        public GameObject ballSpawner;

        // The ball currently in play.
        public GameObject ball;

        // Scoreboard reference.
        public ScoreKeeper scoreKeeper;

        // Identifies which team owns this goal.
        public enum WhichGoal
        {
            // Blue team goal.
            Blue,

            // Red team goal.
            Red,
        }

        protected bool defenseTask = false;

        // Goal identifier used to increment the proper team score.
        public WhichGoal whichGoal;

        // Initial ball position used for defense scenarios.
        public Vector3 initalBallPosition;

        // Applies a force that pushes the ball toward the goal.
        public void ShootTheBallInMyDirection()
        {
            defenseTask = true;
            Vector3 shoot = (transform.localPosition - ball.transform.localPosition).normalized;
            ball.GetComponent<Rigidbody>().AddForce(shoot * 800.0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag != "ball")
            {
                return;
            }

            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            scoreKeeper.ScoreGoal((int)whichGoal);

            if (defenseTask)
            {
                ball.transform.localPosition = initalBallPosition;
                ShootTheBallInMyDirection();
            }
            else
            {
                ball.transform.position = ballSpawner.transform.position;
            }
        }
    }
}
