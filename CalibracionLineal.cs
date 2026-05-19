using System;
using System.Collections.Generic;
using System.Text;

namespace FormulaGaussExample
{
    /// <summary>
    /// Representa un punto de calibración: las lecturas raw de las 4 celdas
    /// y el peso conocido colocado en la báscula.
    /// </summary>
    internal class PuntoCalibracion
    {
        /// <summary>Lectura raw de la celda 1.</summary>
        public double X1 { get; set; }
        /// <summary>Lectura raw de la celda 2.</summary>
        public double X2 { get; set; }
        /// <summary>Lectura raw de la celda 3.</summary>
        public double X3 { get; set; }
        /// <summary>Lectura raw de la celda 4.</summary>
        public double X4 { get; set; }
        /// <summary>Peso conocido colocado en la báscula (kg).</summary>
        public double PesoConocido { get; set; }

        public override string ToString()
        {
            return $"X1={X1:F1}  X2={X2:F1}  X3={X3:F1}  X4={X4:F1}  ->  Peso={PesoConocido:F1} kg";
        }
    }

    /// <summary>
    /// Motor de calibración lineal multivariable.
    /// Resuelve un sistema de 5 ecuaciones con 5 incógnitas (m1, m2, m3, m4, B)
    /// usando el método de eliminación de Gauss con pivoteo parcial.
    /// 
    /// Ecuación del sistema:
    ///   PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
    /// 
    /// Los coeficientes m1..m4 representan la contribución de cada celda al peso total,
    /// y B es el bias (offset) del sistema.
    /// </summary>
    internal class CalibracionLineal
    {
        /// <summary>Coeficientes de calibración para cada celda (m1, m2, m3, m4).</summary>
        public double[] Coeficientes { get; private set; }

        /// <summary>Bias (offset) del sistema.</summary>
        public double Bias { get; private set; }

        /// <summary>Indica si la calibración se ha realizado correctamente.</summary>
        public bool EstaCalibrado { get; private set; } = false;

        /// <summary>Número de celdas del sistema (4).</summary>
        public const int NumeroCeldas = 4;

        /// <summary>
        /// Inicializa una nueva instancia con coeficientes por defecto (sin calibrar).
        /// </summary>
        public CalibracionLineal()
        {
            Coeficientes = new double[NumeroCeldas];
            Bias = 0;
        }

        /// <summary>
        /// Inicializa el calibrador con coeficientes ya conocidos.
        /// </summary>
        /// <param name="m1">Coeficiente de la celda 1.</param>
        /// <param name="m2">Coeficiente de la celda 2.</param>
        /// <param name="m3">Coeficiente de la celda 3.</param>
        /// <param name="m4">Coeficiente de la celda 4.</param>
        /// <param name="b">Bias del sistema.</param>
        public CalibracionLineal(double m1, double m2, double m3, double m4, double b)
        {
            Coeficientes = new double[] { m1, m2, m3, m4 };
            Bias = b;
            EstaCalibrado = true;
        }

        /// <summary>
        /// Calcula el peso total a partir de las lecturas raw de las 4 celdas.
        /// PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
        /// </summary>
        /// <param name="x1">Lectura raw de la celda 1.</param>
        /// <param name="x2">Lectura raw de la celda 2.</param>
        /// <param name="x3">Lectura raw de la celda 3.</param>
        /// <param name="x4">Lectura raw de la celda 4.</param>
        /// <returns>Peso calculado en kg.</returns>
        public double PesoCalculado(double x1, double x2, double x3, double x4)
        {
            if (!EstaCalibrado)
                throw new InvalidOperationException("El sistema no está calibrado. Ejecute Calibrar() primero.");

            return x1 * Coeficientes[0] +
                   x2 * Coeficientes[1] +
                   x3 * Coeficientes[2] +
                   x4 * Coeficientes[3] +
                   Bias;
        }

        /// <summary>
        /// Calcula cuánto debe valer CADA UNA de las 4 celdas (X1=X2=X3=X4=X)
        /// para que el peso calculado sea igual al deseado.
        /// 
        /// Despeje: peso_deseado = X*(m1+m2+m3+m4) + B
        ///              X = (peso_deseado - B) / (m1+m2+m3+m4)
        /// </summary>
        /// <param name="pesoDeseado">Peso objetivo en kg.</param>
        /// <returns>Valor de X (lectura raw igual para las 4 celdas) redondeado a 2 decimales.</returns>
        public double PesoIgualParaTodasLasCeldas(double pesoDeseado)
        {
            if (!EstaCalibrado)
                throw new InvalidOperationException("El sistema no está calibrado.");

            double sumaCoeficientes = 0;
            for (int i = 0; i < NumeroCeldas; i++)
                sumaCoeficientes += Coeficientes[i];

            if (Math.Abs(sumaCoeficientes) < 1e-15)
                throw new DivideByZeroException("La suma de los coeficientes m es cero. No se puede resolver.");

            double x = (pesoDeseado - Bias) / sumaCoeficientes;
            return Math.Round(x, 2);
        }

