namespace DynamicSepticSystem
{
    partial class FormRegistrarTrabajador
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.grpDatos = new System.Windows.Forms.GroupBox();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblNacionalidad = new System.Windows.Forms.Label();
            this.cmbNacionalidad = new System.Windows.Forms.ComboBox();
            this.lblFechaNac = new System.Windows.Forms.Label();
            this.dtpFechaNacimiento = new System.Windows.Forms.DateTimePicker();
            this.lblCURP = new System.Windows.Forms.Label();
            this.txtCURP = new System.Windows.Forms.TextBox();
            this.lblRFC = new System.Windows.Forms.Label();
            this.txtRFC = new System.Windows.Forms.TextBox();
            this.lblINE = new System.Windows.Forms.Label();
            this.txtINE = new System.Windows.Forms.TextBox();
            this.lblNSS = new System.Windows.Forms.Label();
            this.txtNSS = new System.Windows.Forms.TextBox();
            this.lblFoto = new System.Windows.Forms.Label();
            this.pbFoto = new System.Windows.Forms.PictureBox();
            this.btnCargarFoto = new System.Windows.Forms.Button();
            this.btnQuitarFoto = new System.Windows.Forms.Button();
            this.grpDocs = new System.Windows.Forms.GroupBox();
            this.lblDocCURP = new System.Windows.Forms.Label();
            this.btnCargarCURP = new System.Windows.Forms.Button();
            this.btnVerCURP = new System.Windows.Forms.Button();
            this.lblEstadoCURP = new System.Windows.Forms.Label();
            this.lblDocRFC = new System.Windows.Forms.Label();
            this.btnCargarRFC = new System.Windows.Forms.Button();
            this.btnVerRFC = new System.Windows.Forms.Button();
            this.lblEstadoRFC = new System.Windows.Forms.Label();
            this.lblDocINE = new System.Windows.Forms.Label();
            this.btnCargarINE = new System.Windows.Forms.Button();
            this.btnVerINE = new System.Windows.Forms.Button();
            this.lblEstadoINE = new System.Windows.Forms.Label();
            this.lblDocNSS = new System.Windows.Forms.Label();
            this.btnCargarNSS = new System.Windows.Forms.Button();
            this.btnVerNSS = new System.Windows.Forms.Button();
            this.lblEstadoNSS = new System.Windows.Forms.Label();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.cmbBuscarTrabajador = new System.Windows.Forms.ComboBox();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.grpDatos.SuspendLayout();
            this.grpDocs.SuspendLayout();
            this.SuspendLayout();
            //
            // panelHeader (banda café Calandria)
            //
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(88, 53, 23);
            this.panelHeader.Controls.Add(this.lblSubtitulo);
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(810, 62);
            this.panelHeader.TabIndex = 0;
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(20, 8);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(305, 28);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "REGISTRO DE TRABAJADORES";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(230, 215, 195);
            this.lblSubtitulo.Location = new System.Drawing.Point(22, 36);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(400, 16);
            this.lblSubtitulo.TabIndex = 0;
            this.lblSubtitulo.Text = "Catálogo de trabajadores · datos personales y documentos oficiales";
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblBuscar.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblBuscar.Location = new System.Drawing.Point(450, 75);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(115, 15);
            this.lblBuscar.TabIndex = 1;
            this.lblBuscar.Text = "BUSCAR TRABAJADOR";
            //
            // cmbBuscarTrabajador
            //
            this.cmbBuscarTrabajador.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBuscarTrabajador.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbBuscarTrabajador.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.cmbBuscarTrabajador.Location = new System.Drawing.Point(450, 93);
            this.cmbBuscarTrabajador.Name = "cmbBuscarTrabajador";
            this.cmbBuscarTrabajador.Size = new System.Drawing.Size(340, 25);
            this.cmbBuscarTrabajador.TabIndex = 2;
            //
            // grpDatos
            //
            this.grpDatos.Controls.Add(this.lblClave);
            this.grpDatos.Controls.Add(this.txtClave);
            this.grpDatos.Controls.Add(this.lblNombre);
            this.grpDatos.Controls.Add(this.txtNombre);
            this.grpDatos.Controls.Add(this.lblNacionalidad);
            this.grpDatos.Controls.Add(this.cmbNacionalidad);
            this.grpDatos.Controls.Add(this.lblFechaNac);
            this.grpDatos.Controls.Add(this.dtpFechaNacimiento);
            this.grpDatos.Controls.Add(this.lblCURP);
            this.grpDatos.Controls.Add(this.txtCURP);
            this.grpDatos.Controls.Add(this.lblRFC);
            this.grpDatos.Controls.Add(this.txtRFC);
            this.grpDatos.Controls.Add(this.lblINE);
            this.grpDatos.Controls.Add(this.txtINE);
            this.grpDatos.Controls.Add(this.lblNSS);
            this.grpDatos.Controls.Add(this.txtNSS);
            this.grpDatos.Controls.Add(this.lblFoto);
            this.grpDatos.Controls.Add(this.pbFoto);
            this.grpDatos.Controls.Add(this.btnCargarFoto);
            this.grpDatos.Controls.Add(this.btnQuitarFoto);
            this.grpDatos.BackColor = System.Drawing.Color.White;
            this.grpDatos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpDatos.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.grpDatos.Location = new System.Drawing.Point(15, 130);
            this.grpDatos.Name = "grpDatos";
            this.grpDatos.Size = new System.Drawing.Size(420, 410);
            this.grpDatos.TabIndex = 3;
            this.grpDatos.TabStop = false;
            this.grpDatos.Text = "  Datos personales  ";
            //
            // lblClave
            //
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblClave.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblClave.Location = new System.Drawing.Point(15, 30);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(56, 15);
            this.lblClave.TabIndex = 0;
            this.lblClave.Text = "MATRÍCULA";
            //
            // txtClave
            //
            this.txtClave.BackColor = System.Drawing.Color.FromArgb(250, 246, 240);
            this.txtClave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtClave.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
            this.txtClave.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.txtClave.Location = new System.Drawing.Point(140, 27);
            this.txtClave.Name = "txtClave";
            this.txtClave.ReadOnly = true;
            this.txtClave.Size = new System.Drawing.Size(260, 24);
            this.txtClave.TabIndex = 0;
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = false;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblNombre.Location = new System.Drawing.Point(15, 65);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(120, 18);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "NOMBRE *";
            //
            // txtNombre
            //
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular);
            this.txtNombre.Location = new System.Drawing.Point(140, 62);
            this.txtNombre.MaxLength = 200;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(260, 24);
            this.txtNombre.TabIndex = 1;
            //
            // lblNacionalidad
            //
            this.lblNacionalidad.AutoSize = false;
            this.lblNacionalidad.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblNacionalidad.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblNacionalidad.Location = new System.Drawing.Point(15, 100);
            this.lblNacionalidad.Name = "lblNacionalidad";
            this.lblNacionalidad.Size = new System.Drawing.Size(120, 18);
            this.lblNacionalidad.TabIndex = 0;
            this.lblNacionalidad.Text = "NACIONALIDAD *";
            //
            // cmbNacionalidad
            //
            this.cmbNacionalidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNacionalidad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbNacionalidad.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.cmbNacionalidad.Location = new System.Drawing.Point(140, 97);
            this.cmbNacionalidad.Name = "cmbNacionalidad";
            this.cmbNacionalidad.Size = new System.Drawing.Size(260, 25);
            this.cmbNacionalidad.TabIndex = 2;
            //
            // lblFechaNac
            //
            this.lblFechaNac.AutoSize = false;
            this.lblFechaNac.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblFechaNac.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblFechaNac.Location = new System.Drawing.Point(15, 135);
            this.lblFechaNac.Name = "lblFechaNac";
            this.lblFechaNac.Size = new System.Drawing.Size(120, 18);
            this.lblFechaNac.TabIndex = 0;
            this.lblFechaNac.Text = "FECHA NACIMIENTO";
            //
            // dtpFechaNacimiento
            //
            this.dtpFechaNacimiento.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.dtpFechaNacimiento.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaNacimiento.Location = new System.Drawing.Point(140, 132);
            this.dtpFechaNacimiento.Name = "dtpFechaNacimiento";
            this.dtpFechaNacimiento.Size = new System.Drawing.Size(260, 25);
            this.dtpFechaNacimiento.TabIndex = 3;
            //
            // lblCURP
            //
            this.lblCURP.AutoSize = false;
            this.lblCURP.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCURP.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblCURP.Location = new System.Drawing.Point(15, 175);
            this.lblCURP.Name = "lblCURP";
            this.lblCURP.Size = new System.Drawing.Size(120, 18);
            this.lblCURP.TabIndex = 0;
            this.lblCURP.Text = "CURP";
            //
            // txtCURP
            //
            this.txtCURP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCURP.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCURP.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtCURP.Location = new System.Drawing.Point(140, 172);
            this.txtCURP.MaxLength = 18;
            this.txtCURP.Name = "txtCURP";
            this.txtCURP.Size = new System.Drawing.Size(260, 23);
            this.txtCURP.TabIndex = 4;
            //
            // lblRFC
            //
            this.lblRFC.AutoSize = false;
            this.lblRFC.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblRFC.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblRFC.Location = new System.Drawing.Point(15, 215);
            this.lblRFC.Name = "lblRFC";
            this.lblRFC.Size = new System.Drawing.Size(120, 18);
            this.lblRFC.TabIndex = 0;
            this.lblRFC.Text = "RFC";
            //
            // txtRFC
            //
            this.txtRFC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRFC.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtRFC.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtRFC.Location = new System.Drawing.Point(140, 212);
            this.txtRFC.MaxLength = 13;
            this.txtRFC.Name = "txtRFC";
            this.txtRFC.Size = new System.Drawing.Size(260, 23);
            this.txtRFC.TabIndex = 5;
            //
            // lblINE
            //
            this.lblINE.AutoSize = false;
            this.lblINE.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblINE.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblINE.Location = new System.Drawing.Point(15, 255);
            this.lblINE.Name = "lblINE";
            this.lblINE.Size = new System.Drawing.Size(120, 18);
            this.lblINE.TabIndex = 0;
            this.lblINE.Text = "INE (CLAVE ELECTOR)";
            //
            // txtINE
            //
            this.txtINE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtINE.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtINE.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtINE.Location = new System.Drawing.Point(140, 252);
            this.txtINE.MaxLength = 18;
            this.txtINE.Name = "txtINE";
            this.txtINE.Size = new System.Drawing.Size(260, 23);
            this.txtINE.TabIndex = 6;
            //
            // lblNSS
            //
            this.lblNSS.AutoSize = false;
            this.lblNSS.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblNSS.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblNSS.Location = new System.Drawing.Point(15, 295);
            this.lblNSS.Name = "lblNSS";
            this.lblNSS.Size = new System.Drawing.Size(120, 18);
            this.lblNSS.TabIndex = 0;
            this.lblNSS.Text = "NSS";
            //
            // txtNSS
            //
            this.txtNSS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNSS.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtNSS.Location = new System.Drawing.Point(140, 292);
            this.txtNSS.MaxLength = 11;
            this.txtNSS.Name = "txtNSS";
            this.txtNSS.Size = new System.Drawing.Size(260, 23);
            this.txtNSS.TabIndex = 7;
            //
            // lblFoto
            //
            this.lblFoto.AutoSize = false;
            this.lblFoto.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblFoto.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblFoto.Location = new System.Drawing.Point(15, 335);
            this.lblFoto.Name = "lblFoto";
            this.lblFoto.Size = new System.Drawing.Size(120, 18);
            this.lblFoto.TabIndex = 0;
            this.lblFoto.Text = "FOTOGRAFÍA";
            //
            // pbFoto
            //
            this.pbFoto.BackColor = System.Drawing.Color.FromArgb(250, 246, 240);
            this.pbFoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbFoto.Location = new System.Drawing.Point(140, 330);
            this.pbFoto.Name = "pbFoto";
            this.pbFoto.Size = new System.Drawing.Size(100, 70);
            this.pbFoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbFoto.TabIndex = 8;
            this.pbFoto.TabStop = false;
            //
            // btnCargarFoto
            //
            this.btnCargarFoto.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnCargarFoto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargarFoto.FlatAppearance.BorderSize = 0;
            this.btnCargarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarFoto.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnCargarFoto.ForeColor = System.Drawing.Color.White;
            this.btnCargarFoto.Location = new System.Drawing.Point(250, 330);
            this.btnCargarFoto.Name = "btnCargarFoto";
            this.btnCargarFoto.Size = new System.Drawing.Size(150, 32);
            this.btnCargarFoto.TabIndex = 9;
            this.btnCargarFoto.Text = "CARGAR FOTO";
            this.btnCargarFoto.UseVisualStyleBackColor = false;
            this.btnCargarFoto.Click += new System.EventHandler(this.btnCargarFoto_Click);
            //
            // btnQuitarFoto
            //
            this.btnQuitarFoto.BackColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.btnQuitarFoto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuitarFoto.FlatAppearance.BorderSize = 0;
            this.btnQuitarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitarFoto.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnQuitarFoto.ForeColor = System.Drawing.Color.White;
            this.btnQuitarFoto.Location = new System.Drawing.Point(250, 368);
            this.btnQuitarFoto.Name = "btnQuitarFoto";
            this.btnQuitarFoto.Size = new System.Drawing.Size(150, 32);
            this.btnQuitarFoto.TabIndex = 10;
            this.btnQuitarFoto.Text = "QUITAR FOTO";
            this.btnQuitarFoto.UseVisualStyleBackColor = false;
            this.btnQuitarFoto.Click += new System.EventHandler(this.btnQuitarFoto_Click);
            //
            // grpDocs
            //
            this.grpDocs.Controls.Add(this.lblDocCURP);
            this.grpDocs.Controls.Add(this.btnCargarCURP);
            this.grpDocs.Controls.Add(this.btnVerCURP);
            this.grpDocs.Controls.Add(this.lblEstadoCURP);
            this.grpDocs.Controls.Add(this.lblDocRFC);
            this.grpDocs.Controls.Add(this.btnCargarRFC);
            this.grpDocs.Controls.Add(this.btnVerRFC);
            this.grpDocs.Controls.Add(this.lblEstadoRFC);
            this.grpDocs.Controls.Add(this.lblDocINE);
            this.grpDocs.Controls.Add(this.btnCargarINE);
            this.grpDocs.Controls.Add(this.btnVerINE);
            this.grpDocs.Controls.Add(this.lblEstadoINE);
            this.grpDocs.Controls.Add(this.lblDocNSS);
            this.grpDocs.Controls.Add(this.btnCargarNSS);
            this.grpDocs.Controls.Add(this.btnVerNSS);
            this.grpDocs.Controls.Add(this.lblEstadoNSS);
            this.grpDocs.BackColor = System.Drawing.Color.White;
            this.grpDocs.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpDocs.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.grpDocs.Location = new System.Drawing.Point(450, 130);
            this.grpDocs.Name = "grpDocs";
            this.grpDocs.Size = new System.Drawing.Size(340, 410);
            this.grpDocs.TabIndex = 4;
            this.grpDocs.TabStop = false;
            this.grpDocs.Text = "  Documentos PDF  ";
            //
            // lblDocCURP
            //
            this.lblDocCURP.AutoSize = false;
            this.lblDocCURP.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDocCURP.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblDocCURP.Location = new System.Drawing.Point(15, 35);
            this.lblDocCURP.Name = "lblDocCURP";
            this.lblDocCURP.Size = new System.Drawing.Size(60, 18);
            this.lblDocCURP.TabIndex = 0;
            this.lblDocCURP.Text = "CURP";
            //
            // btnCargarCURP
            //
            this.btnCargarCURP.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnCargarCURP.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargarCURP.FlatAppearance.BorderSize = 0;
            this.btnCargarCURP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarCURP.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnCargarCURP.ForeColor = System.Drawing.Color.White;
            this.btnCargarCURP.Location = new System.Drawing.Point(80, 30);
            this.btnCargarCURP.Name = "btnCargarCURP";
            this.btnCargarCURP.Size = new System.Drawing.Size(90, 30);
            this.btnCargarCURP.TabIndex = 0;
            this.btnCargarCURP.Text = "CARGAR PDF";
            this.btnCargarCURP.UseVisualStyleBackColor = false;
            this.btnCargarCURP.Click += new System.EventHandler(this.btnCargarCURP_Click);
            //
            // btnVerCURP
            //
            this.btnVerCURP.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.btnVerCURP.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVerCURP.FlatAppearance.BorderSize = 0;
            this.btnVerCURP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerCURP.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnVerCURP.ForeColor = System.Drawing.Color.White;
            this.btnVerCURP.Location = new System.Drawing.Point(176, 30);
            this.btnVerCURP.Name = "btnVerCURP";
            this.btnVerCURP.Size = new System.Drawing.Size(60, 30);
            this.btnVerCURP.TabIndex = 1;
            this.btnVerCURP.Text = "VER";
            this.btnVerCURP.UseVisualStyleBackColor = false;
            this.btnVerCURP.Click += new System.EventHandler(this.btnVerCURP_Click);
            //
            // lblEstadoCURP
            //
            this.lblEstadoCURP.AutoSize = false;
            this.lblEstadoCURP.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEstadoCURP.ForeColor = System.Drawing.Color.Gray;
            this.lblEstadoCURP.Location = new System.Drawing.Point(244, 35);
            this.lblEstadoCURP.Name = "lblEstadoCURP";
            this.lblEstadoCURP.Size = new System.Drawing.Size(86, 22);
            this.lblEstadoCURP.TabIndex = 0;
            this.lblEstadoCURP.Text = "○ sin archivo";
            this.lblEstadoCURP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDocRFC
            //
            this.lblDocRFC.AutoSize = false;
            this.lblDocRFC.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDocRFC.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblDocRFC.Location = new System.Drawing.Point(15, 105);
            this.lblDocRFC.Name = "lblDocRFC";
            this.lblDocRFC.Size = new System.Drawing.Size(60, 18);
            this.lblDocRFC.TabIndex = 0;
            this.lblDocRFC.Text = "RFC";
            //
            // btnCargarRFC
            //
            this.btnCargarRFC.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnCargarRFC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargarRFC.FlatAppearance.BorderSize = 0;
            this.btnCargarRFC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarRFC.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnCargarRFC.ForeColor = System.Drawing.Color.White;
            this.btnCargarRFC.Location = new System.Drawing.Point(80, 100);
            this.btnCargarRFC.Name = "btnCargarRFC";
            this.btnCargarRFC.Size = new System.Drawing.Size(90, 30);
            this.btnCargarRFC.TabIndex = 2;
            this.btnCargarRFC.Text = "CARGAR PDF";
            this.btnCargarRFC.UseVisualStyleBackColor = false;
            this.btnCargarRFC.Click += new System.EventHandler(this.btnCargarRFC_Click);
            //
            // btnVerRFC
            //
            this.btnVerRFC.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.btnVerRFC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVerRFC.FlatAppearance.BorderSize = 0;
            this.btnVerRFC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerRFC.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnVerRFC.ForeColor = System.Drawing.Color.White;
            this.btnVerRFC.Location = new System.Drawing.Point(176, 100);
            this.btnVerRFC.Name = "btnVerRFC";
            this.btnVerRFC.Size = new System.Drawing.Size(60, 30);
            this.btnVerRFC.TabIndex = 3;
            this.btnVerRFC.Text = "VER";
            this.btnVerRFC.UseVisualStyleBackColor = false;
            this.btnVerRFC.Click += new System.EventHandler(this.btnVerRFC_Click);
            //
            // lblEstadoRFC
            //
            this.lblEstadoRFC.AutoSize = false;
            this.lblEstadoRFC.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEstadoRFC.ForeColor = System.Drawing.Color.Gray;
            this.lblEstadoRFC.Location = new System.Drawing.Point(244, 105);
            this.lblEstadoRFC.Name = "lblEstadoRFC";
            this.lblEstadoRFC.Size = new System.Drawing.Size(86, 22);
            this.lblEstadoRFC.TabIndex = 0;
            this.lblEstadoRFC.Text = "○ sin archivo";
            this.lblEstadoRFC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDocINE
            //
            this.lblDocINE.AutoSize = false;
            this.lblDocINE.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDocINE.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblDocINE.Location = new System.Drawing.Point(15, 175);
            this.lblDocINE.Name = "lblDocINE";
            this.lblDocINE.Size = new System.Drawing.Size(60, 18);
            this.lblDocINE.TabIndex = 0;
            this.lblDocINE.Text = "INE";
            //
            // btnCargarINE
            //
            this.btnCargarINE.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnCargarINE.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargarINE.FlatAppearance.BorderSize = 0;
            this.btnCargarINE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarINE.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnCargarINE.ForeColor = System.Drawing.Color.White;
            this.btnCargarINE.Location = new System.Drawing.Point(80, 170);
            this.btnCargarINE.Name = "btnCargarINE";
            this.btnCargarINE.Size = new System.Drawing.Size(90, 30);
            this.btnCargarINE.TabIndex = 4;
            this.btnCargarINE.Text = "CARGAR PDF";
            this.btnCargarINE.UseVisualStyleBackColor = false;
            this.btnCargarINE.Click += new System.EventHandler(this.btnCargarINE_Click);
            //
            // btnVerINE
            //
            this.btnVerINE.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.btnVerINE.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVerINE.FlatAppearance.BorderSize = 0;
            this.btnVerINE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerINE.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnVerINE.ForeColor = System.Drawing.Color.White;
            this.btnVerINE.Location = new System.Drawing.Point(176, 170);
            this.btnVerINE.Name = "btnVerINE";
            this.btnVerINE.Size = new System.Drawing.Size(60, 30);
            this.btnVerINE.TabIndex = 5;
            this.btnVerINE.Text = "VER";
            this.btnVerINE.UseVisualStyleBackColor = false;
            this.btnVerINE.Click += new System.EventHandler(this.btnVerINE_Click);
            //
            // lblEstadoINE
            //
            this.lblEstadoINE.AutoSize = false;
            this.lblEstadoINE.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEstadoINE.ForeColor = System.Drawing.Color.Gray;
            this.lblEstadoINE.Location = new System.Drawing.Point(244, 175);
            this.lblEstadoINE.Name = "lblEstadoINE";
            this.lblEstadoINE.Size = new System.Drawing.Size(86, 22);
            this.lblEstadoINE.TabIndex = 0;
            this.lblEstadoINE.Text = "○ sin archivo";
            this.lblEstadoINE.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDocNSS
            //
            this.lblDocNSS.AutoSize = false;
            this.lblDocNSS.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDocNSS.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblDocNSS.Location = new System.Drawing.Point(15, 245);
            this.lblDocNSS.Name = "lblDocNSS";
            this.lblDocNSS.Size = new System.Drawing.Size(60, 18);
            this.lblDocNSS.TabIndex = 0;
            this.lblDocNSS.Text = "NSS";
            //
            // btnCargarNSS
            //
            this.btnCargarNSS.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnCargarNSS.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargarNSS.FlatAppearance.BorderSize = 0;
            this.btnCargarNSS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarNSS.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnCargarNSS.ForeColor = System.Drawing.Color.White;
            this.btnCargarNSS.Location = new System.Drawing.Point(80, 240);
            this.btnCargarNSS.Name = "btnCargarNSS";
            this.btnCargarNSS.Size = new System.Drawing.Size(90, 30);
            this.btnCargarNSS.TabIndex = 6;
            this.btnCargarNSS.Text = "CARGAR PDF";
            this.btnCargarNSS.UseVisualStyleBackColor = false;
            this.btnCargarNSS.Click += new System.EventHandler(this.btnCargarNSS_Click);
            //
            // btnVerNSS
            //
            this.btnVerNSS.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.btnVerNSS.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVerNSS.FlatAppearance.BorderSize = 0;
            this.btnVerNSS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerNSS.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnVerNSS.ForeColor = System.Drawing.Color.White;
            this.btnVerNSS.Location = new System.Drawing.Point(176, 240);
            this.btnVerNSS.Name = "btnVerNSS";
            this.btnVerNSS.Size = new System.Drawing.Size(60, 30);
            this.btnVerNSS.TabIndex = 7;
            this.btnVerNSS.Text = "VER";
            this.btnVerNSS.UseVisualStyleBackColor = false;
            this.btnVerNSS.Click += new System.EventHandler(this.btnVerNSS_Click);
            //
            // lblEstadoNSS
            //
            this.lblEstadoNSS.AutoSize = false;
            this.lblEstadoNSS.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEstadoNSS.ForeColor = System.Drawing.Color.Gray;
            this.lblEstadoNSS.Location = new System.Drawing.Point(244, 245);
            this.lblEstadoNSS.Name = "lblEstadoNSS";
            this.lblEstadoNSS.Size = new System.Drawing.Size(86, 22);
            this.lblEstadoNSS.TabIndex = 0;
            this.lblEstadoNSS.Text = "○ sin archivo";
            this.lblEstadoNSS.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            //
            // panelFooter
            //
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(245, 240, 232);
            this.panelFooter.Controls.Add(this.btnCancelar);
            this.panelFooter.Controls.Add(this.btnEliminar);
            this.panelFooter.Controls.Add(this.btnGuardar);
            this.panelFooter.Controls.Add(this.btnNuevo);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelFooter.Size = new System.Drawing.Size(810, 64);
            this.panelFooter.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarSeparadorFooter);
            //
            // btnGuardar (acción primaria — verde)
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(15, 10);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(200, 42);
            this.btnGuardar.TabIndex = 6;
            this.btnGuardar.Text = "✓ GUARDAR";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnNuevo (acción secundaria — café Calandria)
            //
            this.btnNuevo.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnNuevo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNuevo.FlatAppearance.BorderSize = 0;
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnNuevo.ForeColor = System.Drawing.Color.White;
            this.btnNuevo.Location = new System.Drawing.Point(225, 10);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.Size = new System.Drawing.Size(150, 42);
            this.btnNuevo.TabIndex = 5;
            this.btnNuevo.Text = "+ NUEVO";
            this.btnNuevo.UseVisualStyleBackColor = false;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);
            //
            // btnEliminar (acción destructiva — rojo)
            //
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.btnEliminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.Location = new System.Drawing.Point(515, 10);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(140, 42);
            this.btnEliminar.TabIndex = 7;
            this.btnEliminar.Text = "✕ ELIMINAR";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            //
            // btnCancelar (neutral — gris)
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(665, 10);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(125, 42);
            this.btnCancelar.TabIndex = 8;
            this.btnCancelar.Text = "CANCELAR";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // FormRegistrarTrabajador
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(250, 246, 240);
            this.ClientSize = new System.Drawing.Size(810, 620);
            this.Controls.Add(this.grpDocs);
            this.Controls.Add(this.grpDatos);
            this.Controls.Add(this.cmbBuscarTrabajador);
            this.Controls.Add(this.lblBuscar);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRegistrarTrabajador";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Registro de Trabajadores";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.grpDatos.ResumeLayout(false);
            this.grpDatos.PerformLayout();
            this.grpDocs.ResumeLayout(false);
            this.grpDocs.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.GroupBox grpDatos;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblNacionalidad;
        private System.Windows.Forms.ComboBox cmbNacionalidad;
        private System.Windows.Forms.Label lblFechaNac;
        private System.Windows.Forms.DateTimePicker dtpFechaNacimiento;
        private System.Windows.Forms.Label lblCURP;
        private System.Windows.Forms.TextBox txtCURP;
        private System.Windows.Forms.Label lblRFC;
        private System.Windows.Forms.TextBox txtRFC;
        private System.Windows.Forms.Label lblINE;
        private System.Windows.Forms.TextBox txtINE;
        private System.Windows.Forms.Label lblNSS;
        private System.Windows.Forms.TextBox txtNSS;
        private System.Windows.Forms.GroupBox grpDocs;
        private System.Windows.Forms.Label lblDocCURP;
        private System.Windows.Forms.Button btnCargarCURP;
        private System.Windows.Forms.Button btnVerCURP;
        private System.Windows.Forms.Label lblEstadoCURP;
        private System.Windows.Forms.Label lblDocRFC;
        private System.Windows.Forms.Button btnCargarRFC;
        private System.Windows.Forms.Button btnVerRFC;
        private System.Windows.Forms.Label lblEstadoRFC;
        private System.Windows.Forms.Label lblDocINE;
        private System.Windows.Forms.Button btnCargarINE;
        private System.Windows.Forms.Button btnVerINE;
        private System.Windows.Forms.Label lblEstadoINE;
        private System.Windows.Forms.Label lblDocNSS;
        private System.Windows.Forms.Button btnCargarNSS;
        private System.Windows.Forms.Button btnVerNSS;
        private System.Windows.Forms.Label lblEstadoNSS;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.ComboBox cmbBuscarTrabajador;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblFoto;
        private System.Windows.Forms.PictureBox pbFoto;
        private System.Windows.Forms.Button btnCargarFoto;
        private System.Windows.Forms.Button btnQuitarFoto;
    }
}
