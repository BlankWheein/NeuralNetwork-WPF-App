using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace New_WPF_APP.NN
{
    [Serializable]
    public class NeuralNetwork
    {
        public int input_nodes { get; set; }
        public int hidden_nodes { get; set; }
        public int output_nodes { get; set; }

        public Matrix weights_ih { get; set; }
        public Matrix weights_ho { get; set; }

        public Matrix bias_h { get; set; }
        public Matrix bias_o { get; set; }

        public double learning_rate { get; set; }
        public string ActivationFunction { get; set; }
        public string DerivativeActivationFunction { get; set; }

        public List<List<double>> FakeInputs { get; set; }
        public List<List<double>> FakeOutputs { get; set; }
        public int EpochsTrained { get; set; }

        private Random rand;


        // double nn


        public NeuralNetwork(int input_nodes, int hidden_nodes, int output_nodes)
        {
            this.input_nodes = input_nodes;
            this.hidden_nodes = hidden_nodes;
            this.output_nodes = output_nodes;

            this.weights_ih = new(this.hidden_nodes, this.input_nodes);
            this.weights_ho = new(this.output_nodes, this.hidden_nodes);
            this.weights_ho.Randomize();
            this.weights_ih.Randomize();

            this.bias_h = new(this.hidden_nodes, 1);
            this.bias_o = new(this.output_nodes, 1);
            this.bias_h.Randomize();
            this.bias_o.Randomize();
            this.learning_rate = 0.1;
            SetActivationFunction("sigmoid");
            SetDervativeActivationFunction("dsigmoid");
            rand = new();
            FakeInputs = new();
            FakeOutputs = new();
        }
        public void InitFakeTrainer(int data_size)
        {
            for (int j = 0; j < data_size; j++)
            {
                List<double> input_arr = new();
                for (int i = 0; i < input_nodes; i++)
                {
                    input_arr.Add(rand.NextDouble() * 2 - 1);
                }
                FakeInputs.Add(input_arr);

                List<double> output_arr = new();
                for (int i = 0; i < output_nodes; i++)
                {
                    output_arr.Add(rand.Next(-1, 1));
                }
                FakeOutputs.Add(output_arr);
            }
        }
        public void FakeTrainer(int data_size, int epochs)
        {
                

            for (int e = 0; e < epochs; e++)
            {
                for (int i = 0; i < data_size; i++)
                {
                    Train(FakeInputs[i], FakeOutputs[i]);
                }
                EpochsTrained++;
            }
            Mutate();
            

        }
        public Func<double, double> Activation(string func)
        {
            if (func == "sigmoid")
            {
                return Matrix.Sigmoid;
            }
            else if (func == "dsigmoid")
            {
                return Matrix.DSigmoid;
            }

            return Matrix.Sigmoid;
        }
        public void SetActivationFunction(string func)
        {
            ActivationFunction = func;
        }
        public void SetDervativeActivationFunction(string func)
        {
            DerivativeActivationFunction = func;
        }
        public void Mutate()
        {
            this.weights_ho.Mutate();
            this.weights_ih.Mutate();
            this.bias_h.Mutate();
            this.bias_o.Mutate();
        }

        public List<double> Predict(List<double> input_arr)
        {
            Matrix inputs = Matrix.FromArray(input_arr);
            Matrix hidden = Matrix.Multiply(this.weights_ih, inputs);
            hidden.Add(this.bias_h);

            hidden.Map(Activation(ActivationFunction));

            Matrix output = Matrix.Multiply(this.weights_ho, hidden);
            output.Add(this.bias_o);
            output.Map(Activation(ActivationFunction));

            return output.ToArray();
        }

        public double GetAccuracyFromIrisTest(List<Iris> set)
        {
            double total = 0;
            double correct = 0;
            for (int i = 0; i < set.Count; i++)
            {
                Iris x = set[i];
                List<double> prediction = Predict(x.Pixels);
                int highestOutputidx = 0;
                double highestOutputivalue = -1;
                for (int j = 0; j < prediction.Count; j++)
                {
                    if (prediction[j] > highestOutputivalue)
                    {
                        highestOutputivalue = prediction[j];
                        highestOutputidx = j;
                    }
                }
                total++;
                if (x.LabetInt == highestOutputidx)
                {
                    correct++;
                }
            }
            Console.WriteLine("Accuracy: {0}%, {1} OF {2}", (correct / total) * 100, correct, total);
            return (correct / total) * 100;
        }
        public void TrainEpochs(List<Iris> set, int epochs)
        {
            for (int i = 0; i < epochs; i++)
            {
                foreach (Iris data in set)
                {
                    Train(data.Pixels, data.LabelList);
                }
                EpochsTrained++;
            }
        }
        public void Train(List<double> input_arr, List<double> target_arr)
        {

            // Generate Hidden Outputs
            Matrix inputs = Matrix.FromArray(input_arr);
            Matrix hidden = Matrix.Multiply(this.weights_ih, inputs);
            hidden.Add(this.bias_h);
            hidden.Map(Activation(ActivationFunction));

            Matrix output = Matrix.Multiply(this.weights_ho, hidden);
            output.Add(this.bias_o);
            output.Map(Activation(ActivationFunction));

            Matrix targets = Matrix.FromArray(target_arr);
            Matrix output_error = Matrix.Subtract(targets, output);

            Matrix gradients = Matrix.Map(output, Activation(DerivativeActivationFunction));
            gradients.Multiply(output_error);
            gradients.Multiply(learning_rate);

            Matrix hidden_t = Matrix.Transpose(hidden);
            Matrix weight_ho_deltas = Matrix.Multiply(gradients, hidden_t);

            this.weights_ho.Add(weight_ho_deltas);
            this.bias_o.Add(gradients);

            Matrix who_t = Matrix.Transpose(this.weights_ho);
            Matrix hidden_errors = Matrix.Multiply(who_t, output_error);

            Matrix hidden_gradient = Matrix.Map(hidden, Activation(DerivativeActivationFunction));
            hidden_gradient.Multiply(hidden_errors);
            hidden_gradient.Multiply(learning_rate);

            Matrix inputs_T = Matrix.Transpose(inputs);
            Matrix weight_ih_deltas = Matrix.Multiply(hidden_gradient, inputs_T);

            this.weights_ih.Add(weight_ih_deltas);

            this.bias_h.Add(hidden_gradient);

        }


        public void Print()
        {
            Console.WriteLine("{0}-{1}-{2}", this.input_nodes, this.hidden_nodes, this.output_nodes);
            Console.WriteLine("{0}-{1}", this.weights_ih, this.weights_ho);
        }
        public void PrintPrediction(List<double> input_arr)
        {
            List<double> prediction = Predict(input_arr);
            string message = "";
            int highestOutputidx = 0;
            double highestOutputivalue = -1;
            for (int i = 0; i < prediction.Count; i++)
            {
                if (prediction[i] > highestOutputivalue)
                {
                    highestOutputivalue = prediction[i];
                    highestOutputidx = i;
                }
                message += String.Format("{0}, ", prediction[i]);
            }
            //Console.WriteLine("Prediction: {0}", message);
            Console.WriteLine("Prediction: {0}", IrisReader.ConvertIntToLabel(highestOutputidx));
        }

        public void Serialize(string name)
        {
            var neural = this;
            string fileName = name + ".json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(neural, options);
            File.WriteAllText(fileName, jsonString);
        }
        public static NeuralNetwork DeSerialize(string name)
        {
            string filename = name + ".json";
            string text = System.IO.File.ReadAllText(filename);
            System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
            NeuralNetwork nn = JsonSerializer.Deserialize<NeuralNetwork>(text);
            return nn;
        }
    }
}
