using System;
using System.Collections.Generic;
using System.Linq;
using NeuroGeneticAI.Genetics;
using NeuroGeneticAI.Infrastructure.Diagnostics;
using NeuroGeneticAI.Robotics;
using NeuroGeneticAI.Scoring;
using NeuroGeneticAI.Sensors;
using NeuroGeneticAI.Utils;
using UnityEngine;

namespace NeuroGeneticAI.Controllers
{
    
    // MonoBehaviour that bridges the evolved neural controller with the Unity scene by
    // updating the robot's sensors, applying forces, and collecting fitness metrics.
    
    public class D31NeuralControler : MonoBehaviour
    {
        private const float FieldSize = 95.0f;
        private readonly ILogger _logger = LogManager.Logger;

        // Robot controller attached to the scene object.
        public RobotUnit agent;

        // Index of the player (0 = blue, 1 = red).
        public int player;

        // Reference to the soccer ball.
        public GameObject ball;

        // The friendly goal.
        public GameObject MyGoal;

        // The goal being defended by the adversary.
        public GameObject AdversaryGoal;

        // The adversary robot.
        public GameObject Adversary;

        // Score keeping system within the simulation prefab.
        public GameObject ScoreSystem;

        // Gets the number of input sensors exposed to the neural network.
        public int numberOfInputSensores { get; private set; }

        // Normalized sensor readings consumed by the network.
        public float[] sensorsInput;

        [Header("Environment  Information")]
        public List<float> distanceToBall;
        public List<float> distanceToMyGoal;
        public List<float> distanceToAdversaryGoal;
        public List<float> distanceToAdversary;
        public List<float> distancefromBallToAdversaryGoal;
        public List<float> distancefromBallToMyGoal;
        public List<float> distanceToClosestWall;
        public List<float> agentSpeed;
        public List<float> ballSpeed;
        public List<float> advSpeed;

        // Elapsed time since the simulation started.
        public float simulationTime = 0;

        // Total distance covered by the robot in meters.
        public float distanceTravelled = 0.0f;

        // Average speed observed across the simulation.
        public float avgSpeed = 0.0f;

        // Maximum speed achieved by the robot.
        public float maxSpeed = 0.0f;

        // Current speed computed from the last position delta.
        public float currentSpeed = 0.0f;

        // Distance covered during the last sampling window.
        public float currentDistance = 0.0f;

        // Number of times the robot touched the ball.
        public int hitTheBall;

        // Number of times the robot collided with a wall.
        public int hitTheWall;

        // Number of FixedUpdate calls executed.
        public int fixedUpdateCalls = 0;

        // Maximum allowed simulation time for a single evaluation.
        public float maxSimulTime = 30;

        // Enables a special debug scene that spawns a sample neural network.
        public bool GameFieldDebugMode = false;

        // Flag that signals when the evaluation concluded.
        public bool gameOver = false;

        // Indicates if the controller is currently simulating.
        public bool running = false;

        private Vector3 startPos;
        private Vector3 previousPos;
        private Vector3 ballStartPos;
        private Vector3 ballPreviousPos;
        private Vector3 advPreviousPos;
        private int sampleRate = 1;
        private int countFrames = 0;

        // Goals scored on the adversary goal.
        public int GoalsOnAdversaryGoal;

        // Goals conceded on the friendly goal.
        public int GoalsOnMyGoal;

        // Neural network outputs for the latest step.
        public float[] result;

        // The neural network being evolved.
        public NeuralNetwork neuralController;

        private ScoreKeeper? scoreKeeper;

        private void Awake()
        {
            _logger.LogInformation($"Initializing controller for player {player}");
            agent = GetComponent<RobotUnit>();
            if (agent == null)
            {
                throw new InvalidOperationException("RobotUnit component is required");
            }

            numberOfInputSensores = 18;
            sensorsInput = new float[numberOfInputSensores];

            startPos = agent.transform.localPosition;
            previousPos = startPos;
            if (ball != null)
            {
                ballPreviousPos = ball.transform.localPosition;
            }
            if (Adversary != null)
            {
                advPreviousPos = Adversary.transform.localPosition;
            }

            if (GameFieldDebugMode && neuralController?.weights == null)
            {
                _logger.LogWarning("GameFieldDebugMode is enabled – instantiating a sample neural network.");
                int[] topology = { 12, 4, 2 };
                neuralController = new NeuralNetwork(topology, 0);
            }

            distanceToBall = new List<float>();
            distanceToMyGoal = new List<float>();
            distanceToAdversaryGoal = new List<float>();
            distanceToAdversary = new List<float>();
            distancefromBallToAdversaryGoal = new List<float>();
            distancefromBallToMyGoal = new List<float>();
            distanceToClosestWall = new List<float>();
            agentSpeed = new List<float>();
            ballSpeed = new List<float>();
            advSpeed = new List<float>();
            scoreKeeper = ScoreSystem?.GetComponent<ScoreKeeper>();
        }

