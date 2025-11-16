using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeuroGeneticAI.Genetics
{
    
    // Lightweight fully connected neural network used as the controller genome.
    
    [Serializable]
    public class NeuralNetwork
    {
        public float[][][] weights;
        public int[] parameters;
        public int networkSize;

        private void initializeVariables()
        {
            weights = new float[parameters.Length - 1][][];
            networkSize = parameters.Length;
        }

        // Creates an empty network with the specified topology.
        public NeuralNetwork(int[] parameters)
        {
            this.parameters = parameters;
            initializeVariables();
        }

        // Creates a random network with the specified topology.
        public NeuralNetwork(int[] parameters, int random)
        {
            this.parameters = parameters;
            initializeVariables();
            for (int i = 0; i < parameters.Length - 1; i++)
            {
                weights[i] = new float[parameters[i]][];
                for (int j = 0; j < parameters[i]; j++)
                {
                    weights[i][j] = new float[parameters[i + 1]];
                    for (int k = 0; k < parameters[i + 1]; k++)
                    {
                        weights[i][j][k] = getRandomWeight();
                    }
                }
            }
        }

        // Maps the supplied genotype into the weight matrices.
        public void map_from_linear(float[] geno)
        {
            int counter = 0;
            for (int i = 0; i < parameters.Length - 1; i++)
            {
                weights[i] = new float[parameters[i]][];
                for (int j = 0; j < parameters[i]; j++)
                {
                    weights[i][j] = new float[parameters[i + 1]];
                    for (int k = 0; k < parameters[i + 1]; k++)
                    {
                        weights[i][j][k] = geno[counter++];
                    }
                }
            }
        }

        // Feeds the supplied inputs through the network and returns the outputs.
        public float[] process(float[] inputs)
        {
            if (inputs.Length != parameters[0])
            {
                Debug.Log("Input lenght does not match the number of neurons in the input layer!");
                return Array.Empty<float>();
            }

            float[] outputs;
            for (int i = 0; i < networkSize - 1; i++)
            {
                outputs = new float[parameters[i + 1]];
                for (int j = 0; j < inputs.Length; j++)
                {
                    for (int k = 0; k < outputs.Length; k++)
                    {
                        outputs[k] += inputs[j] * weights[i][j][k];
                    }
                }

                inputs = new float[outputs.Length];
                for (int l = 0; l < outputs.Length; l++)
                {
                    inputs[l] = Mathf.Max((float)(0.01 * outputs[l]), outputs[l]);
                }
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = (float)Math.Tanh(inputs[i]);
            }

            return inputs;
        }

        
        public override string ToString()
        {
            string output = string.Empty;
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    output += "Weights Layer " + i + "\n";
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        output += weights[i][j][k] + " ";
                    }

                    output += "\n";
                }
            }

            return output;
        }

        private float getRandomWeight()
        {
            return Random.Range(-0.5f, 0.5f);
        }
    }
}
