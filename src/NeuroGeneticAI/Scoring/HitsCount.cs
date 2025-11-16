using NeuroGeneticAI.Controllers;
using UnityEngine;

namespace NeuroGeneticAI.Scoring
{
    
    // Counts how many times each player collides with the ball so the genetic
    // algorithm can reward offensive behaviour.
    
    public class HitsCount : MonoBehaviour
    {
        // Blue player instance.
        public GameObject BluePlayer;

        // Red player instance.
        public GameObject RedPlayer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "BluePlayer" && BluePlayer != null)
            {
                BluePlayer.GetComponent<D31NeuralControler>().hitTheBall++;
            }
            else if (other.gameObject.tag == "RedPlayer" && RedPlayer != null)
            {
                RedPlayer.GetComponent<D31NeuralControler>().hitTheBall++;
            }
        }
    }
}
