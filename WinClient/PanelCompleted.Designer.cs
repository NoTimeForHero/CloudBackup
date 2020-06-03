namespace WinClient
{
    partial class PanelCompleted
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblShudownHelper = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Trebuchet MS", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(18, 15);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(664, 37);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Отправка архивов завершена!";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTimer
            // 
            this.lblTimer.Font = new System.Drawing.Font("Algerian", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(102)))), ((int)(((byte)(85)))));
            this.lblTimer.Location = new System.Drawing.Point(14, 52);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(668, 68);
            this.lblTimer.TabIndex = 3;
            this.lblTimer.Text = "00:05:27";
            this.lblTimer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(228)))), ((int)(((byte)(215)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(245, 182);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(198, 50);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // lblShudownHelper
            // 
            this.lblShudownHelper.Font = new System.Drawing.Font("Trebuchet MS", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblShudownHelper.Location = new System.Drawing.Point(18, 120);
            this.lblShudownHelper.Name = "lblShudownHelper";
            this.lblShudownHelper.Size = new System.Drawing.Size(655, 36);
            this.lblShudownHelper.TabIndex = 5;
            this.lblShudownHelper.Text = "Осталось до выключения компьютера";
            this.lblShudownHelper.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PanelCompleted
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblShudownHelper);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblStatus);
            this.MaximumSize = new System.Drawing.Size(700, 250);
            this.MinimumSize = new System.Drawing.Size(700, 250);
            this.Name = "PanelCompleted";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.Size = new System.Drawing.Size(700, 250);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label lblStatus;
        internal System.Windows.Forms.Label lblTimer;
        internal System.Windows.Forms.Label lblShudownHelper;
        internal System.Windows.Forms.Button btnClose;
    }
}
