namespace TheGrammar
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TextBox ApiKeyTextBox = new TextBox();
        private TextBox PrompTextBox = new TextBox();
        private Label ApiKeyTextBoxLabel = new Label();
        private Label PromptTextBoxLabel = new Label();
        private Button SubmitButton = new Button();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            ApiKeyTextBox = new TextBox();
            PrompTextBox = new TextBox();
            ApiKeyTextBoxLabel = new Label();
            PromptTextBoxLabel = new Label();
            SubmitButton = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // ApiKeyTextBox
            // 
            ApiKeyTextBox.Location = new Point(12, 104);
            ApiKeyTextBox.Name = "ApiKeyTextBox";
            ApiKeyTextBox.Size = new Size(391, 23);
            ApiKeyTextBox.TabIndex = 0;
            // 
            // PrompTextBox
            // 
            PrompTextBox.Location = new Point(12, 161);
            PrompTextBox.Multiline = true;
            PrompTextBox.Name = "PrompTextBox";
            PrompTextBox.Size = new Size(391, 139);
            PrompTextBox.TabIndex = 1;
            // 
            // ApiKeyTextBoxLabel
            // 
            ApiKeyTextBoxLabel.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ApiKeyTextBoxLabel.Location = new Point(12, 83);
            ApiKeyTextBoxLabel.Name = "ApiKeyTextBoxLabel";
            ApiKeyTextBoxLabel.Size = new Size(100, 23);
            ApiKeyTextBoxLabel.TabIndex = 2;
            ApiKeyTextBoxLabel.Text = "API Key";
            // 
            // PromptTextBoxLabel
            // 
            PromptTextBoxLabel.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            PromptTextBoxLabel.Location = new Point(12, 135);
            PromptTextBoxLabel.Name = "PromptTextBoxLabel";
            PromptTextBoxLabel.Size = new Size(100, 23);
            PromptTextBoxLabel.TabIndex = 3;
            PromptTextBoxLabel.Text = "Prompt";
            // 
            // SubmitButton
            // 
            SubmitButton.Location = new Point(328, 306);
            SubmitButton.Name = "SubmitButton";
            SubmitButton.Size = new Size(75, 23);
            SubmitButton.TabIndex = 4;
            SubmitButton.Text = "Speichern";
            SubmitButton.Click += SubmitButton_Click;
            // 
            // label1
            // 
            label1.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(391, 52);
            label1.TabIndex = 5;
            label1.Text = "Wenn du Text in die Zwischenablage kopierst und dann STRG + UMSCHALT + Y drückst, wird der kopierte Text automatisch korrigiert und die korrigierte Version in die Zwischenablage gesetzt.";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(418, 341);
            Controls.Add(label1);
            Controls.Add(ApiKeyTextBox);
            Controls.Add(PrompTextBox);
            Controls.Add(ApiKeyTextBoxLabel);
            Controls.Add(PromptTextBoxLabel);
            Controls.Add(SubmitButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Main";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TheGrammar";
            WindowState = FormWindowState.Minimized;
            Load += Main_Load;
            Resize += Main_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
    }
}
