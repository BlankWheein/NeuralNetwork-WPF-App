using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace New_WPF_APP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : UserControl
    {
        public NN.DeepNeuralNetwork nn;
        public NN.IrisReader iris;
        public NN.MnistReader mnist;
        private float StrokeWeight = 1f;
        public int EpochsPerIteration = 1;
        public int EpochPerGraphUpdater = 1;
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public void initGraph()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Loss",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 5
                },
                new LineSeries
                {
                    Title = "Learning Rate",
                    Values = new ChartValues<double> {  },
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 5
                },
                new LineSeries
                {
                    Title = "Accuracy",
                    Values = new ChartValues<double> {  },
                    PointGeometry = DefaultGeometries.Triangle,
                    PointGeometrySize = 5
                }
            };

            Labels = new List<string>() { $"{nn.EpochsTrained}" };
            YFormatter = val => $"{val}";

            DataContext = this;
        }

        public void UpdateGraph()
        {
            SeriesCollection[0].Values.Add((double)nn.Loss);
            SeriesCollection[1].Values.Add((double)nn.LearningRate);
            SeriesCollection[2].Values.Add((double)nn.Accuracy);
            if (SeriesCollection[0].Values.Count > 25)
            {
                for (int i = 0; i < SeriesCollection.Count; i++)
                {
                    SeriesCollection[i].Values.RemoveAt(0);
                }
                Labels.RemoveAt(0);
            }
            Labels.Add($"{nn.EpochsTrained}");
            DataContext = this;
        }
        public static void Execute()
        {

            CudafyModule km = CudafyModule.TryDeserialize(typeof(MainWindow).Name);
            if (km == null || !km.TryVerifyChecksums())
            {
                km = CudafyTranslator.Cudafy(typeof(MainWindow));
                km.Serialize();
            }
            GPGPU _gpu = CudafyHost.GetDevice(eGPUType.Cuda);
            _gpu.LoadModule(km);
            _gpu.Launch().kernel();


        }
        public MainWindow()
        {
            MainWindow.Execute();
            InitializeComponent();
            iris = new();
            mnist = new();
            nn = new NN.DeepNeuralNetwork(new List<int>() { 784, 24, 10 });
            //nn = NN.DeepNeuralNetwork.DeSerialize("NN");

            //UpdateNNButton();
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            InitNetwork();
            UpdateNNButton();
            initGraph();
        }
        [Cudafy]
        public static void thekernel()
        {

        }
        public static double CalculateTop(int index, int Nodes)
        {
            return CalculateDimension(Nodes) * index * 2.2 + 25;
        }
        public static double CalculateLeft(int index, int Nodes)
        {
            return CalculateDimension(Nodes) * index * 2.4;
        }
        public static double CalculateDimension(int nodes)
        {
            double dim = 1000 / nodes / 2.5;

            return Math.Min(100, dim);
        }
        private SolidColorBrush setColor(double value)
        {
            byte valuePercent;
            byte R = 10;
            byte G = 10;
            byte B = 30;
            value *= 150;
            valuePercent = (byte)(Math.Max(value, -value) / 100 * (255 - 10) + 10);
            if (value > 1)
            {
                G = valuePercent;
            }
            else if (value < -1)
            {
                R = valuePercent;
            }
            else
            {
                B = valuePercent;
            }

            var brush = new SolidColorBrush(Color.FromArgb(255, R, G, B));
            return brush;
        }
        private void InitNetwork()
        {
            for (int i = 0; i < nn.values.Length; i++)
            {
                for (int j = 0; j < nn.values[i].Length; j++)
                {
                    Ellipse l = new();
                    l.Height = CalculateDimension(nn.values[i].Length);
                    l.Width = CalculateDimension(nn.values[i].Length);
                    l.Stroke = Brushes.White;
                    l.Margin = new Thickness(CalculateLeft(i, nn.values.Length), CalculateTop(j, nn.values[i].Length), 0, 0);
                    if (i == 0 || i == nn.values.Length - 1)
                    {
                        //CircleCanvas.Children.Add(l);

                    }
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                }

            }
            Draw();



        }
        private void Draw()
        {
            List<Line> lines = new();
            foreach (Line l in CircleCanvas.Children.OfType<Line>())
            {
                lines.Add(l);
            }
            for (int ii = 0; ii < lines.Count; ii++)
            {
                CircleCanvas.Children.Remove(lines[ii]);
            }
            for (int i = 0; i < nn.values.Length - 1; i++)
            {
                for (int j = 0; j < nn.values[i].Length; j++)
                {
                    int weight = 0;
                    for (int k = 0; k < nn.values[i + 1].Length; k++)
                    {
                        Line line = new Line();
                        line.Visibility = Visibility.Visible;
                        line.StrokeThickness = StrokeWeight;
                        float value = nn.NextWeight();
                        line.Stroke = setColor(value);
                        line.X1 = (double)CalculateLeft(i, nn.values.Length) + CalculateDimension(nn.values[i].Length) / 2;
                        line.Y1 = (double)CalculateTop(j, nn.values[i].Length) + CalculateDimension(nn.values[i].Length) / 2;

                        line.X2 = (double)CalculateLeft(i + 1, nn.values.Length) + CalculateDimension(nn.values[i + 1].Length) / 2;
                        line.Y2 = (double)CalculateTop(k, nn.values[i + 1].Length) + CalculateDimension(nn.values[i + 1].Length) / 2;
                        CircleCanvas.Children.Add(line);
                    }

                }
            }
        }


        public void UpdateNNButton()
        {
            NNButton.Content = nn.GetInfo();
        }

        private void trainer_Click(object sender, RoutedEventArgs e)
        {
            int k = 0;
            UpdateGraph();
            for (int i = 0; i < 1000000000; i++)
            {
                for (int j = 0; j < EpochsPerIteration; j++)
                {
                    k++;
                    nn.Train(mnist.TrainingSetArray.Item1, mnist.TrainingSetArray.Item2);
                    if (k == EpochPerGraphUpdater)
                    {
                        UpdateGraph();
                        k = 0;
                    }
                }
                nn.Serialize("NN");
                trainer.Content = String.Format("{1}-{0}", nn.ErrorRate, nn.EpochsTrained);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                Draw();
                UpdateNNButton();
                nn.CalculateAccuracy(mnist.TrainingSetArray.Item1, mnist.TrainingSetArray.Item2);

            }
        }

        private void SetLR(object sender, RoutedEventArgs e)
        {
            nn.LearningRate = (float)Convert.ToDouble(LR.Text);
        }
    }
}
