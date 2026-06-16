using System;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    /// <summary>
    /// TreeListView resistente al fallo conocido de ObjectListView 2.7 en modo
    /// virtual: durante el repintado, el ListView nativo puede pedir (vía
    /// WM_REFLECT + WM_NOTIFY / RetrieveVirtualItem) un índice de fila que, por un
    /// instante, queda fuera de la lista aplanada del árbol mientras se expanden,
    /// colapsan o reordenan nodos. Eso hace que GetNthObject ejecute
    /// ArrayList[n] fuera de rango y lance ArgumentOutOfRangeException.
    ///
    /// La excepción es transitoria e inofensiva: el siguiente mensaje de pintado
    /// vuelve a consultar el índice ya correcto. Antes, al venir del hilo de UI,
    /// la atrapaba el manejador global (Application.ThreadException) y mostraba al
    /// usuario el diálogo "Ocurrió un error inesperado..." una y otra vez.
    ///
    /// Aquí la absorbemos en el punto exacto donde ocurre (el WndProc del control)
    /// para que el repintado simplemente continúe. Se registra de forma silenciosa
    /// (sin diálogo, con antispam) para conservar telemetría.
    /// </summary>
    public class SafeTreeListView : TreeListView
    {
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Desincronización transitoria entre el tamaño virtual del
                // ListView nativo y la lista aplanada del árbol. Se ignora: el
                // próximo mensaje de pintado pide el índice ya válido.
                ErrorLogger.RegistrarMensaje(
                    "SafeTreeListView.WndProc",
                    "Índice virtual fuera de rango absorbido (msg=0x" +
                    m.Msg.ToString("X") + "): " + ex.Message);
            }
        }
    }
}