        /// <summary>
        /// Resuelve el sistema de 5 ecuaciones con 5 incógnitas usando
        /// eliminación de Gauss con pivoteo parcial.
        /// 
        /// Matriz de 5x6 (5 coeficientes + 1 término independiente por ecuación):
        /// [X1_1, X2_1, X3_1, X4_1, 1,  Peso_1]
        /// [X1_2, X2_2, X3_2, X4_2, 1,  Peso_2]
        /// [X1_3, X2_3, X3_3, X4_3, 1,  Peso_3]
        /// [X1_4, X2_4, X3_4, X4_4, 1,  Peso_4]
        /// [X1_5, X2_5, X3_5, X4_5, 1,  Peso_5]
        /// 
        /// Las incógnitas son [m1, m2, m3, m4, B].
        /// </summary>
        /// <param name="puntos">Lista de 5 puntos de calibración.</param>
        /// <returns>True si la calibración fue exitosa.</returns>
        public bool Calibrar(List<PuntoCalibracion> puntos)
        {
            if (puntos == null || puntos.Count < 5)
                throw new ArgumentException("Se necesitan al menos 5 puntos de calibración.", nameof(puntos));

            int n = 5; // 4 coeficientes + 1 bias = 5 incógnitas

            // Construir matriz aumentada de 5x6
            double[,] matrizAumentada = new double[n, n + 1];

            for (int i = 0; i < 5; i++)
            {
                matrizAumentada[i, 0] = puntos[i].X1;    // coeficiente de m1
                matrizAumentada[i, 1] = puntos[i].X2;    // coeficiente de m2
                matrizAumentada[i, 2] = puntos[i].X3;    // coeficiente de m3
                matrizAumentada[i, 3] = puntos[i].X4;    // coeficiente de m4
                matrizAumentada[i, 4] = 1.0;              // coeficiente de B (siempre 1)
                matrizAumentada[i, 5] = puntos[i].PesoConocido; // término independiente
            }

            // --- ELIMINACIÓN HACIA ADELANTE (con pivoteo parcial) ---
            for (int col = 0; col < n; col++)
            {
                // Pivoteo parcial: encontrar la fila con el mayor valor absoluto en esta columna
                int filaMax = col;
                for (int fila = col + 1; fila < n; fila++)
                {
                    if (Math.Abs(matrizAumentada[fila, col]) > Math.Abs(matrizAumentada[filaMax, col]))
                        filaMax = fila;
                }

                // Intercambiar filas si es necesario
                if (filaMax != col)
                {
                    for (int j = col; j <= n; j++)
                    {
                        double temp = matrizAumentada[col, j];
                        matrizAumentada[col, j] = matrizAumentada[filaMax, j];
                        matrizAumentada[filaMax, j] = temp;
                    }
                }

                // Verificar que el pivote no sea cero
                if (Math.Abs(matrizAumentada[col, col]) < 1e-15)
                    throw new InvalidOperationException(
                        "El sistema no tiene solución única (pivote cero). " +
                        "Verifique que los puntos de calibración sean linealmente independientes.");

                // Eliminar los elementos debajo del pivote
                for (int fila = col + 1; fila < n; fila++)
                {
                    double factor = matrizAumentada[fila, col] / matrizAumentada[col, col];
                    for (int j = col; j <= n; j++)
                    {
                        matrizAumentada[fila, j] -= factor * matrizAumentada[col, j];
                    }
                }
            }

            // --- SUSTITUCIÓN HACIA ATRÁS ---
            double[] solucion = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                solucion[i] = matrizAumentada[i, n];
                for (int j = i + 1; j < n; j++)
                {
                    solucion[i] -= matrizAumentada[i, j] * solucion[j];
                }
                solucion[i] /= matrizAumentada[i, i];
            }

            // Almacenar la solución
            for (int i = 0; i < NumeroCeldas; i++)
                Coeficientes[i] = solucion[i];

            Bias = solucion[4];
            EstaCalibrado = true;

            return true;
        }

        /// <summary>
        /// Verifica la calibración calculando el peso para cada punto de calibración
        /// y mostrando el error absoluto.
        /// </summary>
        /// <param name="puntos">Lista de puntos de calibración usados.</param>
        /// <returns>Cadena con la tabla de verificación.</returns>
        public string VerificarCalibracion(List<PuntoCalibracion> puntos)
        {
            if (!EstaCalibrado || puntos == null)
                return "Sistema no calibrado.";

            var sb = new StringBuilder();
            sb.AppendLine("=== VERIFICACIÓN DE CALIBRACIÓN ===");
            sb.AppendLine();
            sb.AppendLine("  Peso esperado  |  Peso calculado  |  Error");
            sb.AppendLine("  -------------- | ---------------- | ------");

            double errorMaximo = 0;
            for (int i = 0; i < puntos.Count && i < 5; i++)
            {
                double calculado = PesoCalculado(puntos[i].X1, puntos[i].X2, puntos[i].X3, puntos[i].X4);
                double error = Math.Abs(calculado - puntos[i].PesoConocido);
                errorMaximo = Math.Max(errorMaximo, error);

                sb.AppendLine($"  {puntos[i].PesoConocido,12:F1}  |  {calculado,14:F10}  |  {error,10:E2}");
            }

            sb.AppendLine();
            sb.AppendLine($"Error máximo absoluto: {errorMaximo:E2}");

            // Mostrar la ecuación final
            sb.AppendLine();
            sb.AppendLine("=== ECUACIÓN DEL SISTEMA ===");
            sb.AppendLine($"PESO = X1*{Coeficientes[0]:F10} + X2*{Coeficientes[1]:F10} + " +
                         $"X3*{Coeficientes[2]:F10} + X4*{Coeficientes[3]:F10} + {Bias:F10}");

            return sb.ToString();
        }

