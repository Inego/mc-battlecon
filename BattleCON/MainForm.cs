using System;
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
    public partial class MainForm : Form
    {

        GameState currentGame;

        static EventWaitHandle _waitHandle = new AutoResetEvent(false);

        bool waitingForButton = false;


        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {

            if (currentGame == null)
            {
                currentGame = new GameState(Character.shekhtur, Character.eligor, backgroundWorker1, _waitHandle);
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                if (waitingForButton)
                {
                    waitingForButton = false;
                    _waitHandle.Set();
                }
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            currentGame.playout();

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            foreach(string s in currentGame.consoleBuffer)
            {
                listBox1.Items.Add(s);
            }

            currentGame.consoleBuffer.Clear();

            listBox1.SelectedIndex = listBox1.Items.Count - 1;

            waitingForButton = true;

        }
    }
}
