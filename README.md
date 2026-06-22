# FormulaGaussExample — CellScale

Sistema de báscula multicelda con comunicación RS-485 para celdas de carga HBM C16iC3. Soporta calibración simple, multivariable (Gauss 5 puntos), compensación de esquinas (excentricidad) y registro en MySQL.

## Arquitectura

```
ViewMain (Principal)
  ├── ViewCeldas        → Calibración de esquinas + Gauss multivariable
  ├── ViewWeightCeldas   → Monitoreo en tiempo real de cada celda
  ├── ViewBd             → Gestión de base de datos
  ├── ViewCeldaConfig    → Configuración de direcciones de celdas
  └── CeldaManager       → Comunicación serial con celdas HBM
        ├── CalibracionLineal   → Solver Gauss (5×5)
        ├── BalanzaMatricial    → Corrección matricial 4×4
        ├── CeldaInfo           → Modelo de datos de cada celda
        └── WeightService       → Simulación de pesos por esquina
```

## Formularios

### ViewMain
- Peso unificado en `txtBalanza` (actualizado cada 250ms vía round-robin)
- Barra de herramientas: LOGIN, MENÚ, CONFIG, BD, **PESO CELDAS**, **CELDAS CONFIG**
- Botón "REGISTRAR PESO" para guardar en MySQL
- Menú → **Calibración Gauss 5-Ptos**: captura 5 puntos con distintos pesos y resuelve el sistema
- Guardar configuración → `Guardar Configuración` persiste en config.json
- Conexión automática al iniciar si hay un puerto configurado

### ViewCeldas
- **Calibración de Esquinas** (Excentricidad):
  1. Capturar Cero (balanza vacía)
  2. Colocar peso conocido en Esquina 1..4 y presionar botón correspondiente
  3. Calcula factores de corrección → peso constante en cualquier posición
- **Calibración Multivariable (Gauss)**:
  1. Colocar peso TOTAL conocido en la báscula
  2. Capturar 5 puntos con distintos pesos
  3. Resuelve coeficientes m1..m4, B por eliminación de Gauss
  4. Aplicar Gauss para activar en tiempo real
- Monitoreo individual de cada celda en los slots
- `txtPesoCalibracion` se persiste automáticamente en `config.json` al cambiar el texto
- Botón **CONSULTAR TODOS** para consultar y guardar todas las celdas en BD

### ViewWeightCeldas
- Abrir desde toolbar → **PESO CELDAS**
- Muestra en tiempo real (cada 1s) el peso individual de las celdas detectadas
- Solo lectura, para monitoreo

### ViewBd
- Formulario de configuración de conexión MySQL
- Campos: Servidor, BD, Puerto, Usuario, Contraseña
- Botón **Probar Conexión**: verifica conectividad con los parámetros ingresados
- Botón **Guardar**: persiste en config.json
- Muestra la ruta del archivo config.json

### ViewCeldaConfig
- Configuración de direcciones personalizadas para cada celda (S00..S03)
- Guarda en config.json como lista separada por comas

## Modos de Calibración

| Modo | Descripción | Activación |
|---|---|---|
| **Simple** | Peso = Raw × Factor por celda | Default |
| **Gauss Multivariable** | Peso = X1·m1 + X2·m2 + X3·m3 + X4·m4 + B (5 puntos) | ViewCeldas → "Capturar Punto" |
| **Compensación de Esquinas** | Factores correctivos por esquina para peso independiente de la posición | ViewCeldas → "Capturar Cero" + "Esquina 1..4" |
| **Matricial** | Corrección por matriz 4×4 (Gauss-Jordan) | Código |

## Comunicación Serial

- Protocolo HBM: `S{dir:D2};{COMANDO};\r\n`
- Velocidad: 9600 baud, 8N1
- Comandos: `MSV?` (peso), `IDN?` (identificación), `ADR2` (asignar dirección)
- Las celdas se direccionan como S00, S01, S02, S03
- Soporte multi-línea: `S98` → `MSV?` → `S{dir}` para compatibilidad con ciertos firmwares

## Configuración

Archivo: `config.json`

```json
{
  "Servidor": "localhost",
  "BD": "bdCellScale",
  "Puerto": "3306",
  "COMBalanza": "COM3",
  "CalibracionBalanza": "1000",
  "CoeficienteM1": 0.0,
  "CoeficienteM2": 0.0,
  "CoeficienteM3": 0.0,
  "CoeficienteM4": 0.0,
  "BiasB": 0.0,
  "CalibracionMultivariableActiva": false,
  "CerosCompensacion": null,
  "FactoresCompensacion": null,
  "CompensacionEsquinasActiva": false,
  "FactoresCalibracion": {},
  "Celda1": "S00",
  "Celda2": "S01",
  "Celda3": "S02",
  "Celda4": "S03",
  "Celdas": "S00,S01,S02,S03"
}
```

## Clases del Proyecto

| Clase | Archivo | Propósito |
|---|---|---|
| `AppConfig` | `AppConfig.cs` | Modelo de configuración serializable a JSON |
| `BalanzaMatricial` | `BalanzaMatricial.cs` | Corrección matricial 4×4 por Gauss-Jordan |
| `PuntoCalibracion` | `CalibracionLineal.cs` | Punto de calibración (lecturas + peso conocido) |
| `CalibracionLineal` | `CalibracionLineal.cs` | Solver de sistema 5×5 por eliminación de Gauss |
| `CeldaInfo` | `CeldaInfo.cs` | Modelo de datos de una celda de carga |
| `CeldaManager` | `CeldaManager.cs` | Gestor de comunicación serial RS-485 con celdas HBM |
| `ConectionBD` | `ConectionBD.cs` | Conexión y consultas a MySQL |
| `ConfigManager` | `ConfigManager.cs` | Carga/guardado de config.json |
| `WeightService` | `WeightService.cs` | Simulación de distribución de pesos por esquina |

## Base de Datos

Tablas:
- `celda_peso` — Registro de pesos individuales (nombre_celda, valor_peso, fecha_registro)
- `peso` — Registro de pesadas (peso, fecha, celda_id)
- `usuario` — Credenciales de acceso (nombre, contrasena)

## Proyecto

- .NET Framework 4.7.2 (Windows Forms)
- C# con System.IO.Ports para RS-485
- MySql.Data para conexión MySQL
- System.Text.Json para persistencia

--
Desarrollado por: Anthony Josue Laura Perez
GitHub : https://github.com/anthony2004lp