        /// <summary>
        /// Calcula el valor de X para que todas las celdas tengan la misma lectura
        /// y el peso total sea el deseado. Muestra la comprobación.
        /// </summary>
        /// <param name="pesoDeseado">Peso objetivo en kg.</param>
        /// <returns>Cadena con el resultado y la comprobación.</returns>
        public string CalcularXPesoIgual(double pesoDeseado)
        {
            if (!EstaCalibrado)
                return "Sistema no calibrado.";

            double sumaM = 0;
            for (int i = 0; i < NumeroCeldas; i++)
                sumaM += Coeficientes[i];

            double x = (pesoDeseado - Bias) / sumaM;
            double xRedondeado = Math.Round(x, 2);
            double comprobacion = xRedondeado * sumaM + Bias;

            var sb = new StringBuilder();
            sb.AppendLine($"=== PESO IGUAL PARA TODAS LAS CELDAS ===");
            sb.AppendLine($"Peso deseado: {pesoDeseado} kg");
            sb.AppendLine();
            sb.AppendLine($"Suma de coeficientes (m1+m2+m3+m4) = {sumaM:F10}");
            sb.AppendLine($"Bias (B) = {Bias:F10}");
            sb.AppendLine();
            sb.AppendLine($"X = (pesoDeseado - B) / suma(m)");
            sb.AppendLine($"X = ({pesoDeseado} - ({Bias:F10})) / {sumaM:F10}");
            sb.AppendLine($"X = {x:F10}");
            sb.AppendLine();
            sb.AppendLine($"X redondeado a 2 decimales = {xRedondeado:F2}");
            sb.AppendLine();
            sb.AppendLine($"COMPROBACIÓN:");
            sb.AppendLine($"Peso = {xRedondeado:F2} * {sumaM:F10} + ({Bias:F10})");
            sb.AppendLine($"Peso = {comprobacion:F2} kg");
            sb.AppendLine($"Error: {Math.Abs(comprobacion - pesoDeseado):E2}");

            return sb.ToString();
        }

        /// <summary>
        /// Entrega un resumen completo de la calibración: matriz aumentada original,
        /// matriz escalonada, solución y verificación.
        /// </summary>
        /// <param name="puntos">Puntos de calibración utilizados.</param>
        /// <returns>Cadena con el informe completo.</returns>
        public string GenerarInforme(List<PuntoCalibracion> puntos)
        {
            if (!EstaCalibrado || puntos == null || puntos.Count < 5)
                return "No hay datos de calibración para generar un informe.";

            var sb = new StringBuilder();
            sb.AppendLine("==========================================");
            sb.AppendLine("  INFORME DE CALIBRACIÓN MULTIVARIABLE");
            sb.AppendLine("==========================================");
            sb.AppendLine();
            sb.AppendLine("Ecuación del sistema:");
            sb.AppendLine("  PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B");
            sb.AppendLine();

            // Matriz aumentada original
            sb.AppendLine("1. MATRIZ AUMENTADA ORIGINAL (5x6):");
            sb.AppendLine("  [ X1    X2    X3    X4    1  |  Peso ]");
            for (int i = 0; i < 5; i++)
            {
                sb.AppendLine($"  [{puntos[i].X1,5:F1}  {puntos[i].X2,5:F1}  {puntos[i].X3,5:F1}  " +
                             $"{puntos[i].X4,5:F1}    1  |  {puntos[i].PesoConocido,4:F1}]");
            }
            sb.AppendLine();

            // Solución
            sb.AppendLine("2. SOLUCIÓN DEL SISTEMA:");
            sb.AppendLine($"  m1 = {Coeficientes[0]:F10}");
            sb.AppendLine($"  m2 = {Coeficientes[1]:F10}");
            sb.AppendLine($"  m3 = {Coeficientes[2]:F10}");
            sb.AppendLine($"  m4 = {Coeficientes[3]:F10}");
            sb.AppendLine($"   B = {Bias:F10}");
            sb.AppendLine();

            // Verificación
            sb.AppendLine(VerificarCalibracion(puntos));
            sb.AppendLine();

            // Peso igual para todas las celdas (ejemplo con 200 kg)
            sb.AppendLine(CalcularXPesoIgual(200));
            sb.AppendLine();

            sb.AppendLine("==========================================");

            return sb.ToString();
        }
    }
}
