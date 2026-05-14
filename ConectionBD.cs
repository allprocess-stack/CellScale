using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;

namespace FormulaGaussExample
{
    internal class ConectionBD
    {
        private MySqlConnection conexion;

        // Constructor: inicializa la conexión
        public ConectionBD(AppConfig config)
        {
            // Cadea de conexión a la base de satos MYSQL
            string cadenaConexion = $"Server={config.Servidor};Port={config.Puerto};Database={config.BD};User={config.Usuario};Password={config.Contrasena};";
            conexion = new MySqlConnection(cadenaConexion);
        }

        // Método para abrir conexión
        public void AbrirConexion()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Closed)
                    conexion.Open();
                Console.WriteLine("Conexión abierta correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir conexión: " + ex.Message);
            }
        }

        // Método para cerrar conexión
        public void CerrarConexion()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                    conexion.Close();
                Console.WriteLine("Conexión cerrada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cerrar conexión: " + ex.Message);
            }
        }

        // Método para ejecutar consultas
        public MySqlDataReader EjecutarConsulta(string query)
        {
            MySqlCommand comando = new MySqlCommand(query, conexion);
            return comando.ExecuteReader();
        }

        // Propiedad para obtener la conexión
        public MySqlConnection ObtenerConexion()
        {
            return conexion;
        }
    }
}
