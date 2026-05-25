using System;

namespace FormulaGaussExample
{
    public class BalanzaMatricial
    {
        private double[] coeficientes;

        public bool EstaCalibrado => coeficientes != null && coeficientes.Length == 4;

        public double[] ObtenerCoeficientes()
        {
            return coeficientes;
        }

        public void EstablecerCoeficientes(double[] coefs)
        {
            if (coefs == null || coefs.Length != 4)
                throw new ArgumentException("Debe proporcionar 4 coeficientes.");
            coeficientes = coefs;
        }

        public void Calibrar(double[,] lecturas, double pesoPatron)
        {
            if (lecturas == null)
                throw new ArgumentNullException(nameof(lecturas));
            if (lecturas.GetLength(0) != 4 || lecturas.GetLength(1) != 4)
                throw new ArgumentException("La matriz debe ser 4x4 (4 posiciones x 4 celdas).");
            if (pesoPatron <= 0)
                throw new ArgumentException("El peso patrón debe ser mayor a cero.");

            double[,] matrizA = new double[4, 4];
            double[] vectorB = new double[4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrizA[i, j] = lecturas[i, j];
                }
                vectorB[i] = pesoPatron;
            }

            coeficientes = ResolverGaussJordan(matrizA, vectorB);
        }

        public double ObtenerPesoCorregido(double[] lecturasActuales)
        {
            if (!EstaCalibrado)
                throw new InvalidOperationException("La balanza no está calibrada. Ejecute Calibrar primero.");
            if (lecturasActuales == null || lecturasActuales.Length != 4)
                throw new ArgumentException("Debe proporcionar las lecturas de las 4 celdas.");

            double peso = 0;
            for (int i = 0; i < 4; i++)
            {
                peso += coeficientes[i] * lecturasActuales[i];
            }
            return peso;
        }

        private static double[] ResolverGaussJordan(double[,] matrizA, double[] vectorB)
        {
            int n = 4;
            double[,] aumentada = new double[n, n + 1];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    aumentada[i, j] = matrizA[i, j];
                }
                aumentada[i, n] = vectorB[i];
            }

            for (int col = 0; col < n; col++)
            {
                int maxFila = col;
                for (int fila = col + 1; fila < n; fila++)
                {
                    if (Math.Abs(aumentada[fila, col]) > Math.Abs(aumentada[maxFila, col]))
                        maxFila = fila;
                }

                for (int j = col; j <= n; j++)
                {
                    double temp = aumentada[col, j];
                    aumentada[col, j] = aumentada[maxFila, j];
                    aumentada[maxFila, j] = temp;
                }

                double pivote = aumentada[col, col];
                if (Math.Abs(pivote) < 1e-12)
                    throw new InvalidOperationException("La matriz es singular o mal condicionada.");

                for (int j = col; j <= n; j++)
                {
                    aumentada[col, j] /= pivote;
                }

                for (int fila = 0; fila < n; fila++)
                {
                    if (fila != col)
                    {
                        double factor = aumentada[fila, col];
                        for (int j = col; j <= n; j++)
                        {
                            aumentada[fila, j] -= factor * aumentada[col, j];
                        }
                    }
                }
            }

            double[] solucion = new double[n];
            for (int i = 0; i < n; i++)
            {
                solucion[i] = aumentada[i, n];
            }
            return solucion;
        }
    }
}
