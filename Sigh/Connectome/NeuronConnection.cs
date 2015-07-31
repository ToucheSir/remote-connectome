namespace Connectome
{
    public struct NeuronConnection
    {
        Neuron target;
        int weight;

        public NeuronConnection(Neuron target, int weight)
        {
            this.target = target;
            this.weight = weight;
        }
        // TODO everything

        public void SendSynapse()
        {
            target.ReceiveSynapse(weight);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", target.name, weight);
        }
    }
}
