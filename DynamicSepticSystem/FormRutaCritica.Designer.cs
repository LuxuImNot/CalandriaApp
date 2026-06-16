namespace DynamicSepticSystem
{
    partial class FormRutaCritica
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

        #region C�digo generado por el Dise�ador de Windows Forms

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeListView1 = new DynamicSepticSystem.SafeTreeListView();
            this.Concepto = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.WBS = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Nombre = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.progressBarGenCon = new System.Windows.Forms.ProgressBar();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.lblPorcentajeGEN = new System.Windows.Forms.Label();
            this.lblLastDestajo = new MaterialSkin.Controls.MaterialLabel();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeListView1
            // 
            this.treeListView1.AllColumns.Add(this.Concepto);
            this.treeListView1.AllColumns.Add(this.WBS);
            this.treeListView1.AllColumns.Add(this.Nombre);
            this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Concepto,
            this.WBS,
            this.Nombre});
            this.treeListView1.HideSelection = false;
            this.treeListView1.Location = new System.Drawing.Point(12, 12);
            this.treeListView1.Name = "treeListView1";
            this.treeListView1.OwnerDraw = true;
            this.treeListView1.ShowGroups = false;
            this.treeListView1.Size = new System.Drawing.Size(1016, 572);
            this.treeListView1.TabIndex = 0;
            this.treeListView1.UseCompatibleStateImageBehavior = false;
            this.treeListView1.View = System.Windows.Forms.View.Details;
            this.treeListView1.VirtualMode = true;
            // 
            // Concepto
            // 
            this.Concepto.AspectName = "Concepto";
            this.Concepto.CellPadding = null;
            this.Concepto.DisplayIndex = 1;
            this.Concepto.Text = "Concepto";
            // 
            // WBS
            // 
            this.WBS.AspectName = "WBS";
            this.WBS.CellPadding = null;
            this.WBS.DisplayIndex = 0;
            this.WBS.Text = "ID";
            // 
            // Nombre
            // 
            this.Nombre.AspectName = "Nombre";
            this.Nombre.CellPadding = null;
            this.Nombre.Text = "Tarea";
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(12, 601);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(160, 19);
            this.materialLabel1.TabIndex = 1;
            this.materialLabel1.Text = "PROGRESO GENERAL:";
            // 
            // progressBarGenCon
            // 
            this.progressBarGenCon.Location = new System.Drawing.Point(15, 623);
            this.progressBarGenCon.Name = "progressBarGenCon";
            this.progressBarGenCon.Size = new System.Drawing.Size(613, 35);
            this.progressBarGenCon.TabIndex = 2;
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.Location = new System.Drawing.Point(631, 587);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(179, 19);
            this.materialLabel2.TabIndex = 3;
            this.materialLabel2.Text = "Ultimo destajo terminado";
            // 
            // lblPorcentajeGEN
            // 
            this.lblPorcentajeGEN.Location = new System.Drawing.Point(565, 587);
            this.lblPorcentajeGEN.Name = "lblPorcentajeGEN";
            this.lblPorcentajeGEN.Size = new System.Drawing.Size(60, 35);
            this.lblPorcentajeGEN.TabIndex = 5;
            this.lblPorcentajeGEN.Text = "0%";
            this.lblPorcentajeGEN.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLastDestajo
            // 
            this.lblLastDestajo.AutoEllipsis = true;
            this.lblLastDestajo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLastDestajo.Depth = 0;
            this.lblLastDestajo.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblLastDestajo.Location = new System.Drawing.Point(634, 623);
            this.lblLastDestajo.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblLastDestajo.Name = "lblLastDestajo";
            this.lblLastDestajo.Size = new System.Drawing.Size(400, 46);
            this.lblLastDestajo.TabIndex = 4;
            this.lblLastDestajo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormRutaCritica
            // 
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.lblPorcentajeGEN);
            this.Controls.Add(this.lblLastDestajo);
            this.Controls.Add(this.materialLabel2);
            this.Controls.Add(this.progressBarGenCon);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.treeListView1);
            this.Name = "FormRutaCritica";
            this.Text = "Ruta Cr�tica";
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DynamicSepticSystem.SafeTreeListView treeListView1;
        private BrightIdeasSoftware.OLVColumn WBS;
        private BrightIdeasSoftware.OLVColumn Nombre;
        private BrightIdeasSoftware.OLVColumn Concepto;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private System.Windows.Forms.ProgressBar progressBarGenCon;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private System.Windows.Forms.Label lblPorcentajeGEN;
        private MaterialSkin.Controls.MaterialLabel lblLastDestajo;
    }
}

