using System;

namespace FormulaGaussExample
{
    /// <summary>
    /// Representa una celda de carga conectada al sistema.
    /// Almacena la información de identificación, estado y peso de cada celda
    /// detectada en el bus RS-485.
    /// </summary>
    internal class CeldaInfo
    {
        /// <summary>
        /// Dirección esclavo (1-15) asignada a la celda en el bus RS-485.
        /// </summary>
        public int SlaveNumber { get; set; }

        /// <summary>
        /// Número de serie de la celda, obtenido mediante el comando IDN?.
        /// Formato típico: HBM,C16iC3/40t,M64702,P52
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Peso crudo (sin calibrar) leído directamente desde la celda.
        /// </summary>
        public double RawWeight { get; set; }

        /// <summary>
        /// Peso calibrado = RawWeight * Factor de calibración de la celda.
        /// </summary>
        public double CalibratedWeight { get; set; }

        /// <summary>
        /// Indica si la celda está respondiendo correctamente en la red.
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        /// Fecha y hora de la última lectura exitosa de peso.
        /// </summary>
        public DateTime LastRead { get; set; }

        /// <summary>
        /// Devuelve una cadena formateada con la información resumida de la celda
        /// para mostrar en la interfaz de usuario.
        /// </summary>
        public override string ToString()
        {
            return $"Celda #{SlaveNumber} | Serie: {SerialNumber ?? "N/A"} | Peso: {CalibratedWeight:F2}";
        }
    }
}
