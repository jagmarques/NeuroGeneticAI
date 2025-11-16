using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NeuroGeneticAI.Genetics;
using NeuroGeneticAI.Infrastructure.Diagnostics;
using NeuroGeneticAI.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroGeneticAI.Controllers
{
    
    // Loads two serialized neural networks from disk and plays a one-off exhibition
    // match between them.
    
    public class MatchMaker : MonoBehaviour
    {
        private readonly ILogger _logger = LogManager.Logger;

        // Singleton instance enforced at runtime.
        public static MatchMaker instance = null;

        // UI field used to display the match status.
        [HideInInspector]
        public Text infoText;

        // Indicates whether a match is currently running.
        [HideInInspector]
        public bool simulating = false;

        // Path to the serialized red controller.
        public string PathRedPlayer;

        // Path to the serialized blue controller.
        public string PathBluePlayer;

        // Simulation prefab used to create the arena.
        public GameObject simulationPrefab;

        private SimulationInfo bestSimulation;
        private NeuralNetwork BlueController;
        private NeuralNetwork RedController;

        // Time scale applied to the Unity scene.
        [HideInInspector]
        public int TheTimeScale = 1;

        // Duration of the match in seconds.
        public float MatchTimeInSecs;

        [Header("Scenario Conditions")]
        public bool randomRedPlayerPosition = false;
        public bool randomBluePlayerPosition = false;
        public bool randomBallPosition = false;
        public bool MovingBall = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            LoadPlayers();
            simulating = false;
        }

        private void LoadPlayers()
        {
            _logger.LogInformation($"Loading blue player from {PathBluePlayer}");
            if (File.Exists(PathBluePlayer))
            {
                BinaryFormatter bf = new BinaryFormatter();
                using FileStream file = File.Open(PathBluePlayer.Trim(), FileMode.Open);
                BlueController = (NeuralNetwork)bf.Deserialize(file);
            }
            else
            {
                _logger.LogWarning("Blue player path does not exist.");
            }

            _logger.LogInformation($"Loading red player from {PathRedPlayer}");
            if (File.Exists(PathRedPlayer))
            {
                BinaryFormatter bf = new BinaryFormatter();
                using FileStream file = File.Open(PathRedPlayer.Trim(), FileMode.Open);
                RedController = (NeuralNetwork)bf.Deserialize(file);
            }
            else
            {
                _logger.LogWarning("Red player path does not exist.");
            }
        }

        private SimulationInfo CreateSimulation(int simIndex, Rect location)
        {
            D31NeuralControler bluePlayerScript = null;
            D31NeuralControler redPlayerScript = null;
            GameObject sim = Instantiate(simulationPrefab, transform.position + new Vector3(0, (simIndex * 250), 0), Quaternion.identity);
            sim.GetComponentInChildren<Camera>().rect = location;

            Transform redTransform = sim.transform.Find("D31-red");
            if (redTransform != null)
            {
                redPlayerScript = redTransform.gameObject.transform.Find("Body").gameObject.GetComponent<D31NeuralControler>();
            }

            Transform blueTransform = sim.transform.Find("D31-blue");
            if (blueTransform != null)
            {
                bluePlayerScript = blueTransform.gameObject.transform.Find("Body").gameObject.GetComponent<D31NeuralControler>();
            }

            sim.transform.Find("Scoring System").gameObject.GetComponent<ScoreKeeper>().setIds(PathBluePlayer, PathRedPlayer);

            if (bluePlayerScript != null && bluePlayerScript.enabled)
            {
                bluePlayerScript.neuralController = BlueController;
                bluePlayerScript.maxSimulTime = MatchTimeInSecs;
                bluePlayerScript.running = true;
            }

            if ((redPlayerScript != null && redPlayerScript.enabled) || PathRedPlayer.Length != 0)
            {
                if (redPlayerScript != null)
                {
                    redPlayerScript.enabled = true;
                    redPlayerScript.neuralController = RedController;
                    redPlayerScript.maxSimulTime = MatchTimeInSecs;
                    redPlayerScript.running = true;
                }
            }

            return new SimulationInfo(sim, redPlayerScript, bluePlayerScript, 0, 0);
        }

        private void Update()
        {
            if (infoText != null)
            {
                infoText.text = $"Playing a match for {MatchTimeInSecs} secs";
            }

            if (!simulating)
            {
                Vector3 redPlayerStartPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(0, 20));
                Vector3 ballStartPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 0));
                bestSimulation = CreateSimulation(0, new Rect(0.0f, 0.0f, 1f, 1f));

                if (randomRedPlayerPosition)
                {
                    GameObject p = bestSimulation.sim.transform.Find("D31-red").gameObject;
                    redPlayerStartPosition.y = p.transform.localPosition.y;
                    p.transform.localPosition = redPlayerStartPosition;
                }

                if (randomBallPosition)
                {
                    GameObject p = bestSimulation.sim.transform.Find("Ball").gameObject;
                    ballStartPosition.y = p.transform.localPosition.y;
                    p.transform.localPosition = ballStartPosition;
                }

                if (MovingBall)
                {
                    Goal goal = bestSimulation.sim.transform.Find("Field").transform.Find("RedGoal").GetComponent<Goal>();
                    GameObject p = bestSimulation.sim.transform.Find("Ball").gameObject;
                    goal.initalBallPosition = randomBallPosition ? ballStartPosition : p.transform.position;
                    goal.ShootTheBallInMyDirection();
                }

                Time.timeScale = TheTimeScale;
                simulating = true;
            }
            else if (simulating)
            {
                if (!bestSimulation.playerRed.running && bestSimulation.playerRed.gameOver)
                {
                    _logger.LogInformation($"Red score: {bestSimulation.playerRed.GetScoreRed()}");
                    if (bestSimulation.playerBlue != null)
                    {
                        _logger.LogInformation($"Blue score: {bestSimulation.playerBlue.GetScoreBlue()}");
                    }

                    simulating = false;
                    DestroyImmediate(bestSimulation.sim);
                }
            }
        }
    }
}
