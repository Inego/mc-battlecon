namespace BattleCON
{
    partial class GameSetupForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.reverseCharactersBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.character2CB = new System.Windows.Forms.ComboBox();
            this.character1CB = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Human";
            // 
            // reverseCharactersBtn
            // 
            this.reverseCharactersBtn.Location = new System.Drawing.Point(143, 19);
            this.reverseCharactersBtn.Name = "reverseCharactersBtn";
            this.reverseCharactersBtn.Size = new System.Drawing.Size(33, 23);
            this.reverseCharactersBtn.TabIndex = 2;
            this.reverseCharactersBtn.Text = "<->";
            this.reverseCharactersBtn.UseVisualStyleBackColor = true;
            this.reverseCharactersBtn.Click += new System.EventHandler(this.reverseCharactersBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(179, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Computer";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Location = new System.Drawing.Point(16, 48);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(205, 48);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MCTS Settings";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(243, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(63, 31);
            this.button1.TabIndex = 6;
            this.button1.Text = "FIGHT!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "kPlayouts";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BattleCON.Properties.Settings.Default, "kPlayouts", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown1.Location = new System.Drawing.Point(65, 19);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown1.TabIndex = 6;
            this.numericUpDown1.Value = global::BattleCON.Properties.Settings.Default.kPlayouts;
            // 
            // character2CB
            // 
            this.character2CB.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.character2CB.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.character2CB.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::BattleCON.Properties.Settings.Default, "Character2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.character2CB.FormattingEnabled = true;
            this.character2CB.Location = new System.Drawing.Point(182, 19);
            this.character2CB.Name = "character2CB";
            this.character2CB.Size = new System.Drawing.Size(121, 21);
            this.character2CB.TabIndex = 3;
            this.character2CB.Text = global::BattleCON.Properties.Settings.Default.Character2;
            // 
            // character1CB
            // 
            this.character1CB.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.character1CB.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.character1CB.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::BattleCON.Properties.Settings.Default, "Character1", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.character1CB.FormattingEnabled = true;
            this.character1CB.Location = new System.Drawing.Point(16, 19);
            this.character1CB.Name = "character1CB";
            this.character1CB.Size = new System.Drawing.Size(121, 21);
            this.character1CB.TabIndex = 1;
            this.character1CB.Text = global::BattleCON.Properties.Settings.Default.Character1;
            // 
            // GameSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 107);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.character2CB);
            this.Controls.Add(this.reverseCharactersBtn);
            this.Controls.Add(this.character1CB);
            this.Controls.Add(this.label1);
            this.Name = "GameSetupForm";
            this.Text = "Game Setup";
            this.Load += new System.EventHandler(this.GameSetupForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox character1CB;
        private System.Windows.Forms.Button reverseCharactersBtn;
        private System.Windows.Forms.ComboBox character2CB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
    }
}