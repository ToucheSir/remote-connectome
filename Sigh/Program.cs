using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using MonoBrick.NXT;
using Connectome;

namespace Sigh
{
    class MainClass
    {
        private static Dictionary<string, Neuron> neurons = new Dictionary<string, Neuron>();
        private static int accumLeft = 0;
        private static int accumRight = 0;

        private const int THRESHOLD = 30;

        private static readonly string[] musDleft =
        { "MDL07", "MDL08", "MDL09", "MDL10", "MDL11",
            "MDL12", "MDL13", "MDL14", "MDL15", "MDL16", "MDL17", "MDL18", "MDL19", "MDL20",
            "MDL21", "MDL22", "MDL23"
        };
        private static readonly string[] musDright =
        { "MDR07", "MDR08", "MDR09", "MDR10", "MDR11",
            "MDR12", "MDR13", "MDR14", "MDR15", "MDR16", "MDR17", "MDR18", "MDR19", "MDR20",
            "MDL21", "MDR22", "MDR23"
        };
        private static readonly string[] musVleft =
        { "MVL07", "MVL08", "MVL09", "MVL10", "MVL11",
            "MVL12", "MVL13", "MVL14", "MVL15", "MVL16", "MVL17", "MVL18", "MVL19", "MVL20",
            "MVL21", "MVL22", "MVL23"
        };
        private static readonly string[] musVright =
        { "MVR07", "MVR08", "MVR09", "MVR10", "MVR11",
            "MVR12", "MVR13", "MVR14", "MVR15", "MVR16", "MVR17", "MVR18", "MVR19", "MVR20",
            "MVL21", "MVR22", "MVR23"
        };

		private static Brick<Sensor, Sensor, Sensor, Sensor> wormBrick;

        public static void Main(string[] args)
        {
            double tFood = 0;
			wormBrick = new Brick<Sensor, Sensor, Sensor, Sensor> ("usb");
			wormBrick.Connection.Open ();
			wormBrick.Sensor1 = new Sonar ();

			wormBrick.Vehicle.LeftPort = MotorPort.OutA;
			wormBrick.Vehicle.RightPort = MotorPort.OutB;
			wormBrick.Vehicle.ReverseLeft = false;  
			wormBrick.Vehicle.ReverseRight = false; 

            CreatePostSynaptic(neurons);

            for (; ;)
            {
				//var dist = ((Sonar)wormBrick.Sensor1).ReadDistance();
				var dist = 0;
                if (dist > 0 && dist < 30)
                {
                    Console.WriteLine("OBSTACLE (Nose Touch) " + dist);
                    DendriteAccumulate("FLPR");
                    DendriteAccumulate("FLPL");
                    DendriteAccumulate("ASHL");
                    DendriteAccumulate("ASHR");
                    DendriteAccumulate("IL1VL");
                    DendriteAccumulate("IL1VR");
                    DendriteAccumulate("OLQDL");
                    DendriteAccumulate("OLQDR");
                    DendriteAccumulate("OLQVR");
                    DendriteAccumulate("OLQVL");

                    RunConnectome();
                }
                else
                {
                    if (tFood < 2)
                    {
                        DendriteAccumulate("ADFL");
                        DendriteAccumulate("ADFR");
                        DendriteAccumulate("ASGR");
                        DendriteAccumulate("ASGL");
                        DendriteAccumulate("ASIL");
                        DendriteAccumulate("ASIR");
                        DendriteAccumulate("ASJR");
                        DendriteAccumulate("ASJL");

                        Thread.Sleep(500);
                        RunConnectome();
                    }

                    tFood += 0.5;
                    if (tFood > 20)
                    {
                        tFood = 0;
                    }
                }
            }

		}

        private static void CreatePostSynaptic(Dictionary<string, Neuron> neurons)
        {
            var neuronReader = new DataReader("connectome_neurons.csv", "connectome_connections.csv");
            neuronReader.ReadData(neurons);

            foreach (var name in neurons.Keys)
            {
                Console.WriteLine(string.Format("{0}: {1}", name, neurons[name]));
            }
        }

        private static void RunConnectome()
        {
            foreach (var name in neurons.Keys)
            {
                if (!Neuron.IsMuscle(name) && Math.Abs(neurons[name].value) > THRESHOLD)
                {
                    neurons[name].Fire();
                    neurons[name].ClearSynapse();
                    // TODO reset neuron value
                }

                neurons[name].Reset();
            }

            MotorControl();
        }

        private static void MotorControl()
        {
            foreach (var name in neurons.Keys)
            {
                if (musDleft.Contains(name) || musVleft.Contains(name))
                {
                    Console.WriteLine(name + " is muscle " + neurons[name].value);
                    accumLeft += neurons[name].value;
                    neurons[name].ClearSynapse();
                    // TODO reset neuron value
                }
                else if (musDright.Contains(name) || musVright.Contains(name))
                {
                    Console.WriteLine(name + " is muscle " + neurons[name].value);
                    accumRight += neurons[name].value;
                    neurons[name].ClearSynapse();
                    // TODO reset neuron value
                }
            }

			sbyte speed = (sbyte)(Math.Abs(accumLeft) + Math.Abs(accumRight));
			var turnRatio = (float)accumLeft / (float)accumRight;

			if (speed > sbyte.MaxValue)
            {
				speed = sbyte.MaxValue;
            }
            else
            {
                speed = 75;
            }
				
            Console.WriteLine("Left: {0}, Right: {1}, Speed: {2}", accumLeft, accumRight, speed);

            // TODO movement code
			if (accumLeft == 0 && accumRight == 0) {
				if (turnRatio <= 0.6) {
					// rotate left
					wormBrick.Vehicle.SpinLeft(speed);
					Thread.Sleep (800);
				} else if (turnRatio >= 2) {
					// rotate right
					wormBrick.Vehicle.SpinRight(speed);
					Thread.Sleep (800);
				}

				// backwards
				Thread.Sleep (500);
			} else if (accumRight <= 0 && accumLeft >= 0) {
				// rotate right
				wormBrick.Vehicle.SpinRight(speed);
				Thread.Sleep (800);
			} else if (accumRight >= 0 && accumLeft <= 0) {
				// rotate left
				wormBrick.Vehicle.SpinLeft(speed);
				Thread.Sleep (800);
			} else if (accumRight >= 0 && accumLeft > 0) {
				if (turnRatio <= 0.6) {
					// rotate left
					wormBrick.Vehicle.SpinLeft(speed);
					Thread.Sleep (800);
				} else if (turnRatio >= 2) {
					// rotate right
					wormBrick.Vehicle.SpinRight(speed);
					Thread.Sleep (800);
				}

				// foward
				wormBrick.Vehicle.Forward(speed);
				Thread.Sleep (500);
			} else {
				wormBrick.Vehicle.Brake ();
			}
            // TODO end of movement code

            accumLeft = accumRight = 0;
        }

        private static void DendriteAccumulate(string dneuron)
        {
            neurons[dneuron].Fire();
        }

        private static void FireNeuron(string fneuron)
        {
            if (!fneuron.Equals("MVULVA"))
            {
                neurons[fneuron].Fire();
            }
        }
    }
}
