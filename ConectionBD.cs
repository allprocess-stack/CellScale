using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FormulaGaussExample
{
    /// <summary>
    /// Gestiona la conexión a la base de datos MySQL y la ejecución de consultas.
    /// Proporciona métodos para abrir/cerrar conexión, ejecutar consultas SELECT
    /// (EjecutarConsulta) y comandos INSERT/UPDATE/DELETE (EjecutarNonQuery).
    /// </summary>
    internal class ConectionBD
    {
        // Objeto de conexión MySQL subyacente
        private MySqlConnection conexion;

        /// <summary>
        /// Inicializa una nueva instancia de ConectionBD con los parámetros
        /// de configuración especificados.
        /// </summary>
        /// <param name="config">Configuración con datos del servidor MySQL.</param>
        public ConectionBD(AppConfig config)
        {
            // Construir cadena de conexión MySQL con los datos de configuración
            string cadenaConexion = $"Server={config.Servidor};Port={config.Puerto};Database={config.BD};User={config.Usuario};Password={config.Contrasena};";
            conexion = new MySqlConnection(cadenaConexion);
        }

        /// <summary>
        /// Abre la conexión con la base de datos MySQL si está cerrada.
        /// </summary>
        /// <exception cref="Exception">Lanza excepción si no se puede establecer la conexión.</exception>
        public void AbrirConexion()
        {
            if (conexion.State == System.Data.ConnectionState.Closed)
                conexion.Open();
            Console.WriteLine("Conexión abierta correctamente.");
        }

        /// <summary>
        /// Cierra la conexión con la base de datos MySQL si está abierta.
        /// </summary>
        public void CerrarConexion()
        {
            if (conexion.State == System.Data.ConnectionState.Open)
                conexion.Close();
            Console.WriteLine("Conexión cerrada correctamente.");
        }

        /// <summary>
        /// Ejecuta una consulta SELECT con parámetros y devuelve un lector de datos.
        /// Importante: el MySqlDataReader debe ser cerrado por el llamador.
        /// </summary>
        /// <param name="query">Consulta SQL con parámetros nombrados (ej: SELECT * FROM usuario WHERE nombre=@usuario).</param>
        /// <param name="parametros">Diccionario con los parámetros nombre-valor de la consulta.</param>
        /// <returns>MySqlDataReader para iterar sobre los resultados.</returns>
        public MySqlDataReader EjecutarConsulta(string query, Dictionary<string, object> parametros)
        {
            var comando = new MySqlCommand(query, conexion);

            foreach (var p in parametros)
            {
                comando.Parameters.AddWithValue(p.Key, p.Value);
            }

            return comando.ExecuteReader();
        }

        /// <summary>
        /// Ejecuta un comando INSERT, UPDATE o DELETE con parámetros.
        /// </summary>
        /// <param name="query">Comando SQL con parámetros nombrados.</param>
        /// <param name="parametros">Diccionario con los parámetros nombre-valor.</param>
        /// <returns>Número de filas afectadas por el comando.</returns>
        public int EjecutarNonQuery(string query, Dictionary<string, object> parametros)
        {
            using (var comando = new MySqlCommand(query, conexion))
            {
                foreach (var p in parametros)
                {
                    comando.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }

                return comando.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Obtiene la conexión MySQL subyacente para operaciones avanzadas.
        /// </summary>
        /// <returns>Objeto MySqlConnection activo.</returns>
        public MySqlConnection ObtenerConexion()
        {
            return conexion;
        }
    }
}
