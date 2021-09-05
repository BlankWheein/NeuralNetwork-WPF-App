using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace New_WPF_APP.NN
{
    public class Matrix
    {
        #region Members
        public int Rows { get; set; }
        public int Columns { get; set; }
        public double[][] values { get; set; }
        [JsonIgnore]
        private Random rand;
        #endregion
        #region Constructor
        public Matrix(int Rows, int Columns)
        {
            rand = new Random();
            ConstructMatrix(Rows, Columns);
            ZeroMatrix();
        }
        public void ConstructMatrix(int Rows, int Columns)
        {
            this.Rows = Rows;
            this.Columns = Columns;
            values = new double[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                values[i] = new double[Columns];
            }
        }
        public void ZeroMatrix()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] = 0;
                }
            }
        }
        #endregion
        #region Static Functions
        public static Matrix FromArray(List<double> arr)
        {
            Matrix matrix = new Matrix(arr.Count, 1);
            for (int i = 0; i < arr.Count; i++)
            {
                matrix.values[i][0] = arr[i];
            }
            return matrix;
        }
        public static double Sigmoid(double value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }

        public static double DSigmoid(double x)
        {
            //return Sigmoid(x) * (-1 * Sigmoid(x));
            return x * (1 - x);
        }

        public static double Relu(double x)
        {
            return Math.Max(0, x);
        }
        public static double SoftPlus(double x)
        {
            return Math.Log(Math.Exp(x) + 1);
        }

        public static int RoundIt(double x)
        {
            if (x >= 0.5)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static List<int> RountIt(List<double> x)
        {
            List<int> res = new();
            foreach (double value in x)
            {
                if (value >= 0.5)
                {
                    res.Add(1);
                }
                else
                {
                    res.Add(0);
                }
            }
            return res;
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
            {
                Console.WriteLine("Columns of A does not match Rows of B");
                return new Matrix(1, 1);
            }

            Matrix result = new Matrix(a.Rows, b.Columns);
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    for (int k = 0; k < a.Columns; k++)
                    {
                        result.values[i][j] += a.values[i][k] * b.values[k][j];
                    }
                }
            }
            return result;
        }

        public static Matrix Transpose(Matrix m)
        {
            Matrix result = new Matrix(m.Columns, m.Rows);
            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Columns; j++)
                {
                    result.values[j][i] = m.values[i][j];
                }
            }
            return result;
        }

        public static Matrix Subtract(Matrix a, Matrix b)
        {
            Matrix matrix = new Matrix(a.Rows, a.Columns);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    matrix.values[i][j] = a.values[i][j] - b.values[i][j];
                }
            }
            return matrix;
        }

        public static Matrix Map(Matrix m, Func<double, double> function_)
        {
            Matrix result = new Matrix(m.Rows, m.Columns);
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    double value = function_(m.values[i][j]);
                    result.values[i][j] = value;
                }
            }
            return result;
        }
        #endregion
        #region Methods

        public void Randomize()
        {
            Map(RandomizeHelper);
        }
        private double RandomizeHelper(double x)
        {
            double value = (rand.NextDouble() * 2) - 1;
            return value;
        }
        public void Mutate()
        {
            this.Map(MutateHelper);
        }
        private double MutateHelper(double x)
        {
            double value = ((rand.NextDouble() * 2) - 1) * 0.1;
            return x + value;
        }

        public IEnumerable<double> ReadMatrix()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    yield return values[i][j];
                }
            }
        }

        public void SetMatrix(double value)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] = value;
                }
            }
        }

        public List<double> ToArray()
        {
            List<double> arr = new();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    arr.Add(values[i][j]);
                }
            }
            return arr;
        }

        // Hadamard Product
        public void Multiply(Matrix n)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] *= n.values[i][j];
                }
            }
        }

        // Scalar Product
        public void Multiply(double n)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] *= n;
                }
            }
        }


        public void Add(double x)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] += x;
                }
            }
        }

        public void Add(Matrix m)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] += m.values[i][j];
                }
            }
        }

        public Matrix Copy()
        {
            Matrix a = new Matrix(Rows, Columns);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    a.values[i][j] = values[i][j];
                }
            }
            return a;
        }

        public void Subtract(Matrix b)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    values[i][j] -= b.values[i][j];
                }
            }
        }

        public void Map(Func<double, double> function_)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    double value = function_(values[i][j]);
                    values[i][j] = value;
                }
            }
        }

        public void Print()
        {
            Console.WriteLine("Rows: {0}, Columns: {1}", Rows, Columns);
            for (int i = 0; i < Rows; i++)
            {
                string value = "| ";
                for (int j = 0; j < Columns; j++)
                {
                    value = value + String.Format("{0} | ", values[i][j]);
                }
                Console.WriteLine(value);
            }
        }
        #endregion
    }
}
