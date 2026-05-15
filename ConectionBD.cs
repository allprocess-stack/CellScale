using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        // Método para ejecutar consultas(SELECT)
        public MySqlDataReader EjecutarConsulta(string query, Dictionary<string, object> parametros)
        {
            MySqlCommand comando = new MySqlCommand(query, conexion);

            foreach (var p in parametros)
            {
                comando.Parameters.AddWithValue(p.Key, p.Value);
            }

            return comando.ExecuteReader();
        }

        // Método para ejecutar consultas
        public int EjecutarNonQuery(string query, Dictionary<string, object> parametros)
        {
            using (var comando = new MySqlCommand(query, conexion))
            {
                foreach (var p in parametros)
                {
                    comando.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }

                // Ejecuta el comando y devuelve el número de filas afectadas
                return comando.ExecuteNonQuery();
            }
        }


        // Propiedad para obtener la conexión
        public MySqlConnection ObtenerConexion()
        {
            return conexion;
        }
    }
}
