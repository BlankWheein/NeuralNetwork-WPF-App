using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace New_WPF_APP.NN
{
    [Serializable]
    public class DeepNeuralNetwork
    {
        Random rnd = new Random();
        public List<int> structure {  get; set; }
        public float[][] values { get; set; }
        public float[][] biases { get; set; }
        public float[][][] weights { get; set; }

        private float[][] desiredValues { get; set; }
        private float[][] biasesSmudge { get; set; }
        private float[][][] weightsSmudge { get; set; }
        public int EpochsTrained {  get; set; }

        private float WeightDecay { get; set; }
        private float LearningRate { get; set; }


        private int _correct = 0;
        private int _total = 0;
        public int ErrorRate;

        private int _i, _j, _k;

        public ActivationFunctions Activation {  get; set; }
        public ActivationFunctions DActivation { get; set; }

        private float ActivationFunction(float x)
        {
            Func<float, float> func = GetActivationFunction(Activation);
            return func(x);
        }
        private float DActivationFunction(float x)
        {
            Func<float, float> func = GetActivationFunction(DActivation);
            return func(x);
        }
        private Func<float, float> GetActivationFunction(ActivationFunctions function)
        {
            switch (function)
            {
                case ActivationFunctions.Sigmoid:
                    return Sigmoid;
                case ActivationFunctions.DSigmoid:
                    return DSigmoid;
            }
            return Sigmoid;
        }
        private void Constructor(List<int> structure)
        {
            this.structure = structure;
            Activation = ActivationFunctions.Sigmoid;
            DActivation = ActivationFunctions.DSigmoid;
            WeightDecay = 0.001f;
            _i = 0;
            _j = 0;
            _k = 0;
            EpochsTrained = 0;
            LearningRate = 0.1f;
            values = new float[structure.Count][];
            desiredValues = new float[structure.Count][];
            biases = new float[structure.Count][];
            biasesSmudge = new float[structure.Count][];
            weights = new float[structure.Count - 1][][];
            weightsSmudge = new float[structure.Count - 1][][];

            for (var i = 0; i < structure.Count; i++)
            {
                values[i] = new float[structure[i]];
                desiredValues[i] = new float[structure[i]];
                biases[i] = new float[structure[i]];
                biasesSmudge[i] = new float[structure[i]];
            }
            for (var i = 0; i < structure.Count - 1; i++)
            {
                weights[i] = new float[values[i + 1].Length][];
                weightsSmudge[i] = new float[values[i + 1].Length][];
                for (var j = 0; j < weights[i].Length; j++)
                {
                    weights[i][j] = new float[values[i].Length];
                    weightsSmudge[i][j] = new float[values[i].Length];
                    for (var k = 0; k < weights[i][j].Length; k++)
                        weights[i][j][k] = (float)rnd.NextDouble() * MathF.Sqrt(2f / weights[i][j].Length);
                }
            }
        }
        public DeepNeuralNetwork(List<int> structure)
        {
            this.structure = structure;
            Constructor(structure);
        }
        public void ReConstruct(List<int> newStructure)
        {
            Constructor(newStructure);
        }
        public float NextWeight()
        {
            float value = weights[_i][_j][_k];
            _k++;
            if (_k==weights[_i][_j].Length)
            {
                _j++;
                _k = 0;
                if (_j == weights[_i].Length)
                {
                    _i++;
                    _j = 0;
                    _k = 0;
                    if (_i == weights.Length)
                    {
                        _i = 0;
                        _j = 0;
                        _k = 0;
                    }
                }
            }

            return value;
        }
        public string GetInfo()
        {
            string info = "";
            for (int i = 0; i < values.Length; i++)
            {
                info += $"{values[i].Length}-";
            }
            info += $"{LearningRate}";
            return info;
        }
        public float[] Predict(float[] input)
        {
            for (var i = 0; i < values[0].Length; i++) values[0][i] = input[i];

            for (var i = 1; i < values.Length; i++)
                for (var j = 0; j < values[i].Length; j++)
                {
                    values[i][j] = ActivationFunction(Sum(values[i - 1], weights[i - 1][j]) + biases[i][j]);
                    desiredValues[i][j] = values[i][j];
                }
            return values[values.Length - 1];
        }
        private static float Sum(IEnumerable<float> values, IReadOnlyList<float> weights) =>
        values.Select((v, i) => v * weights[i]).Sum();
        private static float Sigmoid (float x) => 1f / (1f + (float) Math.Exp(-x));
        private static float DSigmoid(float x) => x * (1 - x);
        private static float HardSigmoid(float x)
        {
            if (x < -2.5f) { return 0f; }
            if (x > 2.5f) { return 1f; }
            return 0.2f * x + 0.5f;
        }
        public void Train(float[][] trainingInputs, float[][] trainingOutputs)
        {
            if (_total > 1000)
            {
                _total = 0;
                _correct = 0;
            }
            for (var i = 0; i < trainingInputs.Length; i++)
            {
                _total++;
                int randInt = rnd.Next(0, trainingInputs.Length);
                Predict(trainingInputs[randInt]);
                float highestValue = 0;
                int highestIndex = 0;
                for (int g = 0; g < values[values.Length - 1].Length; g++)
                {
                    if (values[values.Length - 1][g] > highestValue)
                    {
                        highestIndex = g;
                        highestValue = values[values.Length - 1][g];
                    }
                }
                if (trainingOutputs[randInt][highestIndex] == 1)
                {
                    _correct++;
                }

                for (var j = 0; j < desiredValues[desiredValues.Length - 1].Length; j++)
                    desiredValues[desiredValues.Length - 1][j] = trainingOutputs[randInt][j];

                for (var j = values.Length - 1; j >= 1; j--)
                {
                    for (var k = 0; k < values[j].Length; k++)
                    {
                        var biasSmudge = DActivationFunction(values[j][k]) *
                                         (desiredValues[j][k] - values[j][k]);
                        biasesSmudge[j][k] += biasSmudge;

                        for (var l = 0; l < values[j - 1].Length; l++)
                        {
                            var weightSmudge = values[j - 1][l] * biasSmudge;
                            weightsSmudge[j - 1][k][l] += weightSmudge;

                            var valueSmudge = weights[j - 1][k][l] * biasSmudge;
                            desiredValues[j - 1][l] += valueSmudge;
                        }
                    }
                }
            }

            for (var i = values.Length - 1; i >= 1; i--)
            {
                for (var j = 0; j < values[i].Length; j++)
                {
                    biases[i][j] += biasesSmudge[i][j] * LearningRate;
                    biases[i][j] *= 1 - WeightDecay;
                    biasesSmudge[i][j] = 0;
                    for (var k = 0; k < values[i - 1].Length; k++)
                    {
                        weights[i - 1][j][k] += weightsSmudge[i - 1][j][k] * LearningRate;
                        weights[i - 1][j][k] *= 1 - WeightDecay;
                        weightsSmudge[i - 1][j][k] = 0;
                    }
                    desiredValues[i][j] = 0;
                }
            }
            EpochsTrained++;
            CalculateError();
        }

        private void CalculateError()
        {
            ErrorRate = (int)(((float)_correct / (float)_total) * 100);
            if (ErrorRate > 90)
            {
                if (ErrorRate > 95)
                {
                    LearningRate = 1f / ErrorRate/2;
                    WeightDecay = 0;
                } else
                {
                    LearningRate = 1f / ErrorRate;
                }
                
            }
            
        }
        public void Serialize(string name)
        {
            var neural = this;
            string fileName = name + ".json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(neural, options);
            File.WriteAllText(fileName, jsonString);
        }
        public static DeepNeuralNetwork DeSerialize(string name)
        {
            string filename = name + ".json";
            string text = System.IO.File.ReadAllText(filename);
            System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
            DeepNeuralNetwork nn = JsonSerializer.Deserialize<DeepNeuralNetwork>(text);
            return nn;
        }
    }

}
