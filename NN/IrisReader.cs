using System;
using System.Collections.Generic;

namespace New_WPF_APP.NN
{
    public class IrisReader
    {
        private const string irisData = "iris/iris.data";
        public List<Iris> TrainingSet;
        public List<Iris> TestSet;
        public IrisReader()
        {
            List<Iris> iris_training = new();
            List<Iris> iris_test = new();
            foreach (Iris x in Read())
            {
                iris_training.Add(x);
            }

            int pointer = 0;
            for (int i = 0; i < 30; i++)
            {
                iris_test.Add(iris_training[pointer]);
                iris_training.RemoveAt(pointer);
                pointer += 1;
                if (i % 10 == 0 && i != 0)
                {
                    pointer += 40;
                }
            }
            TrainingSet = iris_training;
            TestSet = iris_test;
        }
        public static IEnumerable<Iris> Read()
        {
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
            new System.IO.StreamReader(irisData);
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split(',');
                if (words[0] != "")
                {
                    yield return new Iris()
                    {
                        LabelString = words[4],
                        LabetInt = ConvertLabelToInt(words[4]),
                        Pixels = new List<double>() { Convert.ToDouble(words[0]) / 10, Convert.ToDouble(words[1]) / 10, Convert.ToDouble(words[2]) / 10, Convert.ToDouble(words[3]) / 10 },
                        LabelList = ConvertLabelToList(ConvertLabelToInt(words[4]))
                    };
                }

            }
            file.Close();
        }
        public static int ConvertLabelToInt(string label)
        {
            if (label == "Iris-setosa")
            {
                return 0;
            }
            if (label == "Iris-versicolor")
            {
                return 1;
            }
            if (label == "Iris-virginica")
            {
                return 2;
            }
            return 3;

        }
        public static string ConvertIntToLabel(int label)
        {
            if (label == 0)
            {
                return "Iris-setosa";
            }
            if (label == 1)
            {
                return "Iris-versicolor";
            }
            if (label == 2)
            {
                return "Iris-virginica";
            }
            return "None";

        }
        private static List<double> ConvertLabelToList(int label)
        {
            List<double> label_list = new() { 0, 0, 0 };
            label_list[label] = 1;
            return label_list;
        }
    }

    public class Iris
    {
        public string LabelString { get; set; }
        public int LabetInt { get; set; }
        public List<double> Pixels { get; set; }
        public List<double> LabelList { get; set; }
    }
}
