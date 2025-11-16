using NeuroGeneticAI.Genetics;
using Xunit;

namespace NeuroGeneticAI.Tests
{
    public class NeuralNetworkTests
    {
        [Fact]
        public void Process_RespectsTopology()
        {
            var net = new NeuralNetwork(new[] { 2, 2, 1 }, random: 0);
            net.map_from_linear(new[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f });
            var result = net.process(new[] { 1f, 1f });
            Assert.Single(result);
        }
    }
}
