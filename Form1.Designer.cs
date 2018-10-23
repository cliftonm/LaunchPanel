namespace LaunchPanel
{
    partial class Form1
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
            this.btnExplorer1 = new System.Windows.Forms.Button();
            this.btnBrowser1 = new System.Windows.Forms.Button();
            this.btnBrowser2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnExplorer1
            // 
            this.btnExplorer1.Location = new System.Drawing.Point(13, 13);
            this.btnExplorer1.Name = "btnExplorer1";
            this.btnExplorer1.Size = new System.Drawing.Size(106, 23);
            this.btnExplorer1.TabIndex = 0;
            this.btnExplorer1.Tag = "c:\\temp";
            this.btnExplorer1.Text = "C:\\temp";
            this.btnExplorer1.UseVisualStyleBackColor = true;
            // 
            // btnBrowser1
            // 
            this.btnBrowser1.Location = new System.Drawing.Point(147, 13);
            this.btnBrowser1.Name = "btnBrowser1";
            this.btnBrowser1.Size = new System.Drawing.Size(135, 23);
            this.btnBrowser1.TabIndex = 1;
            this.btnBrowser1.Tag = "https://codeproject.com";
            this.btnBrowser1.Text = "CodeProject";
            this.btnBrowser1.UseVisualStyleBackColor = true;
            // 
            // btnBrowser2
            // 
            this.btnBrowser2.Location = new System.Drawing.Point(147, 43);
            this.btnBrowser2.Name = "btnBrowser2";
            this.btnBrowser2.Size = new System.Drawing.Size(135, 23);
            this.btnBrowser2.TabIndex = 2;
            this.btnBrowser2.Tag = "https://www.codeproject.com/Lounge.aspx";
            this.btnBrowser2.Text = "CP Lounge";
            this.btnBrowser2.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnBrowser2);
            this.Controls.Add(this.btnBrowser1);
            this.Controls.Add(this.btnExplorer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnExplorer1;
        private System.Windows.Forms.Button btnBrowser1;
        private System.Windows.Forms.Button btnBrowser2;
    }
}

