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
        public NN.NeuralNetwork nn;
        public NN.IrisReader iris;
        public MainWindow()
        {
            InitializeComponent();
            iris = new();
        }

        private SolidColorBrush setColor(double value)
        {
            value *= 15;
            byte R = 0;
            byte G = 0;
            byte B = 0;
            if (value > 0)
            {
                G = (byte)value;
            } else
            {
                value *= -1;
                R = (byte)value;
            }

            if (R > 255)
            {
                R = 255;
            }
            if (G > 255)
            {
                G = 255;
            }

            var brush = new SolidColorBrush(Color.FromArgb(255, R, G, B));
            return brush;
        }
        public void Draw()
        {
            List<Ellipse> input = new() { i0, i1, i2, i3 };
            List<Ellipse> hidden = new() { h0, h1, h2, h3 };
            List<Ellipse> output = new() { o0, o1, o2 };
            List<Line> lines = new();
            foreach (Line l in myCanvas.Children.OfType<Line>())
            {
                lines.Add(l);
            }
            for (int i = 0; i < lines.Count; i++)
            {
                myCanvas.Children.Remove(lines[i]);
            }


            for (int i = 0; i < nn.input_nodes; i++)
            {
                Ellipse inp = input[i];
                for (int j = 0; j < nn.hidden_nodes; j++)
                {
                    Ellipse hid = hidden[j];
                    Line line = new Line();
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = 2;
                    line.Stroke = setColor(nn.weights_ih.values[i][j]);
                    line.X1 = (double)inp.GetValue(Canvas.LeftProperty) + 40;
                    line.Y1 = (double)inp.GetValue(Canvas.TopProperty) + 40;

                    line.X2 = (double)hid.GetValue(Canvas.LeftProperty) + 40;
                    line.Y2 = (double)hid.GetValue(Canvas.TopProperty) + 40;


                    myCanvas.Children.Add(line);

                }
            }

            for (int i = 0; i < nn.hidden_nodes; i++)
            {
                Ellipse hid = hidden[i];
                for (int j = 0; j < nn.output_nodes; j++)
                {
                    Ellipse outp = output[j];
                    Line line = new Line();
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = 2;
                    line.Stroke = setColor(nn.weights_ih.values[i][j]);
                    line.X1 = (double)hid.GetValue(Canvas.LeftProperty) + 40;
                    line.Y1 = (double)hid.GetValue(Canvas.TopProperty) + 40;

                    line.X2 = (double)outp.GetValue(Canvas.LeftProperty) + 40;
                    line.Y2 = (double)outp.GetValue(Canvas.TopProperty) + 40;


                    myCanvas.Children.Add(line);

                }
            }
        }
        private void Create_Neural_Network(object sender, RoutedEventArgs e)
        {
            nn = new(4, 4, 3);
            ((Button)sender).Content = String.Format("{0}-{1}-{2} by {3}", nn.input_nodes, nn.hidden_nodes, nn.output_nodes, nn.learning_rate);
            Draw();
        }
        private void Train_1_epoch()
        {
            for (int i = 0; i < 10000; i++)
            {
                nn.TrainEpochs(iris.TrainingSet, 100);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                Draw();
            }
        }
        private void Train_NN(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Content = "Training";
            Train_1_epoch();
            ((Button)sender).Content = "Done";
            


        }
    }
}
