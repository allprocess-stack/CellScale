using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    internal class CeldaManager
    {
        private SerialPort puerto;

        // Evento para notificar al Form que se ha recibido una trama de datos
        public event Action<string> TramaRecibida;
        // Variable para controlar si la última trama recibida fue completa o no
        private bool ultimaTramaRecibida = false;


        //Abre el puerto COM especificado
        public void Conectar(string nombrePuerto)
        {
            puerto = new SerialPort(nombrePuerto, 9600, Parity.None, 8, StopBits.None);// Configuración típica para celdas de carga
            puerto.ReadTimeout = 2000; // 2 segundos de timeout para lectura
            puerto.DataReceived += Puerto_DataReceived;
            puerto.Open();
        }

        // Se ejecuta automáticamente cada vez que llega información al puerto COM
        private void Puerto_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string trama = puerto.ReadExisting();// LEE todo lo que llegó
                string limpia = LimpiarTrama(trama);

                if (!string.IsNullOrEmpty(limpia))
                {
                    TramaRecibida?.Invoke(limpia);
                    ultimaTramaRecibida = true; // llegó trama
                }
                else
                {
                    MessageBox.Show("Trama vacía recibida, ignorando.");
                    ultimaTramaRecibida = false; // no llegó nada
                }
            }
            catch (Exception ex)
            {
                TramaRecibida?.Invoke("ERROR: " + ex.Message);
            }
        }


        //Envía un comando de texto al puerto
        public string EnviarComando(string comando)
        {
            // Enviar comando a la celda de carga
            if (puerto!= null && puerto.IsOpen)
            {
                puerto.WriteLine(comando);
                string respuesta = puerto.ReadLine();
                return LimpiarTrama(respuesta);
            }
            return null;
        }

        //Limpia la trama que mande las celdas de carga, eliminando espacios y caracteres no deseados
        private string LimpiarTrama(string trama)
        {
            if (string.IsNullOrEmpty(trama)) return string.Empty;
            return trama.Trim();
        }

        // Consulta el peso actual de la celda de carga en la dirección especificada
        public double ConsultarPeso(int direccion)
        {
            string respuesta = EnviarComando($"S{direccion:D2};MSV?");
            if (double.TryParse(respuesta, out double peso)) return peso;
            return 0;
        }

        // Consulta el número de serie de la celda de carga en la dirección especificada
        public string ConsultarSerie(int direccion)
        {
            return EnviarComando($"S{direccion:D2};IDN?");
        }

        // Asigna una nueva dirección a la celda de carga con el número de serie especificado
        public string AsignarDireccion(int nuevaDireccion, string numeroSerie)
        {
            return EnviarComando($"ADR{nuevaDireccion},\"{numeroSerie}\";");
        }

        // Guarda la configuración actual de la celda de carga en la dirección especificada en su memoria EEPROM
        public string GuardarEEPROM(int direccion)
        {
            return EnviarComando($"S{direccion:D2};TDD1;");
        }
    }
}
