using System.Collections.Generic;

namespace FormulaGaussExample
{
    /// <summary>
    /// Configuración general de la aplicación.
    /// Se serializa/deserializa desde/hacia el archivo config.json.
    /// Contiene datos de conexión a la base de datos MySQL, configuración
    /// del puerto COM de la balanza y factores de calibración de las celdas.
    /// </summary>
    internal class AppConfig
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
        /// Diccionario de factores de calibración por celda.
        /// Key con formato "CELDA_XX" (ej: CELDA_01), Value: factor de calibración.
        /// </summary>
        public Dictionary<string, double> FactoresCalibracion { get; set; }
    }
}
