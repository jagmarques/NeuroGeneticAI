using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Individual used by the hill climber metaheuristic.  Mutations are intentionally
    // conservative to mimic a local search strategy.
    
    public class HillClimberIndividual : Individual
    {
        public HillClimberIndividual(int[] topology, int numberOfEvaluations, MetaHeuristic.MutationType mutation)
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

        // Random reset mutation similar to the GA version.
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

        // Applies gentle Gaussian noise to the genotype.
        public void MutateGaussian(float probability)
        {
            const float mean = 0;
            const float stdev = 0.2f;
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
            // Hill climbing typically operates on a single solution, so crossover is a no-op.
        }

        
        public override Individual Clone()
        {
            HillClimberIndividual clone = new HillClimberIndividual(topology, maxNumberOfEvaluations, mutation);
            genotype.CopyTo(clone.genotype, 0);
            clone.fitness = Fitness;
            clone.evaluated = false;
            clone.completedEvaluations = 0;
            return clone;
        }
    }
}
