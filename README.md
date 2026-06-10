# FormulaGaussExample — CellScale

Sistema de báscula multicelda con comunicación RS-485 para celdas de carga HBM C16iC3. Soporta calibración simple, multivariable (Gauss 5 puntos), compensación de esquinas (excentricidad) y registro en MySQL.

## Arquitectura

```
ViewMain (Principal)
  ├── ViewCeldas      → Calibración de esquinas + Gauss multivariable
  ├── ViewWeightCeldas → Monitoreo en tiempo real de cada celda
  ├── ViewBd           → Gestión de base de datos
  └── CeldaManager     → Comunicación serial con celdas HBM
        ├── CalibracionLineal   → Solver Gauss (5×5)
        ├── BalanzaMatricial    → Corrección matricial 4×4
        └── CeldaInfo           → Modelo de datos de cada celda
```

## Modos de Calibración

| Modo | Descripción | Activación |
|---|---|---|
| **Simple** | Peso = Raw × Factor por celda | Default |
| **Gauss Multivariable** | Peso = X1·m1 + X2·m2 + X3·m3 + X4·m4 + B (5 puntos) | ViewCeldas → "Capturar Punto" |
| **Compensación de Esquinas** | Factores correctivos por esquina para peso independiente de la posición | ViewCeldas → "Capturar Cero" + "Esquina 1..4" |
| **Matricial** | Corrección por matriz 4×4 (Gauss-Jordan) | Código |

## Formularios

### ViewMain
- Peso unificado en `txtBalanza` (actualizado cada 250ms)
- Barra de herramientas: LOGIN, MENÚ, CONFIG, BD, **PESO CELDAS**, **CELDAS CONFIG**
- Botón "REGISTRAR PESO" para guardar en MySQL
- Guardar configuración → `Guardar Configuración` (Gauss 5 puntos desde menú)

### ViewCeldas
- **Calibración de Esquinas** (Excentricidad):
  1. Capturar Cero (balanza vacía)
  2. Colocar peso conocido en Esquina 1..4 y presionar botón correspondiente
  3. Calcula factores de corrección → peso constante en cualquier posición
- **Calibración Multivariable (Gauss)**:
  1. Colocar peso TOTAL conocido en la báscula
  2. Capturar 5 puntos con distintos pesos
  3. Resuelve coeficientes m1..m4, B
  4. Aplicar Gauss para activar
- Monitoreo individual de cada celda en los slots
- `txtPesoCalibracion` se persiste automáticamente en `config.json`

### ViewWeightCeldas
- Abrir desde toolbar → **PESO CELDAS**
- Muestra en tiempo real (cada 1s) el peso individual de S00, S01, S02, S03
- Solo lectura, para monitoreo

## Comunicación Serial

- Protocolo HBM: `S{dir:D2};{COMANDO};\r\n`
- Velocidad: 9600 baud, 8N1
- Comandos: `MSV?` (peso), `IDN?` (identificación), `ADR2` (asignar dirección)
- Las celdas se direccionan como S00, S01, S02, S03

## Configuración

Archivo: `config.json`

```json
{
  "Servidor": "localhost",
  "BD": "bdCellScale",
  "Puerto": "3306",
  "COMBalanza": "COM3",
  "CalibracionBalanza": "1000",
  "CoeficienteM1": 0.0,   // Gauss m1
  "CoeficienteM2": 0.0,   // Gauss m2
  "CoeficienteM3": 0.0,   // Gauss m3
  "CoeficienteM4": 0.0,   // Gauss m4
  "BiasB": 0.0,           // Gauss B
  "CalibracionMultivariableActiva": false,
  "CerosCompensacion": null,      // [Z1, Z2, Z3, Z4]
  "FactoresCompensacion": null,   // [F1, F2, F3, F4]
  "CompensacionEsquinasActiva": false
}
```

## Base de Datos

Tablas:
- `celda_peso` — Registro de pesos individuales (nombre_celda, valor_peso, fecha_registro)
- `peso` — Registro de pesadas (peso, fecha, celda_id)
- `usuario` — Credenciales de acceso

## Proyecto

- .NET Framework 4.7.2 (Windows Forms)
- C# con System.IO.Ports para RS-485
- MySql.Data para conexión MySQL
- System.Text.Json para persistencia

## Registro de Cambios

- `cambios_sesion_actual.md` — Cambios de la sesión actual
- `cambios_realizados.md` — Cambios anteriores
