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

            while (p1.priority() == p2.priority())
            {
                if (p1.bases.Count == 0 || p2.bases.Count == 0)
                {
                    normalPlay = false;
                    break;
                }

                p1.selectNextForClash();
                p2.selectNextForClash();

            }

            if (normalPlay)
            {
                Player activePlayer = p1.priority() > p2.priority() ? p1 : p2;
                Player reactivePlayer = activePlayer.opponent;

                activePlayer.resolveStartOfBeat();
                reactivePlayer.resolveStartOfBeat();

                //........

                // End of beat
                firstToAnte = p1.priority() > p2.priority() ? p1 : p2;

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
