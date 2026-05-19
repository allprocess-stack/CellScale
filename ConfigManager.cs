using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// Gestiona la carga y guardado de la configuración de la aplicación
    /// desde/hacia el archivo JSON (config.json) en el directorio de ejecución.
    /// </summary>
    internal class ConfigManager
    {
        // Ruta del archivo de configuración JSON
        private static string rutaArchivoConfig = "config.json";

        /// <summary>
        /// Carga la configuración desde el archivo config.json.
        /// Si el archivo no existe, crea una configuración con valores
        /// predeterminados y la guarda en disco.
        /// </summary>
        /// <returns>
        /// Objeto AppConfig con la configuración cargada.
        /// Retorna null si ocurre un error durante la carga.
        /// </returns>
        public static AppConfig CargarConfig()
        {
            try
            {
                if (!File.Exists(rutaArchivoConfig))
                {
                    // Crear configuración por defecto si no existe el archivo
                    var configDefault = new AppConfig
                    {
                        Servidor = "localhost",
                        BD = "bdCellScale",
                        Puerto = "3306",
                        Usuario = "prueba",
                        Contrasena = "mysqladminWIN11",
                        COMBalanza = "COM3",
                        CalibracionBalanza = "10000"
                    };
                    GuardarConfig(configDefault);
                    return configDefault;
                }

                string json = File.ReadAllText(rutaArchivoConfig);
                return JsonSerializer.Deserialize<AppConfig>(json);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error al cargar configuración: {e.Message}");
                MessageBox.Show($"Error al cargar config.json: {e.Message}\n\nSe usará configuración por defecto.",
                    "Error de configuración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// Guarda la configuración actual en el archivo config.json
        /// con formato JSON indentado para facilitar la lectura manual.
        /// </summary>
        /// <param name="config">Objeto AppConfig con la configuración a guardar.</param>
        public static void GuardarConfig(AppConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaArchivoConfig, json);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error al guardar configuración: {e.Message}");
                MessageBox.Show($"Error al guardar config.json: {e.Message}",
                    "Error de configuración", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
