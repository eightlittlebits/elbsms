namespace elbsms_ui
{
    partial class ConfigurationForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.configPanel = new System.Windows.Forms.Panel();
            this.configTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.configPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(282, 362);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(363, 362);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // configPanel
            // 
            this.configPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configPanel.AutoScroll = true;
            this.configPanel.Controls.Add(this.configTableLayoutPanel);
            this.configPanel.Location = new System.Drawing.Point(12, 12);
            this.configPanel.Name = "configPanel";
            this.configPanel.Size = new System.Drawing.Size(426, 344);
            this.configPanel.TabIndex = 2;
            // 
            // configTableLayoutPanel
            // 
            this.configTableLayoutPanel.AutoSize = true;
            this.configTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.configTableLayoutPanel.ColumnCount = 4;
            this.configTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.configTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.configTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.configTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.configTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.configTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.configTableLayoutPanel.Name = "configTableLayoutPanel";
            this.configTableLayoutPanel.RowCount = 1;
            this.configTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.configTableLayoutPanel.Size = new System.Drawing.Size(426, 0);
            this.configTableLayoutPanel.TabIndex = 0;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(450, 397);
            this.Controls.Add(this.configPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Name = "ConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ConfigurationForm";
            this.configPanel.ResumeLayout(false);
            this.configPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel configPanel;
        private System.Windows.Forms.TableLayoutPanel configTableLayoutPanel;
    }
}