        private void FixedUpdate()
        {
            if (countFrames == 0 && ball != null)
            {
                ballStartPos = ball.transform.localPosition;
                ballPreviousPos = ballStartPos;
            }

            simulationTime += Time.deltaTime;
            if (running && fixedUpdateCalls % 10 == 0)
            {
                UpdateSensors();
                if (neuralController == null)
                {
                    throw new InvalidOperationException("Neural controller is not configured.");
                }

                result = neuralController.process(sensorsInput);
                ApplyNetworkForces(result);
                UpdateGameStatus();

                if (ReachedEndOfSimulation())
                {
                    WrapUp();
                }

                countFrames++;
            }

            fixedUpdateCalls++;
        }

        
        // Samples the current state of the environment and writes normalized values to
        // .
        
        public void UpdateSensors()
        {
            if (agent.objectsDetector == null)
            {
                throw new InvalidOperationException("RobotUnit must expose a detector script.");
            }

            Dictionary<string, ObjectInfo> objects = agent.objectsDetector.GetVisibleObjects();
            if (!objects.Any())
            {
                _logger.LogWarning("Detector returned no visible objects – skipping sensor update.");
                return;
            }

            PopulateMandatorySensor(objects, "DistanceToBall", 0);
            PopulateMandatorySensor(objects, "MyGoal", 2);
            PopulateMandatorySensor(objects, "AdversaryGoal", 4);
            PopulateOptionalSensor(objects, "Adversary", 6);
            PopulateMandatorySensor(objects, "Wall", 10);

            sensorsInput[8] = NormalizeDistance(Vector3.Distance(ball.transform.localPosition, MyGoal.transform.localPosition));
            sensorsInput[9] = NormalizeDistance(Vector3.Distance(ball.transform.localPosition, AdversaryGoal.transform.localPosition));

            Vector2 pp = new Vector2(previousPos.x, previousPos.z);
            Vector2 aPos = new Vector2(agent.transform.localPosition.x, agent.transform.localPosition.z) - pp;
            sensorsInput[12] = NormalizeDistance(aPos.magnitude);
            sensorsInput[13] = Vector2.Angle(aPos, Vector2.right) / 360.0f;

            pp = new Vector2(ballPreviousPos.x, ballPreviousPos.z);
            aPos = new Vector2(ball.transform.localPosition.x, ball.transform.localPosition.z) - pp;
            sensorsInput[14] = NormalizeDistance(aPos.magnitude);
            sensorsInput[15] = Vector2.Angle(aPos.normalized, Vector2.right) / 360.0f;

            if (Adversary != null)
            {
                Vector2 adp = new Vector2(advPreviousPos.x, advPreviousPos.z);
                Vector2 adPos = new Vector2(Adversary.transform.localPosition.x, Adversary.transform.localPosition.z) - adp;
                sensorsInput[16] = NormalizeDistance(adPos.magnitude);
                sensorsInput[17] = Vector2.Angle(adPos, Vector2.right) / 360.0f;
            }
            else
            {
                sensorsInput[16] = -1;
                sensorsInput[17] = -1;
            }

            if (countFrames % sampleRate == 0)
            {
                distanceToBall.Add(sensorsInput[0]);
                distanceToMyGoal.Add(sensorsInput[2]);
                distanceToAdversaryGoal.Add(sensorsInput[4]);
                distanceToAdversary.Add(sensorsInput[6]);
                distancefromBallToMyGoal.Add(sensorsInput[8]);
                distancefromBallToAdversaryGoal.Add(sensorsInput[9]);
                distanceToClosestWall.Add(sensorsInput[10]);
                agentSpeed.Add(sensorsInput[12]);
                ballSpeed.Add(sensorsInput[14]);
                advSpeed.Add(sensorsInput[16]);
            }
        }

        
        // Refreshes runtime metrics such as total distance, score, and last known
        // positions.
        
