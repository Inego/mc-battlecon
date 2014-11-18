using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleCON
{

    enum Direction { Forward, Backward, Both };

    public class MovementResult
    {
        public bool advance; // false if retreat
        public int distance;
        public bool pastOpponent;

        public static MovementResult noMovement = new MovementResult(true, 0, false);

        public MovementResult(bool advance, int distance, bool pastOpponent)
        {
            this.advance = advance;
            this.distance = distance;
            this.pastOpponent = pastOpponent;

        }


    }

    

  
    
    



    public abstract class Character
    {
        public string name;

        public Finisher finisher1;
        public Finisher finisher2;

        public static Character shekhtur;
        public static Character eligor;

        static Character()
        {

            shekhtur = new Shekhtur();
            eligor = new Eligor();

        }

        public abstract string getDescription();


        virtual public void init(Player p)
        {
        }

        virtual public void OnDamage(Player p)
        {
        }

        virtual public void OnDamageTaken(Player p)
        {
        }

        virtual public void AnteEffects(Player p)
        {
        }
    }


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
