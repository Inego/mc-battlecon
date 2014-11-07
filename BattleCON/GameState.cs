using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{
    public class GameState
    {
        Player p1;
        Player p2;

        Player firstToAnte;

        public int beat;

        //public Random rnd = new Random(9);
        public Random rnd = new Random();

        public GameState(Character c1, Character c2)
        {
            beat = 1;

            p1 = new Player(c1, 2);
            p2 = new Player(c2, 6);

            firstToAnte = p1;

            p1.opponent = p2;
            p2.opponent = p1;

            p1.g = this;
            p2.g = this;
        }

        internal int playout()
        {

            while (beat <= 15)
            {
                Console.WriteLine("BEAT " + beat);
                
                this.nextBeat();

                if (p1.isDead)
                    return 2;

                if (p2.isDead)
                    return 1;

            }

            if (p1.health < p2.health)
                return 2;

            if (p1.health > p2.health)
                return 1;

            // DRAW!
            return 0;

        }

        private void nextBeat()
        {
            Console.WriteLine(p1 + " " + p1.health);
            Console.WriteLine(p2 + " " + p2.health);


            // Select random style

            p1.selectAttackingPair();
            p2.selectAttackingPair();

            antePhase();

            p1.AnteEffects();
            p2.AnteEffects();

            // Reveal cards

            p1.RevealEffects();
            p2.RevealEffects();

            // Determine a clash

            bool normalPlay = true;

            Console.WriteLine("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());

            while (p1.priority() == p2.priority())
            {

                Console.WriteLine("CLASH!");

                if (p1.bases.Count == 0 || p2.bases.Count == 0)
                {
                    normalPlay = false;
                    break;
                }

                p1.selectNextForClash();
                p2.selectNextForClash();

                Console.WriteLine(p1 + " selected " + p1.attackStyle + ' ' + p1.attackBase);
                Console.WriteLine(p2 + " selected " + p2.attackStyle + ' ' + p2.attackBase);
                Console.WriteLine("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());


            }

            if (normalPlay)
            {
                Player activePlayer = p1.priority() > p2.priority() ? p1 : p2;
                Player reactivePlayer = activePlayer.opponent;

                Console.WriteLine(activePlayer + " goes first");

                activePlayer.applyCommonProperties();
                reactivePlayer.applyCommonProperties();

                activePlayer.resolveStartOfBeat();
                reactivePlayer.resolveStartOfBeat();


                if (!activePlayer.isStunned)
                {
                    activePlayer.attack(true);

                    if (!reactivePlayer.isDead && !reactivePlayer.isStunned)
                    {
                        reactivePlayer.attack(false);
                    }
                    else {
                        Console.WriteLine(reactivePlayer + " is stunned and can't respond.");
                    }
                }
                else
                {
                    Console.WriteLine(activePlayer + " cannot attack since (s)he is stunned!");

                }


                // End of beat
                firstToAnte = activePlayer;

                activePlayer.resolveEndOfBeat();
                reactivePlayer.resolveEndOfBeat();

            }

            p1.recycle();
            p2.recycle();

            p1.resetBeat();
            p2.resetBeat();

            beat++;
        }

        private void antePhase()
        {
            bool previous = true;

            Player anteingPlayer = firstToAnte;


            while (true)
            {
                bool current = anteingPlayer.ante();

                if (!current && !previous)
                    // both passed
                    break;

                anteingPlayer = anteingPlayer.opponent;
                previous = current;

            }


        }
    }


}
