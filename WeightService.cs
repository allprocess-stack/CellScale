using System;
using System.Collections.Generic;
using System.Drawing;

namespace FormulaGaussExample
{
    /// <summary>Servicio de simulación de pesos para las 4 esquinas de una báscula.</summary>
    public class WeightService
    {
        /// <summary>Tamaño en unidades de la superficie cuadrada de la báscula.</summary>
        public const int SquareSize = 400;
        /// <summary>Peso máximo simulado en kg.</summary>
        public const double MaxWeight = 160000;
        /// <summary>Peso mínimo simulado en kg.</summary>
        public const double MinWeight = 0;

        private readonly Dictionary<string, double> _cornerOffsets;
        private Dictionary<string, double> _tareBaseline;
        private readonly double _noiseStd;
        private readonly Random _random;

        /// <summary>Indica si se ha realizado la tara.</summary>
        public bool IsTared => _tareBaseline != null;

        /// <summary>Inicializa el servicio con offsets aleatorios para cada esquina.</summary>
        public WeightService()
        {
            _random = new Random();
            _noiseStd = 2.0;
            _cornerOffsets = new Dictionary<string, double>
            {
                ["top-left"] = Math.Round(_random.NextDouble() * 10 - 130, 2),
                ["top-right"] = Math.Round(_random.NextDouble() * 9, 2),
                ["bottom-left"] = Math.Round(_random.NextDouble() * 9, 2),
                ["bottom-right"] = Math.Round(_random.NextDouble() * 10 - 130, 2)
            };
            _tareBaseline = null;
        }

        /// <summary>Genera ruido aleatorio usando la transformada Box-Muller.</summary>
        private double GetNoise()
        {
            return BoxMullerTransform() * _noiseStd;
        }

        /// <summary>Transformada Box-Muller para generar ruido gaussiano.</summary>
        private double BoxMullerTransform()
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        /// <summary>Captura la línea base actual como tara.</summary>
        public void Tare()
        {
            _tareBaseline = new Dictionary<string, double>(_cornerOffsets);
        }

        /// <summary>Limpia la tara actual.</summary>
        public void ClearTare()
        {
            _tareBaseline = null;
        }

        /// <summary>Calcula los pesos en cada esquina según la posición y el peso total.</summary>
        /// <param name="positionX">Posición X del centro de carga (0-SquareSize).</param>
        /// <param name="positionY">Posición Y del centro de carga (0-SquareSize).</param>
        /// <param name="totalWeight">Peso total aplicado en kg.</param>
        /// <returns>Diccionario con los pesos de cada esquina.</returns>
        public Dictionary<string, double> CalculateCornerWeights(double positionX, double positionY, double totalWeight)
        {
            double nx = positionX / SquareSize;
            double ny = positionY / SquareSize;

            nx = Math.Max(0, Math.Min(1, nx));
            ny = Math.Max(0, Math.Min(1, ny));

            double rawTl = (1 - nx) * (1 - ny);
            double rawTr = nx * (1 - ny);
            double rawBl = (1 - nx) * ny;
            double rawBr = nx * ny;

            double total = rawTl + rawTr + rawBl + rawBr;
            if (total > 0)
            {
                rawTl /= total;
                rawTr /= total;
                rawBl /= total;
                rawBr /= total;
            }

            var baseWeights = new Dictionary<string, double>
            {
                ["top-left"] = rawTl * totalWeight,
                ["top-right"] = rawTr * totalWeight,
                ["bottom-left"] = rawBl * totalWeight,
                ["bottom-right"] = rawBr * totalWeight
            };

            var corners = new[] { "top-left", "top-right", "bottom-left", "bottom-right" };
            var result = new Dictionary<string, double>();

            foreach (var corner in corners)
            {
                double value = baseWeights[corner] + _cornerOffsets[corner] + GetNoise();
                if (_tareBaseline != null && _tareBaseline.ContainsKey(corner))
                {
                    value -= _tareBaseline[corner];
                }
                result[corner] = Math.Round(value, 2);
            }

            return result;
        }

        /// <summary>Calcula la suma total de los pesos de todas las esquinas.</summary>
        public double GetMeasuredTotal(Dictionary<string, double> cornerWeights)
        {
            double total = 0;
            foreach (var kv in cornerWeights)
            {
                total += kv.Value;
            }
            return total;
        }

        /// <summary>Devuelve un color según la proporción del peso respecto al total medido.</summary>
        public Color GetWeightColor(double weight, double totalMeasured)
        {
            if (totalMeasured <= 0) return Color.Gray;
            double ratio = weight / totalMeasured;
            if (ratio < 0.33) return Color.FromArgb(76, 175, 80);
            if (ratio < 0.66) return Color.FromArgb(0, 106, 255);
            return Color.FromArgb(244, 67, 54);
        }
    }
}
