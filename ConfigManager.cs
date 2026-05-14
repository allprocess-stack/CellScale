using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    internal class ConfigManager
    {
        private static string rutaArchivoConfig = "config.json";

        public static AppConfig CargarConfig()
        {
            try
            {
                if (!File.Exists(rutaArchivoConfig))
                {
                    var configDefault = new AppConfig
                    {
                        Servidor = "localhost",
                        BD = "bdCellScale",
                        Puerto = "3306",
                        Usuario = "prueba",
                        Contrasena = "mysqladmin",
                        COMBalanza = "COM3",
                        CalibracionBalanza = "10000"
                    };
                    GuardarConfig(configDefault);
                    return configDefault;
                }

                string json = File.ReadAllText(rutaArchivoConfig);
                return JsonSerializer.Deserialize<AppConfig>(json);
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return null;
            }
        }

        public static void GuardarConfig(AppConfig config)
        {
            try {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaArchivoConfig, json);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
