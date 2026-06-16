// FormAlmacen - Main partial class declaration
// This file contains only the class declaration and using statements
// Functionality is split across multiple partial class files:
// - FormAlmacen_Core.cs: Core initialization and main logic
// - FormAlmacen_Theme.cs: UI theming and styling
// - FormAlmacen_Layout.cs: Responsive layout configuration
// - FormAlmacen_Entradas.cs: Database operations for warehouse entries
// - FormAlmacen_Salidas.cs: Database operations for warehouse exits
// - FormAlmacen_Historial.cs: History tracking functionality
// - FormAlmacen.Designer.cs: UI controls and component initialization

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using DynamicSepticSystem;
using System.IO;
using System.Diagnostics;
using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Configuration;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario para gestión de almacén (entradas, salidas e historial)
    /// Implementado como clase parcial para mejor organización del código
    /// </summary>
    public partial class FormAlmacen : Form
    {
        // La implementación está dividida en los siguientes archivos:
        // - FormAlmacen_Core.cs: Constructor, inicialización y métodos principales
        // - FormAlmacen_Theme.cs: Configuración de estilos y tema corporativo
        // - FormAlmacen_Layout.cs: Layout responsivo y redimensionamiento
        // - FormAlmacen_Entradas.cs: Operaciones de entrada de almacén
        // - FormAlmacen_Salidas.cs: Operaciones de salida de almacén
        // - FormAlmacen_Historial.cs: Gestión del historial de movimientos
        // - FormAlmacen.Designer.cs: Definición de controles UI
    }
}