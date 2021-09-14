using System;
using System.Collections.Generic;
using System.IO;

namespace New_WPF_APP.NN
{
    public class MnistReader
    {
        private const string TrainImages = "../../../mnist/train-images.idx3-ubyte";
        private const string TrainLabels = "../../../mnist/train-labels.idx1-ubyte";
        private const string TestImages = "../../../mnist/t10k-images.idx3-ubyte";
        private const string TestLabels = "../../../mnist/t10k-labels.idx1-ubyte";

        public (float[][], float[][]) TrainingSetArray;
        public (float[][], float[][]) TestSetArray;
        public List<Image> TrainingSet;
        public List<Image> TestSet;
        public MnistReader()
        {
            TrainingSet = new();
            TestSet = new();
            foreach (Image img in ReadTrainingData())
            {
                TrainingSet.Add(img);
            }
            foreach (Image img in ReadTestData())
            {
                TestSet.Add(img);
            }
            TrainingSetArray = (new float[TrainingSet.Count][], new float[TrainingSet.Count][]);
            TestSetArray = (new float[TestSet.Count][], new float[TestSet.Count][]);
            for (int i = 0; i < TrainingSet.Count; i++)
            {
                TrainingSetArray.Item1[i] = TrainingSet[i].PixelsArray;
                TrainingSetArray.Item2[i] = TrainingSet[i].LabelList;
            }
            for (int i = 0; i < TestSet.Count; i++)
            {
                TestSetArray.Item1[i] = TestSet[i].PixelsArray;
                TestSetArray.Item2[i] = TestSet[i].LabelList;
            }
        }
        public static IEnumerable<Image> ReadTrainingData()
        {
            foreach (var item in Read(TrainImages, TrainLabels))
            {
                yield return item;
            }
        }

        public static IEnumerable<Image> ReadTestData()
        {
            foreach (var item in Read(TestImages, TestLabels))
            {
                yield return item;
            }
        }

        private static IEnumerable<Image> Read(string imagesPath, string labelsPath)
        {
            BinaryReader labels = new BinaryReader(new FileStream(labelsPath, FileMode.Open));
            BinaryReader images = new BinaryReader(new FileStream(imagesPath, FileMode.Open));

            int magicNumber = images.ReadBigInt32();
            int numberOfImages = images.ReadBigInt32();
            int width = images.ReadBigInt32();
            int height = images.ReadBigInt32();

            int magicLabel = labels.ReadBigInt32();
            int numberOfLabels = labels.ReadBigInt32();

            for (int i = 0; i < numberOfImages; i++)
            {
                var bytes = images.ReadBytes(width * height);
                var arr = new byte[height, width];

                arr.ForEach((j, k) => arr[j, k] = bytes[j * height + k]);

                List<double> pixels = new();
                float[] pixelsArray = new float[arr.Length];
                int k = 0;
                foreach (var pixel in arr)
                {
                    pixels.Add(Convert.ToDouble(pixel) / 255);
                    pixelsArray[k] = (float)Convert.ToDouble(pixel) / 255;
                    k++;
                }
                
                byte label = labels.ReadByte();
                yield return new Image()
                {
                    Data = arr,
                    Label = label,
                    Pixels = pixels,
                    LabelList = ConvertLabelToArray(label),
                    PixelsArray = pixelsArray

                };
            }
        }
        
        private static float[] ConvertLabelToArray(byte label)
        {
            float[] label_list = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            label_list[label] = 1;
            return label_list;
        }
    }
    
    public class Image
    {
        public byte Label { get; set; }
        public byte[,] Data { get; set; }
        public List<double> Pixels { get; set; }
        public float[] LabelList { get; set; }
        public float[] PixelsArray {  get; set; }
    }
    public static class Extensions
    {
        public static int ReadBigInt32(this BinaryReader br)
        {
            var bytes = br.ReadBytes(sizeof(Int32));
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static void ForEach<T>(this T[,] source, Action<int, int> action)
        {
            for (int w = 0; w < source.GetLength(0); w++)
            {
                for (int h = 0; h < source.GetLength(1); h++)
                {
                    action(w, h);
                }
            }
        }
    }
}
