using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace New_WPF_APP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NN.DeepNeuralNetwork nn;
        public NN.IrisReader iris;
        private float StrokeWeight = 0.75f;
        public int EpochsPerIteration = 10;
        public MainWindow()
        {
            InitializeComponent();
            iris = new();
            nn = new NN.DeepNeuralNetwork(new List<int>() { 4, 250,250, 3 });
            //nn = NN.DeepNeuralNetwork.DeSerialize("NN");

            //UpdateNNButton();
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            InitNetwork();
            float[] prediction = nn.Predict(iris.TestSetArray.Item1[0]);
            float[] CorrectLabel = iris.TestSetArray.Item2[0];
            UpdateNNButton();
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
            } else if (value < -1)
            {
                R = valuePercent;
            } else
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
                    if (i == 0 || i == nn.values.Length-1)
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
            for (int i = 0; i < 1000000000; i++)
            {
                for (int j = 0; j < EpochsPerIteration;  j++)
                {
                    nn.Train(iris.TrainingSetArray.Item1, iris.TrainingSetArray.Item2);
                }
                nn.Serialize("NN");
                trainer.Content = String.Format("{1}-{0}", nn.ErrorRate, nn.EpochsTrained);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                Draw();
                UpdateNNButton();

            }
        }
    }
    }
