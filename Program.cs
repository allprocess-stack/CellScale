using System;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// Punto de entrada principal para la aplicación de báscula multicelda.
    /// Inicializa los estilos visuales y lanza el formulario principal Form1.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// Habilita los estilos visuales de Windows Forms, configura el renderizado
        /// de texto compatible y ejecuta el formulario principal.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ViewMain());
        }
    }
}