        public void UpdateGameStatus()
        {
            Vector2 pp = new Vector2(previousPos.x, previousPos.z);
            Vector2 aPos = new Vector2(agent.transform.localPosition.x, agent.transform.localPosition.z);
            currentDistance = Vector2.Distance(pp, aPos);
            distanceTravelled += currentDistance;
            currentSpeed = currentDistance / Math.Max(Time.deltaTime, 0.0001f);
            avgSpeed += currentSpeed;
            maxSpeed = Math.Max(maxSpeed, currentSpeed);
            hitTheBall = agent.hitTheBall;
            hitTheWall = agent.hitTheWall;

            previousPos = agent.transform.localPosition;
            ballPreviousPos = ball.transform.localPosition;
            if (Adversary != null)
            {
                advPreviousPos = Adversary.transform.localPosition;
            }

            if (scoreKeeper != null)
            {
                GoalsOnMyGoal = scoreKeeper.score[player == 0 ? 1 : 0];
                GoalsOnAdversaryGoal = scoreKeeper.score[player];
            }
        }

        
        // Calculates the population fitness for the blue player.
        
        // Returns: Blue fitness score.
        public float GetScoreBlue()
        {
            float fitness = distanceTravelled + ((distanceToBall.Average() * -1) * 100) +
                            ((distanceToAdversaryGoal.Average() * -1) * 50) +
                            distancefromBallToMyGoal.Average() +
                            (distancefromBallToAdversaryGoal.Average() * -1) +
                            (hitTheBall * 100) +
                            (GoalsOnAdversaryGoal * 500) +
                            ((GoalsOnMyGoal * -1) * 200) +
                            ((hitTheWall * -1) * 10);
            _logger.LogInformation($"Blue fitness computed as {fitness:0.00}");
            return fitness;
        }

        
        // Calculates the population fitness for the red player.
        
        // Returns: Red fitness score.
        public float GetScoreRed()
        {
            float fitness = ((GoalsOnMyGoal * -1) * 1000) +
                            (100 * distancefromBallToMyGoal.Average()) +
                            ((distanceToBall.Average() * -1) * 1000) +
                            distanceTravelled +
                            (100 * hitTheBall);
            _logger.LogInformation($"Red fitness computed as {fitness:0.00}");
            return fitness;
        }

        
        // Determines if the simulation reached the configured time limit.
        
        // Returns: true when the maximum simulation time is exceeded.
        public bool ReachedEndOfSimulation()
        {
            maxSimulTime = 25;
            return simulationTime > maxSimulTime;
        }

        
        // Resets mutable counters and signals that the evaluation finished.
        
        public void WrapUp()
        {
            avgSpeed = simulationTime > 0 ? avgSpeed / simulationTime : 0;
            gameOver = true;
            running = false;
            countFrames = 0;
            fixedUpdateCalls = 0;
            _logger.LogInformation($"Simulation finished in {simulationTime:0.00}s");
        }

        
        // Computes the standard deviation of the supplied value collection.
        
        // values: Values to analyze.
        // Returns: Standard deviation value.
        public static float StdDev(IEnumerable<float> values)
        {
            float ret = 0;
            int count = values.Count();
            if (count > 1)
            {
                float avg = values.Average();
                float sum = values.Sum(d => (d - avg) * (d - avg));
                ret = Mathf.Sqrt(sum / count);
            }

            return ret;
        }

        private void PopulateMandatorySensor(Dictionary<string, ObjectInfo> objects, string key, int baseIndex)
        {
            if (!objects.TryGetValue(key, out ObjectInfo? info))
            {
                _logger.LogWarning($"Sensor {key} missing; populating with sentinel values.");
                sensorsInput[baseIndex] = -1;
                sensorsInput[baseIndex + 1] = -1;
                return;
            }

            sensorsInput[baseIndex] = NormalizeDistance(info.distance);
            sensorsInput[baseIndex + 1] = info.angle / 360.0f;
        }

        private void PopulateOptionalSensor(Dictionary<string, ObjectInfo> objects, string key, int baseIndex)
        {
            if (!objects.TryGetValue(key, out ObjectInfo? info))
            {
                sensorsInput[baseIndex] = -1;
                sensorsInput[baseIndex + 1] = -1;
                return;
            }

            sensorsInput[baseIndex] = NormalizeDistance(info.distance);
            sensorsInput[baseIndex + 1] = info.angle / 360.0f;
        }

        private void ApplyNetworkForces(IReadOnlyList<float> output)
        {
            if (output.Count < 2)
            {
                _logger.LogWarning("Network output was empty, skipping movement.");
                return;
            }

            float angle = output[0] * 180;
            float strength = output[1];
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
            dir.z = dir.y;
            dir.y = 0;
            agent.rb.AddForce(dir * strength * agent.speed);
        }

        private static float NormalizeDistance(float distance) => distance / FieldSize;
    }
}
