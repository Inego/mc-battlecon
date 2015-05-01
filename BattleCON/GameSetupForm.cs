using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleCON
{
    public partial class GameSetupForm : Form
    {

        public GameSettings gameSettings = null;


        public GameSetupForm()
        {
            InitializeComponent();
        }


        private void GameSetupForm_Load(object sender, EventArgs e)
        {
            initializeCharacterCB(character1CB);
            initializeCharacterCB(character2CB);
        }


        private void initializeCharacterCB(ComboBox cCB)
        {
            for (int i = 0; i < CharacterClass.characters.Length; i++)
            {
                cCB.Items.Add(CharacterClass.characters[i].name);
            }

        }


        private void reverseCharactersBtn_Click(object sender, EventArgs e)
        {
            string c = character1CB.Text;
            character1CB.Text = character2CB.Text;
            character2CB.Text = c;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();

            // Check if possible

            gameSettings = new GameSettings();

            CharacterClass c1 = CharacterClass.getByName(character1CB.Text);

            if (c1 == null)
            {
                MessageBox.Show("Player 1: Wrong character!");
                return;
            }

            CharacterClass c2 = CharacterClass.getByName(character2CB.Text);

            if (c2 == null)
            {
                MessageBox.Show("Player 2: Wrong character!");
                return;
            }

            gameSettings = new GameSettings();
            gameSettings.c1 = c1;
            gameSettings.c2 = c2;

            GameState.MAX_PLAYOUTS = (int) Properties.Settings.Default.kPlayouts * 1000;

            GameState.DEBUG_MESSAGES = Properties.Settings.Default.debugMessages;

            Close();

            

        }

      

        
    }
}
