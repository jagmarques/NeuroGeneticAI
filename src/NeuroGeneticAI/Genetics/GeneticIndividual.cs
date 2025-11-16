using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Concrete individual used by the genetic algorithm; exposes mutation and
    // crossover implementations tailored for neural network weights.
    
    public class GeneticIndividual : Individual
    {
        public GeneticIndividual(int[] topology, int numberOfEvaluations, MutationType mutation)
            : base(topology, numberOfEvaluations, mutation)
        {
        }

        
        public override void Initialize()
        {
            for (int i = 0; i < totalSize; i++)
            {
                genotype[i] = Random.Range(-1.0f, 1.0f);
            }
        }

        
        public override void Initialize(NeuralNetwork nn)
        {
            int count = 0;
            for (int i = 0; i < topology.Length - 1; i++)
            {
                for (int j = 0; j < topology[i]; j++)
                {
                    for (int k = 0; k < topology[i + 1]; k++)
                    {
                        genotype[count++] = nn.weights[i][j][k];
                    }
                }
            }
        }

        
        public override Individual Clone()
        {
            GeneticIndividual clone = new GeneticIndividual(topology, maxNumberOfEvaluations, mutation);
            genotype.CopyTo(clone.genotype, 0);
            clone.fitness = Fitness;
            clone.evaluated = false;
            return clone;
        }

        
        public override void Mutate(float probability)
        {
            switch (mutation)
            {
                case MetaHeuristic.MutationType.Gaussian:
                    MutateGaussian(probability);
                    break;
                case MetaHeuristic.MutationType.Random:
                    MutateRandom(probability);
                    break;
            }
        }

        // Replaces genes with random values using a flat probability.
        public void MutateRandom(float probability)
        {
            for (int i = 0; i < totalSize; i++)
            {
                if (Random.Range(0.0f, 1.0f) < probability)
                {
                    genotype[i] = Random.Range(-1.0f, 1.0f);
                }
            }
        }

        // Applies Gaussian noise to each gene with the provided probability.
        public void MutateGaussian(float probability)
        {
            const float mean = 0;
            const float stdev = 0.5f;
            for (int i = 0; i < totalSize; i++)
            {
                if (Random.Range(0.0f, 1.0f) < probability)
                {
                    genotype[i] += NextGaussian(mean, stdev);
                }
            }
        }

        
        public override void Crossover(Individual partner, float probability)
        {
            if (Random.Range(0.0f, 1.0f) >= probability)
            {
                return;
            }

            int point = Random.Range(1, totalSize - 1);
            GeneticIndividual other = (GeneticIndividual)partner;
            for (int i = 0; i < point; i++)
            {
                float temp = genotype[i];
                genotype[i] = other.genotype[i];
                other.genotype[i] = temp;
            }
        }
    }
}
