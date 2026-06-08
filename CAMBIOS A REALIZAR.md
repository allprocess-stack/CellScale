# Lógica del programa

##### Inicio automático



* Al ejecutar el programa, se inicia un ciclo que cada 1 segundo consulta el peso de las 4 celdas en el simulador.



* La lectura es continua y se actualiza en la interfaz.



##### Interfaz (ViewCeldas)



* Texbox donde se ingresar la calibracion que se aplicara a las celdas.



* Textbox por celda (celda1–4): muestra el peso actual leído del simulador.



* Botón de calibración por celda (calibrar celda 1–4): al presionarlo, guarda localmente el valor calibrado de esa celda (no se guarda en BD).



* Textbox de peso calibrado (mostrarPesoCalibrado1–4): refleja el valor calibrado que se asignó a cada celda.



* Textbox de sumatoria (txtBalanza): muestra la suma de los 4 valores calibrados.



##### Calibración



* Cada celda se calibra de manera independiente con su botón.



* El valor calibrado se guarda en memoria local (ej. en una lista o diccionario).



* Solo cuando las 4 celdas tienen valores calibrados, se habilita la aplicación de la fórmula de Gauss.



##### Aplicación de la fórmula de Gauss



* Una vez calibradas las 4 celdas, se toma la sumatoria y se aplica la fórmula de Gauss para obtener el resultado final.



* Este resultado puede mostrarse en la interfaz o exportarse según necesidad.

