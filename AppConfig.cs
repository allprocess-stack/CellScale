using System.Collections.Generic;

namespace FormulaGaussExample
{
    /// <summary>
    /// Configuración general de la aplicación.
    /// Se serializa/deserializa desde/hacia el archivo config.json.
    /// Contiene datos de conexión a la base de datos MySQL, configuración
    /// del puerto COM de la balanza y factores de calibración de las celdas.
    /// </summary>
    public class AppConfig
    {
        /// <summary>Dirección IP o nombre del servidor MySQL (ej: localhost).</summary>
        public string Servidor { get; set; }

        /// <summary>Nombre de la base de datos MySQL (ej: bdCellScale).</summary>
        public string BD { get; set; }

        /// <summary>Puerto de conexión al servidor MySQL (ej: 3306).</summary>
        public string Puerto { get; set; }

        /// <summary>Usuario de conexión a la base de datos MySQL.</summary>
        public string Usuario { get; set; }

        /// <summary>Contraseña del usuario de la base de datos MySQL.</summary>
        public string Contrasena { get; set; }

        /// <summary>Puerto COM configurado para la conexión con la balanza.</summary>
        public string COMBalanza { get; set; }

        /// <summary>Valor de calibración predeterminado de la balanza.</summary>
        public string CalibracionBalanza { get; set; }

        /// <summary>
        /// Diccionario de factores de calibración por celda (método simple).
        /// Key con formato "CELDA_XX" (ej: CELDA_01), Value: factor de calibración.
        /// </summary>
        public Dictionary<string, double> FactoresCalibracion { get; set; }

        // --- Coeficientes de calibración multivariable ---

        /// <summary>Coeficiente m1 de la celda 1 (calibración multivariable).</summary>
        public double CoeficienteM1 { get; set; }

        /// <summary>Coeficiente m2 de la celda 2 (calibración multivariable).</summary>
        public double CoeficienteM2 { get; set; }

        /// <summary>Coeficiente m3 de la celda 3 (calibración multivariable).</summary>
        public double CoeficienteM3 { get; set; }

        /// <summary>Coeficiente m4 de la celda 4 (calibración multivariable).</summary>
        public double CoeficienteM4 { get; set; }

        /// <summary>Bias (offset) B del sistema (calibración multivariable).</summary>
        public double BiasB { get; set; }

        /// <summary>Indica si la calibración multivariable está activa.</summary>
        public bool CalibracionMultivariableActiva { get; set; }

        // --- Compensación de esquinas (excentricidad) ---

        /// <summary>Valores de cero para compensación de esquinas (Z1..Z4).</summary>
        public double[] CerosCompensacion { get; set; }

        /// <summary>Factores de corrección para compensación de esquinas (F1..F4).</summary>
        public double[] FactoresCompensacion { get; set; }

        /// <summary>Indica si la compensación de esquinas está activa.</summary>
        public bool CompensacionEsquinasActiva { get; set; }
    }
}
