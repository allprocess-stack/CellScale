# Registro de Cambios - FormulaGaussExample

## 1. Protocolo multi-línea HBM (CeldaManager.cs)

### Nuevo método: `ConsultarPesoMultiLinea(int direccion)`
- **Línea:** ~365
- **Descripción:** Implementa el protocolo multi-línea HBM para consultar peso, enviando 3 mensajes consecutivos:

  1. `S98\r\n` — llama a todas las celdas (sin respuesta)
  2. `MSV?\r\n` — comando de consulta de peso
  3. `S{dir:D2}\r\n` — selecciona la celda a consultar y obtiene respuesta

  Cada mensaje se envía por separado con pausa de 100ms entre ellos.
  Se espera 300ms después del tercer mensaje para leer la respuesta.

### Nuevo método: `ExtraerPesoHBM(string trama)`
- **Línea:** ~285
- **Descripción:** Parsea la respuesta en formato HBM.
  - La respuesta típica es: `S98MSV?S00 0000250\r\n`
  - El peso tiene formato: `[espacio o -]` + 7 dígitos (décimas de kg)
  - Ejemplo: ` 0000250` → `25.0` kg, `-0010236` → `-102.3` kg
  - Busca el patrón al **final** de la trama para ignorar el eco de los comandos
  - Si no encuentra el patrón HBM, toma el **último** valor numérico como fallback

### Cambio en formato de comandos
- **Antes:** `S98;\\\r\n`, `MSV?;\\\r\n`, `S{dir:D2};\\\r\n`
- **Después:** `S98\r\n`, `MSV?\r\n`, `S{dir:D2}\r\n`
- **Motivo:** El simulador/balanza no reconoce los comandos con `;` y `\` en modo multi-línea

### Cambio en parseo de respuesta
- **Antes:** `ExtraerValorNumerico(limpia)` — tomaba el primer número (`98` de `S98...`)
- **Después:** `ExtraerPesoHBM(limpia)` — toma el patrón HBM al final o el último número
- **Motivo:** La respuesta incluye eco de comandos + peso, había que ignorar el eco

---

## 2. TextBox de consulta por celda (ViewCeldas.cs)

### Nuevo campo: `TextBox[] txtConsultCelda`
- **Línea:** ~17
- **Descripción:** Arreglo de 4 TextBox creados programáticamente

### Nuevo método: `InicializarConsultTextBoxes()`
- **Línea:** ~36
- **Descripción:** Crea los 4 TextBox en y=270 con valores por defecto `S00`, `S01`, `S02`, `S03`
  - Aumenta el alto del formulario de 271 a 320px

### Nuevo método: `ObtenerConsultTextBox(int index)`
- **Línea:** ~165
- **Descripción:** Retorna el TextBox de consulta según el índice del slot (0-3)

### Nuevo método: `ParsearDireccionConsult(string text)`
- **Línea:** ~172
- **Descripción:** Parsea el texto del TextBox para extraer la dirección numérica
  - Acepta formatos: `S00`, `00`, `0`, `S01`, `01`, `1`
  - Normaliza a `S{dir:D2}` después del parseo

---

## 3. Consulta asíncrona (ViewCeldas.cs)

### Cambio: `ConsultarPesoSlot` → `ConsultarPesoSlotAsync`
- **Línea:** ~186
- **Antes:** Método síncrono, bloqueaba la UI durante ~600ms
- **Después:** Método asíncrono (`async Task`), ejecuta `manager.ConsultarPesoMultiLinea` en `Task.Run`
- **Beneficio:** La UI no se congela durante la consulta serial
- El botón se deshabilita durante la consulta y se re-habilita al final (bloque `try/finally`)

### Cambio: Eventos `Click` → `async void`
- **Línea:** ~181-184
- **Antes:** `btnCelda1_Click(object, EventArgs) => ConsultarPesoSlot(0)`
- **Después:** `async void btnCelda1_Click(object, EventArgs) => await ConsultarPesoSlotAsync(0)`

---

## 4. Slots habilitados sin detección (ViewCeldas.cs)

### Cambio en `ActualizarSlots()`
- **Línea:** ~77-90
- **Antes:** Si no había celdas detectadas (`c.Connected == false`), los slots se mostraban deshabilitados (`Celda #--`, textbox y botón disabled)
- **Después:** Si `manager.IsOpen == true` pero no hay celdas detectadas, muestra los 4 slots con direcciones 01-04 habilitados para consultar manualmente
- **Motivo:** El simulador no responde a `IDN?` (enumeración), por lo que nunca se marcaban como `Connected`

### Cambio en `btnPesos_Click`
- **Línea:** ~248
- **Antes:** Solo consultaba celdas con `Connected == true`
- **Después:** Si no hay celdas conectadas, consulta direcciones 1-4 por defecto

---

## 5. Dependencias agregadas

### ViewCeldas.cs
- `using System.Threading.Tasks;` — para `Task.Run` y `async Task`
