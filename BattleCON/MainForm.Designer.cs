namespace BattleCON
{
    partial class MainForm
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
            this.gameLogListBox = new System.Windows.Forms.ListBox();
            this.startButton = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.userChoiceListBox = new System.Windows.Forms.ListBox();
            this.userChoiceListLabel = new System.Windows.Forms.Label();
            this.gameLogListBoxLabel = new System.Windows.Forms.Label();
            this.battleBoard = new BattleCON.BattleBoard();
            this.SuspendLayout();
            // 
            // gameLogListBox
            // 
            this.gameLogListBox.FormattingEnabled = true;
            this.gameLogListBox.Location = new System.Drawing.Point(718, 350);
            this.gameLogListBox.Name = "gameLogListBox";
            this.gameLogListBox.Size = new System.Drawing.Size(392, 355);
            this.gameLogListBox.TabIndex = 1;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(718, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 2;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // userChoiceListBox
            // 
            this.userChoiceListBox.FormattingEnabled = true;
            this.userChoiceListBox.Location = new System.Drawing.Point(718, 55);
            this.userChoiceListBox.Name = "userChoiceListBox";
            this.userChoiceListBox.Size = new System.Drawing.Size(392, 277);
            this.userChoiceListBox.TabIndex = 5;
            this.userChoiceListBox.Click += new System.EventHandler(this.userChoiceListBox_Click);
            // 
            // userChoiceListLabel
            // 
            this.userChoiceListLabel.AutoSize = true;
            this.userChoiceListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.userChoiceListLabel.Location = new System.Drawing.Point(718, 42);
            this.userChoiceListLabel.Name = "userChoiceListLabel";
            this.userChoiceListLabel.Size = new System.Drawing.Size(114, 13);
            this.userChoiceListLabel.TabIndex = 6;
            this.userChoiceListLabel.Text = "<User Choice List>";
            // 
            // gameLogListBoxLabel
            // 
            this.gameLogListBoxLabel.AutoSize = true;
            this.gameLogListBoxLabel.Location = new System.Drawing.Point(718, 335);
            this.gameLogListBoxLabel.Name = "gameLogListBoxLabel";
            this.gameLogListBoxLabel.Size = new System.Drawing.Size(56, 13);
            this.gameLogListBoxLabel.TabIndex = 7;
            this.gameLogListBoxLabel.Text = "Game Log";
            // 
            // battleBoard
            // 
            this.battleBoard.Location = new System.Drawing.Point(12, 12);
            this.battleBoard.Name = "battleBoard";
            this.battleBoard.Size = new System.Drawing.Size(700, 688);
            this.battleBoard.TabIndex = 4;
            this.battleBoard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.battleBoard_MouseMove);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1122, 712);
            this.Controls.Add(this.gameLogListBoxLabel);
            this.Controls.Add(this.userChoiceListLabel);
            this.Controls.Add(this.userChoiceListBox);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.gameLogListBox);
            this.Controls.Add(this.battleBoard);
            this.Name = "MainForm";
            this.Text = "BattleCON Monte Carlo Engine";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox gameLogListBox;
        private System.Windows.Forms.Button startButton;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private BattleBoard battleBoard;
        private System.Windows.Forms.ListBox userChoiceListBox;
        private System.Windows.Forms.Label userChoiceListLabel;
        private System.Windows.Forms.Label gameLogListBoxLabel;
    }
}

