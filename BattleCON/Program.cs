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
    

    public enum Character
    {
        Generic,
        Shekhtur,
        Eligor,
        Marmelee
    }


    public class CharacterClass
    {
        public Character c;
        public string name;
        public Finisher finisher1;
        public Finisher finisher2;

        public static CharacterClass CharacterEligor   = new CharacterClass(Character.Eligor,   "Eligor", new SheetLightning(), new SweetRevenge());
        public static CharacterClass CharacterShekhtur = new CharacterClass(Character.Shekhtur, "Shekhtur", new SoulBreaker(), new CoffinNails());
        public static CharacterClass CharacterMarmelee = new CharacterClass(Character.Marmelee, "Marmelee", new AstralTrance(), new AstralCannon());

        public static CharacterClass[] characters = new CharacterClass[] {
            CharacterEligor,
            CharacterShekhtur,
            CharacterMarmelee
        };

        public CharacterClass(Character c, string name, Finisher finisher1, Finisher finisher2)
        {
            this.c = c;
            this.name = name;
            this.finisher1 = finisher1;
            this.finisher2 = finisher2;
        }

        internal static CharacterClass From(Character c)
        {
            switch (c)
            {
                case Character.Eligor:
                    return CharacterEligor;
                case Character.Shekhtur:
                    return CharacterShekhtur;
                default:
                    return null;
            }
        }

        internal static CharacterClass getByName(string p)
        {
            foreach (CharacterClass c in characters)
                if (c.name == p)
                    return c;
            return null;

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
