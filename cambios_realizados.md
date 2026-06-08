# Cambios Realizados - Calibración Gauss y Compensación de Esquinas

## Resumen

Se corrigieron 3 problemas que impedían que la calibración multivariable (Gauss) y la compensación de esquinas funcionaran correctamente con el simulador de celdas. La causa raíz era una inconsistencia entre el formato de comando serial usado por `ConsultarPeso` (runtime) y lo que el simulador reconocía, más una discrepancia entre los métodos de lectura usados durante calibración vs. runtime.

---

## Cambio 1: Formato de comando en `EnviarComando` — CeldaManager.cs

**Problema:** `EnviarComando` enviaba `S01;MSV?;\r\n` (con backslash `\` antes del CR+LF), pero el simulador espera `S01;MSV?;\r\n` (sin `\`). La expresión regular del simulador `^S(\d+);MSV\?;?$` no reconocía el comando por el carácter `\` extra → `ConsultarPeso` nunca obtenía respuesta → `RawWeight` nunca se actualizaba en el timer de ViewMain.

**Solución:**
```
// Antes (no funcionaba con el simulador):
string comandoCompleto = $"S{direccion:D2};{comando};\\\r\n";

// Después:
string comandoCompleto = $"S{direccion:D2};{comando};\r\n";
```

**Archivo:** `CeldaManager.cs`, línea 379

---

## Cambio 2: `ExtraerPesoHBM` vs `ExtraerValorNumerico` — inconsistencia en el parseo

**Problema:** El simulador envía el peso en **décimas de kg** (`get_weight_response()` multiplica por 10: 100 kg → ` 0001000`). Pero `ExtraerPesoHBM` (usado por `ConsultarPesoMultiLinea`) dividía entre 100, devolviendo 10.0 kg en vez de 100.0 kg. `ExtraerValorNumerico` (usado por `ConsultarPeso`) NO divide y devuelve el valor entero (1000).

Esto causaba que:
- Calibración (vía `ConsultarPesoMultiLinea`) registrara valores 10× menores que runtime
- Los coeficientes Gauss se resolvían incorrectamente

**Solución:** Los métodos de calibración ahora usan `ConsultarPeso` + `ExtraerValorNumerico`, igual que el timer de runtime. Así calibración y runtime leen los mismos valores.

---

## Cambio 3: Métodos de calibración en ViewCeldas — ahora usan `ConsultarPeso`

**Problema:** Los 3 métodos de calibración en ViewCeldas usaban `ConsultarPesoMultiLinea`, mientras que el timer de ViewMain usa `ConsultarPeso`. Esto producía valores inconsistentes.

**Solución:** Cambiar los 3 métodos para que usen `ConsultarPeso` (llamada por celda) y luego lean `RawWeight` del objeto `CeldaInfo`:

| Método | Línea | Antes | Después |
|---|---|---|---|
| `btnCeroCalibracion_Click` | 448 | `ConsultarPesoMultiLinea(dir)` | `ConsultarPeso(dir)` + `celdas[i].RawWeight` |
| `CapturarEsquinaAsync` | 509 | `ConsultarPesoMultiLinea(dir)` | `ConsultarPeso(dir)` + `celdas[i].RawWeight` |
| `btnCapturarPuntoGauss_Click` | 613 | `ConsultarPesoMultiLinea(dir)` | `ConsultarPeso(dir)` + `celdas[i].RawWeight` |

**Archivo:** `ViewCeldas.cs`

---

## Cambio 4: Direcciones fijas → celdas conectadas ordenadas en `tsmiGuardarConfiguracion_Click`

**Problema:** El método `tsmiGuardarConfiguracion_Click` en ViewMain usaba direcciones fijas `Celdas[1]` a `Celdas[4]` para leer los pesos raw, pero las celdas reales pueden estar en otras direcciones.

**Solución:** Ahora obtiene las celdas conectadas ordenadas por `SlaveNumber`:
```csharp
var celdas = manager.Celdas.Values
    .Where(c => c.Connected)
    .OrderBy(c => c.SlaveNumber)
    .ToList();
```

**Archivo:** `ViewMain.cs`, línea 579

---

## Cambio 5: Eliminación de `manager.Celdas.Clear()` en ViewCeldas_FormClosing

**Problema:** Al cerrar ViewCeldas, `FormClosing` limpiaba `manager.Celdas.Clear()`, destruyendo los datos de celdas que ViewMain necesita.

**Solución:** Se eliminó esa línea. ViewCeldas solo se oculta al cerrar sin afectar el manager compartido.

**Archivo:** `ViewCeldas.cs`

---

## Cambio 6: Propiedades de configuración en AppConfig.cs

**Problema:** No existían propiedades para guardar/cargar la compensación de esquinas en config.json.

**Solución:** Se agregaron:
- `CerosCompensacion` (double[4])
- `FactoresCompensacion` (double[4])
- `CompensacionEsquinasActiva` (bool)

**Archivo:** `AppConfig.cs`

---

## Cambio 7: `using System.Linq` en CeldaManager.cs

**Problema:** Faltaba `using System.Linq` para usar LINQ (`Where`, `OrderBy`, `ToList`) en `ObtenerCeldasOrdenadas()`.

**Solución:** Agregado `using System.Linq;` a los imports.

**Archivo:** `CeldaManager.cs`, línea 5

---

## Flujo corregido

### Calibración Gauss (ViewCeldas)
```
btnCapturarPuntoGauss_Click (x5)
  → ConsultarPeso(celda[0..3].SlaveNumber)  // ahora usa ConsultarPeso
  → RawWeight = valor del simulador (ej: 1000 para 100kg)
  → Guarda PuntoCalibracion { X1..X4 = RawWeight, PesoConocido }
  → Al llegar a 5 puntos: ResolverGauss()
    → Coeficientes m1..m4, B
    → btnAplicarGauss_Click: guarda en config.json + manager.ConfigurarCalibracionMultivariable()
```

### Runtime (ViewMain)
```
TimerPesaje_Tick (c/250ms)
  → ConsultarPeso(celda.SlaveNumber)  // mismo método que calibración
  → RawWeight actualizado (ej: 1000)
  → ObtenerPesoUnificado()
    → CalcularPesoMultivariable(): PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
  → txtBalanza.Text = resultado (kg)
```

### Compensación de Esquinas (ViewCeldas → ViewMain)
```
btnCeroCalibracion_Click
  → ConsultarPeso(celda[0..3])  → ceros[0..3] = RawWeight
CapturarEsquinaAsync(1..4)
  → ConsultarPeso(celda[0..3])  → rawReadings[0..3]
  → Calcula factores compensación
  → Guarda en config.json + manager.ConfigurarCompensacionEsquinas()

ViewMain startup / reconnect
  → CargarFactoresCalibracion()
    → manager.ConfigurarCompensacionEsquinas(ceros, factores)
  → ObtenerPesoUnificado() aplica: Σ(Raw_i - Cero_i) × Factor_i
```

---

## Archivos modificados

| Archivo | Cambios |
|---|---|
| `CeldaManager.cs` | +`using System.Linq`; fix comando serial en `EnviarComando`; nuevo helper `ObtenerCeldasOrdenadas()`; `CalcularPesoMultivariable()` usa celdas conectadas; `ObtenerPesoUnificado()` actualizado para todos los modos |
| `ViewCeldas.cs` | +controles UI para compensación esquinas + Gauss; 3 métodos calibración usan `ConsultarPeso`; eliminado `Celdas.Clear()` en FormClosing |
| `ViewMain.cs` | `CargarCeldasConfig()` carga Gauss; `CargarFactoresCalibracion()` carga Gauss + esquinas; `tsmiGuardarConfiguracion` usa celdas conectadas |
| `AppConfig.cs` | +`CerosCompensacion`, `FactoresCompensacion`, `CompensacionEsquinasActiva` |
| `ViewCeldas.Designer.cs` | Restaurados todos los controles originales + nuevos controles de calibración |
