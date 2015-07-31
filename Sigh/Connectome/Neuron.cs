using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Connectome
{
    public class Neuron
    {
        public readonly string name;
        public int value { get; private set; }
        private bool fired;
        private List<NeuronConnection> connections;

        public Neuron(string name, int value = 0)
        {
            this.name = name;
            this.value = value;
            fired = false;
            connections = new List<NeuronConnection>();
        }

        public void Fire()
        {
            foreach (var conn in connections)
            {
                conn.SendSynapse();
            }
        }

        public void Reset()
        {
            fired = false;
        }

        public void ClearSynapse()
        {
            value = 0;
        }

        public void ReceiveSynapse(int weight)
        {
            if (!fired)
            {
                value += weight;
                fired = true;
                // Console.WriteLine(name + ", " + value);
            }
        }

        public void AddConnection(NeuronConnection nc)
        {
            connections.Add(nc);
        }

        public static bool IsMuscle(string name)
        {
            // FIXME find more elegant way to replace regex.
            return name.Equals("MVULVA") || Regex.IsMatch(name, @"^M[VD][LR](0[0-9]|1[0-9]|2[0-4])$");
        }

        public override string ToString()
        {
            return string.Format("Neuron({0})[{1}, {2}] -> [{3}]", name, value, fired, string.Join(", ", connections));
        }
    }
}
