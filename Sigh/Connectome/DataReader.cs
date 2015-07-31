using System.Collections.Generic;
using System.IO;

namespace Connectome
{
    class DataReader
    {
        private readonly string neuronFile;
        private readonly string connectionFile;

        public DataReader(string neuronFile, string connectionFile)
        {
            this.neuronFile = neuronFile;
            this.connectionFile = connectionFile;
        }

        public void ReadData(Dictionary<string, Neuron> neurons)
        {
            using (StreamReader reader = File.OpenText(neuronFile))
            {
                while (!reader.EndOfStream)
                {
                    var name = reader.ReadLine();
                    neurons.Add(name, new Neuron(name));
                }
            }

            using (StreamReader reader = File.OpenText(connectionFile))
            {
                while (!reader.EndOfStream)
                {
                    var lineContents = reader.ReadLine().Split(',');
                    var name = lineContents[0];
                    var target = lineContents[1];
                    var weight = int.Parse(lineContents[2]);

                    if (!neurons.ContainsKey(target))
                    {
                        throw new KeyNotFoundException(target + " not found");
                    }
                    var conn = new NeuronConnection(neurons[target], weight);
                    neurons[name].AddConnection(conn);
                }
            }
        }
    }
}
