namespace DeepLearningDemoCs
{
    partial class MainViewControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.vmGlobalToolControl1 = new VMControls.Winform.Release.VmGlobalToolControl();
            this.vmMainViewConfigControl1 = new VMControls.Winform.Release.VmMainViewConfigControl();
            this.SuspendLayout();
            // 
            // vmGlobalToolControl1
            // 
            this.vmGlobalToolControl1.BackColor = System.Drawing.SystemColors.Control;
            this.vmGlobalToolControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.vmGlobalToolControl1.Location = new System.Drawing.Point(0, 0);
            this.vmGlobalToolControl1.Name = "vmGlobalToolControl1";
            this.vmGlobalToolControl1.Size = new System.Drawing.Size(754, 39);
            this.vmGlobalToolControl1.TabIndex = 0;
            // 
            // vmMainViewConfigControl1
            // 
            this.vmMainViewConfigControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vmMainViewConfigControl1.Location = new System.Drawing.Point(0, 44);
            this.vmMainViewConfigControl1.Margin = new System.Windows.Forms.Padding(2);
            this.vmMainViewConfigControl1.Name = "vmMainViewConfigControl1";
            this.vmMainViewConfigControl1.Size = new System.Drawing.Size(754, 494);
            this.vmMainViewConfigControl1.TabIndex = 1;
// TODO: Code generation for '' failed because of Exception 'Invalid Primitive Type: System.IntPtr. Consider using CodeObjectCreateExpression.'.
            // 
            // MainViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.vmMainViewConfigControl1);
            this.Controls.Add(this.vmGlobalToolControl1);
            this.Name = "MainViewControl";
            this.Size = new System.Drawing.Size(754, 540);
            this.ResumeLayout(false);

        }

        #endregion

        private VMControls.Winform.Release.VmGlobalToolControl vmGlobalToolControl1;
        private VMControls.Winform.Release.VmMainViewConfigControl vmMainViewConfigControl1;
    }
}
