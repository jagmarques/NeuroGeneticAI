using System.Collections.Generic;
using System.Linq;
using NeuroGeneticAI.Genetics;
using NeuroGeneticAI.Scoring;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NeuroGeneticAI.Controllers
{
    
    // Represents a simulation slot within the evolutionary grid.
    
    public class SimulationInfo
    {
        public GameObject sim;
        public D31NeuralControler playerRed;
        public D31NeuralControler playerBlue;
        public int individualIndexRed;
        public int individualIndexBlue;

        public SimulationInfo(GameObject sim, D31NeuralControler playerRed, D31NeuralControler playerBlue, int individualIndexRed, int individualIndexBlue)
        {
            this.sim = sim;
            this.playerRed = playerRed;
            this.playerBlue = playerBlue;
            this.individualIndexRed = individualIndexRed;
            this.individualIndexBlue = individualIndexBlue;
        }
    }

    
    // Stores shuffled pairings for the red and blue populations.
    
    public class MatchPair
    {
        public List<int> indexesRed;
        public List<int> indexesBlue;

        public MatchPair(List<int> red, List<int> blue)
        {
            indexesRed = red;
            indexesBlue = blue;
        }
    }

    
    // Coordinates the evolutionary simulation by instantiating arenas, assigning
    // neural networks, and tracking fitness over generations.
    
    public class EvolvingControl : MonoBehaviour
    {
        public static EvolvingControl instance = null;

        [HideInInspector]
        public Text infoText;

        [HideInInspector]
        public bool simulating = false;

        public GameObject simulationPrefab;
        public int SimultaneousSimulations;
        public int RandomSeed;

        private MetaHeuristic metaengine;
        private List<SimulationInfo> simsInfo;
        private bool goNextGen = false;
        private int sims_done = 0;
        private int indiv_index_red = 0;
        private int indiv_index_blue = 0;
        private int pairing = 0;
        private bool allFinished = false;
        private int simulations_index = 0;
        private int totalSimulations = 0;

        [Header("Scenario Conditions")]
        public bool randomRedPlayerPosition = true;
        public bool randomBluePlayerPosition = true;
        public bool randomBallPosition = true;
        public bool movingBall = true;
        public int ChangePositionsEveryNGen;

        protected Vector3 redPlayerStartPosition;
        protected Vector3 ballStartPosition;
        protected Vector3 bluePlayerStartPosition;
        protected List<int> indexesRed;
        protected List<int> indexesBlue;
        protected bool singlePlayer;
        protected string textoUpdate = string.Empty;
        protected List<MatchPair> pairings;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Random.InitState(RandomSeed);
            DontDestroyOnLoad(gameObject);
            InitMetaHeuristic();
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Init();
        }

        private void Init()
        {
            CreateSimulationGrid();
            StartSimulations();
        }

        // Fisher-Yates shuffle used to randomize pairings.
        public void Shuffle(List<int> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = Random.Range(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
        }

        private void InitMetaHeuristic()
        {
            MetaHeuristic[] metaengines = GetComponentsInParent<MetaHeuristic>();
            foreach (MetaHeuristic tmp in metaengines)
            {
                if (tmp.enabled)
                {
                    metaengine = tmp;
                    metaengine.InitPopulation();
                    break;
                }
            }
        }

        private void CreateSimulationGrid()
        {
            pairings = new List<MatchPair>();
            indexesRed = Enumerable.Range(0, metaengine.populationSize).ToList();
            indexesBlue = Enumerable.Range(0, metaengine.populationSize).ToList();

            singlePlayer = simulationPrefab.name.Contains("Solo");
            if (singlePlayer)
            {
                totalSimulations = metaengine.populationSize;
                List<int> toShuffle = new List<int>(indexesRed);
                pairings.Add(new MatchPair(toShuffle, indexesBlue));
            }
            else
            {
                totalSimulations = metaengine.populationSize * metaengine.GamesPerIndividualForEvaluation;
                for (int i = 0; i < metaengine.GamesPerIndividualForEvaluation; i++)
                {
                    List<int> toShuffle = new List<int>(indexesRed);
                    Shuffle(toShuffle);
                    pairings.Add(new MatchPair(toShuffle, indexesBlue));
                }
            }

            simsInfo = new List<SimulationInfo>();
            int ncols = totalSimulations == 1 ? 1 : Mathf.Min(SimultaneousSimulations, 7);
            float spacing = 1.0f / ncols;
            float simHeight = 1f / ncols;
            float startX = 0.0f;
            float startY = 0.0f;

            for (int i = 0; i < SimultaneousSimulations && i < totalSimulations; i++)
            {
                if (i > 0 && i % ncols == 0)
                {
                    startX = 0.0f;
                    startY += simHeight;
                }

                simsInfo.Add(CreateSimulation(simulations_index, new Rect(startX, startY, spacing, simHeight), pairings[pairing].indexesRed[indiv_index_red], pairings[pairing].indexesBlue[indiv_index_blue]));
                startX += spacing;
                simulations_index++;

                if (singlePlayer)
                {
                    indiv_index_red++;
                }
                else
                {
                    if ((indiv_index_blue + 1) % metaengine.populationSize == 0)
                    {
                        indiv_index_blue = 0;
                        indiv_index_red = 0;
                        pairing++;
                    }
                    else
                    {
                        indiv_index_blue++;
                        indiv_index_red++;
                    }
                }
            }
        }

        private SimulationInfo CreateSimulation(int simIndex, Rect location, int indexIndRed, int indexIndBlue)
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

            sim.name = $"Blue {indexIndBlue} vs Red {indexIndRed}";
            sim.transform.Find("Scoring System").gameObject.GetComponent<ScoreKeeper>().setIds(indexIndBlue.ToString(), indexIndRed.ToString());

            if (redPlayerScript != null && redPlayerScript.enabled)
            {
                redPlayerScript.neuralController = metaengine.PopulationRed[indexIndRed].getIndividualController();
            }

            if (bluePlayerScript != null && bluePlayerScript.enabled)
            {
                bluePlayerScript.neuralController = metaengine.PopulationBlue[indexIndBlue].getIndividualController();
            }

            return new SimulationInfo(sim, redPlayerScript, bluePlayerScript, indexIndRed, indexIndBlue);
        }

        private void SetTasks(SimulationInfo info)
        {
            if (randomRedPlayerPosition)
            {
                GameObject p = info.sim.transform.Find("D31-red").gameObject;
                redPlayerStartPosition.y = p.transform.localPosition.y;
                p.transform.localPosition = redPlayerStartPosition;
            }

            if (randomBluePlayerPosition)
            {
                GameObject p = info.sim.transform.Find("D31-blue").gameObject;
                bluePlayerStartPosition.y = p.transform.localPosition.y;
                p.transform.localPosition = bluePlayerStartPosition;
            }

            if (randomBallPosition)
            {
                GameObject p = info.sim.transform.Find("Ball").gameObject;
                ballStartPosition.y = p.transform.localPosition.y;
                p.transform.localPosition = ballStartPosition;
            }

            if (movingBall)
            {
                GameObject p = info.sim.transform.Find("Ball").gameObject;
                Goal goal = info.sim.transform.Find("Field").transform.Find("RedGoal").GetComponent<Goal>();
                goal.initalBallPosition = ballStartPosition;
                goal.ShootTheBallInMyDirection();
            }
        }

        private void StartSimulations()
        {
            if (ChangePositionsEveryNGen > 0 && metaengine.generation % ChangePositionsEveryNGen == 0)
            {
                if (randomBallPosition)
                {
                    ballStartPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(-8, 8));
                }

                if (randomRedPlayerPosition)
                {
                    redPlayerStartPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(10, 20));
                }

                if (randomBluePlayerPosition)
                {
                    bluePlayerStartPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(-26, -16));
                }
            }

            if (metaengine.generation != 0 && (!singlePlayer || randomBallPosition || randomRedPlayerPosition || randomBluePlayerPosition))
            {
                metaengine.ResetBestOverall();
            }

            foreach (SimulationInfo info in simsInfo)
            {
                SetTasks(info);
                info.playerRed.running = true;
                if (info.playerBlue != null)
                {
                    info.playerBlue.running = true;
                }
            }

            simulating = true;
            sims_done = 0;
        }

        private void FixedUpdate()
        {
            if (allFinished)
            {
                return;
            }

            if (simulating)
            {
                for (int i = 0; i < simsInfo.Count; i++)
                {
                    if (simsInfo[i] != null && !simsInfo[i].playerRed.running && simsInfo[i].playerRed.gameOver && (simsInfo[i].playerBlue == null || (!simsInfo[i].playerBlue.running && simsInfo[i].playerBlue.gameOver)))
                    {
                        AssignFitness(i);
                        Rect rect = new Rect(simsInfo[i].sim.GetComponentInChildren<Camera>().rect);
                        DestroyImmediate(simsInfo[i].sim);
                        if (simulations_index < totalSimulations)
                        {
                            simsInfo[i] = CreateSimulation(i, rect, pairings[pairing].indexesRed[indiv_index_red], pairings[pairing].indexesBlue[indiv_index_blue]);
                            SetTasks(simsInfo[i]);
                            if (simsInfo[i].playerRed != null)
                            {
                                simsInfo[i].playerRed.running = true;
                            }

                            if (simsInfo[i].playerBlue != null)
                            {
                                simsInfo[i].playerBlue.running = true;
                            }

                            if ((indiv_index_blue + 1) % metaengine.populationSize == 0)
                            {
                                indiv_index_red = 0;
                                indiv_index_blue = 0;
                                pairing++;
                            }
                            else
                            {
                                indiv_index_blue++;
                                indiv_index_red++;
                            }

                            simulations_index++;
                        }
                        else
                        {
                            simsInfo[i] = null;
                        }

                        sims_done++;
                    }
                }

                infoText.text = textoUpdate;
                if (sims_done == totalSimulations)
                {
                    textoUpdate = $"Generation: {metaengine.generation}/{metaengine.numberOfGenerations}    Simulation: {sims_done}/{totalSimulations}\n" +
                                  $"Current Pop Avg Fitness Red: {metaengine.PopAvgRed} Current Best Red: {metaengine.GenerationBestRed.Fitness}\n" +
                                  $"Current Pop Avg Fitness Blue: {metaengine.PopAvgBlue} Current Best Blue: {metaengine.GenerationBestBlue.Fitness}";
                    simsInfo.Clear();
                    goNextGen = true;
                    simulating = false;
                }
            }
        }

        private void AssignFitness(int index)
        {
            if (simsInfo[index].playerRed != null && !metaengine.PopulationRed[simsInfo[index].individualIndexRed].Evaluated)
            {
                metaengine.PopulationRed[simsInfo[index].individualIndexRed].SetEvaluations(simsInfo[index].playerRed.GetScoreRed());
            }

            if (simsInfo[index].playerBlue != null && !metaengine.PopulationBlue[simsInfo[index].individualIndexBlue].Evaluated)
            {
                metaengine.PopulationBlue[simsInfo[index].individualIndexBlue].SetEvaluations(simsInfo[index].playerBlue.GetScoreBlue());
            }
        }

        private void Update()
        {
            if (goNextGen)
            {
                goNextGen = false;
                if (metaengine.generation < metaengine.numberOfGenerations)
                {
                    metaengine.Step();
                    sims_done = 0;
                    indiv_index_red = 0;
                    indiv_index_blue = 0;
                    simulations_index = 0;
                    pairing = 0;
                    Init();
                }
                else if (!allFinished)
                {
                    allFinished = true;
                    simulating = false;
                    metaengine.updateReport();
                    metaengine.dumpStats();
                    if (infoText != null)
                    {
                        infoText.text = "All Done!";
                    }
                }
            }
        }
    }
}
