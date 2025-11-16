using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Base individual representation used by all evolutionary algorithms in the
    // project.  An individual stores the flattened neural network weights (genotype)
    // along with fitness bookkeeping.
    
    public abstract class Individual
    {
        protected float[] genotype;
        protected MetaHeuristic.MutationType mutation;
        protected int[] topology;
        protected int totalSize = 0;
        protected float fitness;
        protected List<float> evaluations;
        protected bool evaluated;
        protected NeuralNetwork network;

        [HideInInspector]
        protected int completedEvaluations = 0;

        [HideInInspector]
        protected int maxNumberOfEvaluations = 1;

        // Gets the total number of genes within the individual.
        public int Size => totalSize;

        // Gets the latest averaged fitness value.
        public float Fitness => fitness;

        // Gets or sets a gene by index.
        public float this[int i]
        {
            get => genotype[i];
            set => genotype[i] = value;
        }

        // Returns true once all evaluations have been completed.
        public bool Evaluated => evaluated;

        protected Individual(int[] topology, int numberOfEvaluations, MetaHeuristic.MutationType mutation)
        {
            for (int i = 1; i < topology.Length; i++)
            {
                totalSize += topology[i - 1] * topology[i];
            }

            this.topology = topology;
            fitness = 0.0f;
            maxNumberOfEvaluations = numberOfEvaluations;
            evaluations = new List<float>(numberOfEvaluations);
            evaluated = false;
            completedEvaluations = 0;
            genotype = new float[totalSize];
            this.mutation = mutation;
        }

        // Records a new quality evaluation and averages results.
        // quality: Fitness contribution of the current simulation.
        public void SetEvaluations(float quality)
        {
            evaluations.Insert(completedEvaluations, quality);
            completedEvaluations++;
            if (completedEvaluations == maxNumberOfEvaluations)
            {
                completedEvaluations = 0;
                fitness = evaluations.Average();
                evaluated = true;
            }
        }

        // Materializes a neural network instance that matches the genotype.
        public NeuralNetwork getIndividualController()
        {
            network = new NeuralNetwork(topology);
            network.map_from_linear(genotype);
            return network;
        }

        
        public override string ToString()
        {
            if (network == null)
            {
                getIndividualController();
            }

            string res = "[GeneticIndividual]: [";
            for (int i = 0; i < totalSize; i++)
            {
                res += genotype[i].ToString();
                if (i != totalSize - 1)
                {
                    res += ",";
                }
            }

            res += "]\n";
            res += "Neural Network\n" + network + "\n";
            return res;
        }

        // Generates a normally distributed random value.
        public static float NextGaussian(float mean, float standardDeviation)
        {
            return mean + NextGaussian() * standardDeviation;
        }

        // Generates a normally distributed random value with zero mean.
        public static float NextGaussian()
        {
            float v1, v2, s;
            do
            {
                v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            }
            while (s >= 1.0f || s == 0f);

            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
            return v1 * s;
        }

        // Initializes the genotype with random weights.
        public abstract void Initialize();

        // Initializes the genotype using an existing neural network.
        public abstract void Initialize(NeuralNetwork nn);

        // Applies mutation to the genotype.
        public abstract void Mutate(float probability);

        // Crossover routine implemented by derived individuals.
        public abstract void Crossover(Individual partner, float probability);

        // Creates a deep copy of the individual.
        public abstract Individual Clone();
    }
}
