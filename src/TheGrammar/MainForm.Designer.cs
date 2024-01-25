namespace TheGrammar
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            SuspendLayout();
            // 
            // blazorWebView1
            // 
            blazorWebView1.Dock = DockStyle.Fill;
            blazorWebView1.Location = new Point(0, 0);
            blazorWebView1.Name = "blazorWebView1";
            blazorWebView1.Size = new Size(699, 732);
            blazorWebView1.TabIndex = 6;
            blazorWebView1.Text = "blazorWebView1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(699, 732);
            Controls.Add(blazorWebView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TheGrammar";
            WindowState = FormWindowState.Minimized;
            FormClosing += Main_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += Main_Load;
            Resize += Main_Resize;
            ResumeLayout(false);
        }

        #endregion
        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView1;
    }
}
