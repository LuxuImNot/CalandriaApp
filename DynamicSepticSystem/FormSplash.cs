using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Win32;

namespace DynamicSepticSystem
{
    public partial class FormSplash : Form
    {
        private bool videoTerminado = false;
        private Timer fallbackTimer;

        public FormSplash()
        {
            InitializeComponent();

            // Ventana sin bordes
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ControlBox = false;

            // Tamaño fijo aproximado
            this.Size = new Size(1042, 450);

            // Configurar WebBrowser
            webBrowser1.Dock = DockStyle.Fill;
            webBrowser1.ScriptErrorsSuppressed = true;

            string rutaVideo = Path.Combine(Application.StartupPath, "startup.mp4");
            if (!File.Exists(rutaVideo))
            {
                this.DialogResult = DialogResult.OK;
                Close();
                return;
            }

            // Forzar emulación IE11 para WebBrowser
            EnsureBrowserEmulation();

            string fileUri = new Uri(rutaVideo).AbsoluteUri;
            string rutaHtml = Path.Combine(Path.GetTempPath(), "splash.html");

            File.WriteAllText(rutaHtml, $@"<!DOCTYPE html>
<html>
<head>
<meta http-equiv='X-UA-Compatible' content='IE=11' />
<meta charset='utf-8'/>
<style>
html,body{{margin:0;height:100%;background:#000;overflow:hidden}}
video{{width:100%;height:100%;object-fit:cover}}
.credito{{
  position:fixed;right:14px;bottom:10px;
  font-family:'Segoe UI',Arial,sans-serif;
  font-size:11px;font-style:italic;
  color:rgba(255,255,255,0.78);
  text-shadow:0 1px 3px rgba(0,0,0,0.85);
  letter-spacing:0.3px;pointer-events:none;
}}
</style>
</head>
<body>
<video autoplay muted playsinline id='v' onended='window.external.Finish();'>
  <source src='{fileUri}' type='video/mp4'>
</video>
<div class='credito'>A software by LuxuDev</div>
<script>
  try {{
    var vid = document.getElementById('v');
    if (vid) {{ vid.play().catch(function(e) {{}}); }}
  }} catch(e) {{}}
</script>
</body>
</html>");

            webBrowser1.ObjectForScripting = new ScriptBridge(this);
            try
            {
                webBrowser1.Navigate(rutaHtml);
            }
            catch
            {
                // Si falla cargar en WebBrowser, intentar reproductor externo
                TryExternalPlay(rutaVideo);
                return;
            }

            // Fallback: si no termina en 90s, cerrar
            fallbackTimer = new Timer();
            fallbackTimer.Interval = 90000; // 90s
            fallbackTimer.Tick += (s, e) =>
            {
                fallbackTimer.Stop();
                if (!videoTerminado) TerminarVideo();
            };
            fallbackTimer.Start();
        }

        private void EnsureBrowserEmulation()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true))
                {
                    string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                    int desired = 11001; // IE11 edge mode
                    object cur = key.GetValue(appName);
                    if (cur == null || (int)cur != desired)
                        key.SetValue(appName, desired, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        private void TryExternalPlay(string rutaVideo)
        {
            try
            {
                var psi = new ProcessStartInfo(rutaVideo) { UseShellExecute = true };
                Process.Start(psi);
                // corta espera y continuar
                var t = new Timer();
                t.Interval = 3000; t.Tick += (s, e) => { t.Stop(); TerminarVideo(); };
                t.Start();
            }
            catch
            {
                TerminarVideo();
            }
        }

        public void TerminarVideo()
        {
            if (!videoTerminado)
            {
                videoTerminado = true;
                try { fallbackTimer?.Stop(); } catch { }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public class ScriptBridge
    {
        private FormSplash parent;
        public ScriptBridge(FormSplash p) { parent = p; }
        public void Finish() => parent.TerminarVideo();
    }
}
