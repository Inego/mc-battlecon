﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BattleCON
{

    public enum UserInteractionTypes
    {
        None,
        Wait,
        Choice
    }


    public partial class MainForm : Form
    {

        GameState currentGame;

        static EventWaitHandle _waitHandle = new AutoResetEvent(false);

        UserInteractionTypes userInteractionType = UserInteractionTypes.None;


        public MainForm()
        {
            InitializeComponent();
        }


        public void startNewGame()
        {

            using (GameSetupForm gsf = new GameSetupForm())
            {
                gsf.ShowDialog();

                GameSettings gameSettings = gsf.gameSettings;

                if (gameSettings != null)
                {
                    currentGame = new GameState(gameSettings.c1, gameSettings.c2, GameVariant.AnteFinishers, backgroundWorker1, _waitHandle);
                    currentGame.pureRandom = true;
                    //currentGame.p1.health = 7;
                    //currentGame.p2.health = 7;
                    battleBoard.gs = currentGame;
                    backgroundWorker1.RunWorkerAsync();
                    
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {

            if (currentGame == null)
            {
                startNewGame();
            }
            else
            {
                switch (userInteractionType)
                {
                    case UserInteractionTypes.Wait:
                        userInteractionType = UserInteractionTypes.None;
                        _waitHandle.Set();
                        break;

                    case UserInteractionTypes.Choice:
                        processUserChoice();
                        break;

                }


                
            }

        }

        private void processUserChoice()
        {
            if (userChoiceListBox.SelectedIndex < 0)
                return;

            currentGame.selectionResult = userChoiceListBox.SelectedIndex;


            currentGame.selectionItems.Clear();
            userChoiceListBox.Items.Clear();
            userChoiceListLabel.Text = "";

            userInteractionType = UserInteractionTypes.None;

            currentGame.sss = SpecialSelectionStyle.None;

            _waitHandle.Set();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            currentGame.playout();

            currentGame.isFinished = true;

            currentGame.writeToConsole("The game is over. You may now start a new one.");

        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            int eventType = e.ProgressPercentage;

            if (eventType == 2)
            {
                playoutNumberLabel.Text = currentGame.playoutsDone.ToString();
                bestWinrateLabel.Text = currentGame.bestWinrate.ToString();

                return;
            }

            startButton.Enabled = (eventType != 3);


            outputCurrentGameConsole();

            gameLogListBox.SelectedIndex = gameLogListBox.Items.Count - 1;

            battleBoard.Redraw(true);

            

            if (eventType == 1)
            {
                userInteractionType = UserInteractionTypes.Choice;
                userChoiceListLabel.Text = currentGame.selectionHeader;
                foreach (string s in currentGame.selectionItems)
                {
                    userChoiceListBox.Items.Add(s);
                }
            }
            else if (eventType == 0)

            {
                userInteractionType = UserInteractionTypes.Wait;


                if (!(currentGame.p1.isHuman || currentGame.p2.isHuman))
                    startButton.PerformClick();
            }

            

        }

        private void outputCurrentGameConsole()
        {
            foreach (string s in currentGame.consoleBuffer)
            {
                gameLogListBox.Items.Add(s);
            }

            currentGame.consoleBuffer.Clear();
        }

        private void battleBoard_MouseMove(object sender, MouseEventArgs e)
        {
            battleBoard.checkMouseMove(e.X, e.Y);
        }


        private void userChoiceListBox_Click(object sender, EventArgs e)
        {
            if (userChoiceListBox.SelectedIndex >= 0)
                startButton.PerformClick();
        }

        private void battleBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (battleBoard.currentRegion != null)
            {
                CardOnScreen card = battleBoard.currentRegion as CardOnScreen;

                if (card != null && card.highlightable)
                {
                    // Look for the corresponding item in the list and select it
                    for (int i = 0; i < userChoiceListBox.Items.Count; i++)
                    {
                        if (userChoiceListBox.Items[i].ToString() == card.c.name)
                        {
                            userChoiceListBox.SelectedIndex = i;
                            startButton.PerformClick();
                            battleBoard.Redraw(true);
                            break;
                        }
                    }
                }
            }
                

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // save 1 click
            startButton.PerformClick();
        }

        private void newGameBtn_Click(object sender, EventArgs e)
        {
            
            if (currentGame != null && !currentGame.isFinished)
            {

                if (currentGame.p1.isDead)
                {
                    MessageBox.Show("Play to the end of the current beat to start a new game.", "New Game");
                    return;
                }
                else
                {
                    DialogResult dr = MessageBox.Show("To start a new game, you have to resign and play to the end of the current beat. Proceed?", "New Game", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No)
                        return;
                    currentGame.p1.isDead = true;
                    currentGame.writeToConsole(currentGame.p1 + " has resigned. Now play to the end of this beat to finish the game.");
                    return;
                }

                    
            }
            startNewGame();

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            outputCurrentGameConsole();
        }

        
        
    }
}
