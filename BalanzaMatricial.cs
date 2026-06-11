using System;

namespace FormulaGaussExample
{
    public class BalanzaMatricial
    {
        private double[] coeficientes;

        /// <summary>Indica si la balanza tiene coeficientes de calibración cargados.</summary>
        public bool EstaCalibrado => coeficientes != null && coeficientes.Length == 4;

        /// <summary>Devuelve los 4 coeficientes de corrección matricial.</summary>
        public double[] ObtenerCoeficientes()
        {
            return coeficientes;
        }

        /// <summary>Establece los 4 coeficientes de corrección matricial.</summary>
        /// <param name="coefs">Arreglo de 4 coeficientes double.</param>
        public void EstablecerCoeficientes(double[] coefs)
        {
            if (coefs == null || coefs.Length != 4)
                throw new ArgumentException("Debe proporcionar 4 coeficientes.");
            coeficientes = coefs;
        }

        /// <summary>Realiza la calibración resolviendo el sistema matricial 4x4.</summary>
        /// <param name="lecturas">Matriz 4x4 con lecturas de las 4 celdas en 4 posiciones.</param>
        /// <param name="pesoPatron">Peso patrón conocido colocado en la balanza.</param>
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

        /// <summary>Calcula el peso corregido aplicando los coeficientes matriciales.</summary>
        /// <param name="lecturasActuales">Lecturas actuales de las 4 celdas.</param>
        /// <returns>Peso corregido en kg.</returns>
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

        /// <summary>Resuelve un sistema 4x4 usando el método de Gauss-Jordan con pivoteo parcial.</summary>
        /// <param name="matrizA">Matriz de coeficientes 4x4.</param>
        /// <param name="vectorB">Vector de términos independientes de tamaño 4.</param>
        /// <returns>Arreglo con la solución del sistema.</returns>
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
