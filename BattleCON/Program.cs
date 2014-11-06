using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    

  
    
    public class Card
    {
        internal string name;

        public int lowRange = 0;
        public int hiRange = 0;
        public int power = 0;
        public int priority = 0;

        virtual public void CommonProperties(Player p)
        {

        }

        virtual public void Reveal(Player p)
        {

        }

        virtual public void StartOfBeat(Player p)
        {

        }

        virtual public void BeforeActivating(Player p)
        {

        }


        virtual public void OnHit(Player p)
        {

        }

        virtual public void OnSoak(Player p)
        {
        }

        virtual public void OnDamage(Player p)
        {

        }

        virtual public void AfterActivating(Player p)
        {

        }

        virtual public void EndOfBeat(Player p)
        {

        }

        virtual public int getAttackPower(Player p)
        {
            return this.power;
        }

        virtual public bool ignoresStunGuard(Player p)
        {
            return false;
        }

        virtual public bool ignoresSoak(Player p)
        {
            return false;
        }

        virtual public void checkCanHit(Player p)
        {
        }

        public override string ToString()
        {
            return name;
        }


    }

    

    public class Character
    {
        public string name;

        public static Character shekhtur;
        public static Character eligor;
        
        static Character() {

            shekhtur = new Shekhtur();
            eligor = new Eligor();
            
        }

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

   
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i <= 1000; i++)
            {
                GameState g = new GameState(Character.shekhtur, Character.eligor);

                g.playout();
            }
            
            

            Console.ReadLine();
        }
        
    }
}
