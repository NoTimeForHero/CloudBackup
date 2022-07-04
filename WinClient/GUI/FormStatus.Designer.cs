namespace WinClient
{
    partial class FormStatus
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStatus));
            this.btnMinimize = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.layerMain = new System.Windows.Forms.TableLayoutPanel();
            this.layerFormHeader = new System.Windows.Forms.TableLayoutPanel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.panelBody = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.layerMain.SuspendLayout();
            this.layerFormHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMinimize
            // 
            this.btnMinimize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMinimize.Image = global::WinClient.Properties.Resources.icons8_minimize_window_48;
            this.btnMinimize.Location = new System.Drawing.Point(600, 0);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(48, 50);
            this.btnMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnMinimize.TabIndex = 3;
            this.btnMinimize.TabStop = false;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Image = global::WinClient.Properties.Resources.icons8_close_window_48;
            this.btnClose.Location = new System.Drawing.Point(648, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(48, 50);
            this.btnClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnClose.TabIndex = 4;
            this.btnClose.TabStop = false;
            // 
            // layerMain
            // 
            this.layerMain.ColumnCount = 1;
            this.layerMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layerMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.layerMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.layerMain.Controls.Add(this.layerFormHeader, 0, 0);
            this.layerMain.Controls.Add(this.panelBody, 0, 1);
            this.layerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerMain.Location = new System.Drawing.Point(0, 0);
            this.layerMain.Margin = new System.Windows.Forms.Padding(0);
            this.layerMain.Name = "layerMain";
            this.layerMain.Padding = new System.Windows.Forms.Padding(2);
            this.layerMain.RowCount = 2;
            this.layerMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.layerMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layerMain.Size = new System.Drawing.Size(704, 304);
            this.layerMain.TabIndex = 5;
            // 
            // layerFormHeader
            // 
            this.layerFormHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(234)))), ((int)(((byte)(248)))));
            this.layerFormHeader.ColumnCount = 3;
            this.layerFormHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layerFormHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layerFormHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layerFormHeader.Controls.Add(this.lblFormTitle, 0, 0);
            this.layerFormHeader.Controls.Add(this.btnMinimize, 1, 0);
            this.layerFormHeader.Controls.Add(this.btnClose, 2, 0);
            this.layerFormHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerFormHeader.Location = new System.Drawing.Point(2, 2);
            this.layerFormHeader.Margin = new System.Windows.Forms.Padding(0);
            this.layerFormHeader.Name = "layerFormHeader";
            this.layerFormHeader.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.layerFormHeader.RowCount = 1;
            this.layerFormHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layerFormHeader.Size = new System.Drawing.Size(700, 50);
            this.layerFormHeader.TabIndex = 6;
            // 
            // lblFormTitle
            // 
            this.lblFormTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(193)))), ((int)(((byte)(233)))));
            this.lblFormTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFormTitle.Font = new System.Drawing.Font("Trebuchet MS", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblFormTitle.Location = new System.Drawing.Point(3, 0);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Size = new System.Drawing.Size(594, 50);
            this.lblFormTitle.TabIndex = 0;
            this.lblFormTitle.Text = "lblFormTitle";
            this.lblFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelBody
            // 
            this.panelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(234)))), ((int)(((byte)(248)))));
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(2, 52);
            this.panelBody.Margin = new System.Windows.Forms.Padding(0);
            this.panelBody.Name = "panelBody";
            this.panelBody.Size = new System.Drawing.Size(700, 250);
            this.panelBody.TabIndex = 7;
            // 
            // FormStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(79)))), ((int)(((byte)(114)))));
            this.ClientSize = new System.Drawing.Size(704, 304);
            this.Controls.Add(this.layerMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormStatus";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.layerMain.ResumeLayout(false);
            this.layerFormHeader.ResumeLayout(false);
            this.layerFormHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox btnMinimize;
        private System.Windows.Forms.PictureBox btnClose;
        private System.Windows.Forms.TableLayoutPanel layerMain;
        private System.Windows.Forms.TableLayoutPanel layerFormHeader;
        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Label lblFormTitle;
    }
}

