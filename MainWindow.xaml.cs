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
        private float StrokeWeight = 0.5f;
        public MainWindow()
        {
            InitializeComponent();
            iris = new();
            nn = new(25, 30, 20);
            UpdateNNButton();
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            InitNetwork();
        }
        public double CalculateTop(int nodes, int index)
        {
            return 50 + (CalculateDimension(nodes) + 5) * (index + 1);
        }
        public double CalculateLeft(double index)
        {
            if (index == 0)
            {
                return 0;
            } else if (index == 1)
            {
                return System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            } else if (index == 3)
            {
                return System.Windows.SystemParameters.PrimaryScreenWidth;
            }
            return 5;
            
        }
        public double CalculateDimension(int nodes)
        {
            double dim = (400 / nodes);
            
            return Math.Min(100, dim);
        }
        private SolidColorBrush setColor(double value)
        {
            value *= 50;
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
            if (B > 255)
            {
                B = 255;
            }
            if (Math.Max((double)G, 0) + Math.Max((double)R, 0) < 25)
            {
                B = (byte)value;
            }

            var brush = new SolidColorBrush(Color.FromArgb(255, R, G, B));
            return brush;
        }
        private void InitNetwork()
        {
            for (int i = 0; i < nn.input_nodes; i++)
            {
                Ellipse l = new();
                l.Height = CalculateDimension(nn.input_nodes);
                l.Width = CalculateDimension(nn.input_nodes);
                l.Stroke = Brushes.Black;
                l.Margin = new Thickness(CalculateLeft(0), CalculateTop(nn.input_nodes, i), 0, 0);
                InputCanvas.Children.Add(l);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            }

            for (int i = 0; i < nn.hidden_nodes; i++)
            {
                Ellipse l = new();
                l.Height = CalculateDimension(nn.hidden_nodes);
                l.Width = CalculateDimension(nn.hidden_nodes);
                l.Stroke = Brushes.Black;
                l.Margin = new Thickness(CalculateLeft(1), CalculateTop(nn.hidden_nodes, i), 0, 0);
                HiddenCanvas.Children.Add(l);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            }

            for (int i = 0; i < nn.output_nodes; i++)
            {
                Ellipse l = new();
                l.Height = CalculateDimension(nn.output_nodes);
                l.Width = CalculateDimension(nn.output_nodes);
                l.Stroke = Brushes.Black;
                l.Margin = new Thickness(CalculateLeft(2), CalculateTop(nn.output_nodes, i), 0, 0);
                OutputCanvas.Children.Add(l);
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            }

        }
        public void Draw()
        {
            List<Line> lines = new();
            foreach (Line l in LineCanvas.Children.OfType<Line>())
            {
                lines.Add(l);
            }
            for (int ii = 0; ii < lines.Count; ii++)
            {
                LineCanvas.Children.Remove(lines[ii]);
            }

            int i = 0;
            int j;
            foreach (Ellipse inp in InputCanvas.Children.OfType<Ellipse>())
            {
                j = 0;
                foreach (Ellipse hid in HiddenCanvas.Children.OfType<Ellipse>())
                {
                    Line line = new Line();
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = StrokeWeight;
                    line.Stroke = setColor(nn.weights_ih.values[j][i]);
                    line.X1 = (double)CalculateLeft(0) + (CalculateDimension(nn.input_nodes) / 2);
                    line.Y1 = (double)CalculateTop(nn.input_nodes, i) + (CalculateDimension(nn.input_nodes) / 2);

                    line.X2 = (double)CalculateLeft(1) + (CalculateDimension(nn.hidden_nodes) / 2);
                    line.Y2 = (double)CalculateTop(nn.hidden_nodes, j) + (CalculateDimension(nn.hidden_nodes) / 2);


                    LineCanvas.Children.Add(line);
                    j++;
                }

                i++;
            }
            i = 0;
            foreach (Ellipse hid in HiddenCanvas.Children.OfType<Ellipse>())
            {
                j = 0;
                foreach (Ellipse output in OutputCanvas.Children.OfType<Ellipse>())
                {
                    Line line = new Line();
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = StrokeWeight;
                    line.Stroke = setColor(nn.weights_ih.values[i][j]);
                    line.X1 = (double)CalculateLeft(1) + (CalculateDimension(nn.hidden_nodes) / 2);
                    line.Y1 = (double)CalculateTop(nn.hidden_nodes, i) + (CalculateDimension(nn.hidden_nodes) / 2);

                    line.X2 = (double)CalculateLeft(2) + (CalculateDimension(nn.output_nodes) / 2);
                    line.Y2 = (double)CalculateTop(nn.output_nodes, j) + (CalculateDimension(nn.output_nodes) / 2);


                    LineCanvas.Children.Add(line);
                    j++;
                }

                i++;
            }
        }
        public void UpdateNNButton()
        {
            NNButton.Content = String.Format("{0}-{1}-{2} by {3}", nn.input_nodes, nn.hidden_nodes, nn.output_nodes, nn.learning_rate);
        }
        private void Create_Neural_Network(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            Draw();
        }
        private void Train_1_epoch()
        {
            nn.InitFakeTrainer(100);
            for (int i = 0; i < 1000000000; i++)
            {
                
                nn.FakeTrainer(100, 10);
                trainer.Content = String.Format("Training {0} * 10", i);
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
