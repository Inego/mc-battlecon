using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace BattleCON
{
    public class GameState
    {
        public bool isMainGame;
        public List<string> consoleBuffer;
        public BackgroundWorker bw;
        public EventWaitHandle waitHandle;


        public Player p1;
        public Player p2;

        Player firstToAnte;

        public int beat;

        //public Random rnd = new Random(9);
        public Random rnd = new Random();

        public GameState(Character c1, Character c2,  BackgroundWorker bw, EventWaitHandle waitHandle)
        {
            beat = 1;

            p1 = new Player(c1, 2, this, true);
            p2 = new Player(c2, 6, this, false);

            firstToAnte = p1;

            p1.opponent = p2;
            p2.opponent = p1;

            isMainGame = true;
            consoleBuffer = new List<string>();

            this.bw = bw;
            this.waitHandle = waitHandle;
        }

        internal int playout()
        {
            while (beat <= 15)
            {
                if (isMainGame)
                    writeToConsole("BEAT " + beat);
                
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

        public void flushConsole()
        {
            bw.ReportProgress(0);
            waitHandle.WaitOne();

        }

        public void writeToConsole(string p)
        {
            consoleBuffer.Add(p);
        }

        private void nextBeat()
        {
            if (isMainGame)
            {
                writeToConsole(p1 + " " + p1.health);
                writeToConsole(p2 + " " + p2.health);
            }

            flushConsole();

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

            if (isMainGame)
                writeToConsole("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());

            while (p1.priority() == p2.priority())
            {

                if (isMainGame)
                    writeToConsole("CLASH! (both players " + p1.priority() + ')');

                if (p1.bases.Count == 0 || p2.bases.Count == 0)
                {
                    normalPlay = false;
                    break;
                }

                p1.selectNextForClash();
                p2.selectNextForClash();

                if (isMainGame)
                {
                    writeToConsole(p1 + " selected " + p1.attackStyle + ' ' + p1.attackBase);
                    writeToConsole(p2 + " selected " + p2.attackStyle + ' ' + p2.attackBase);
                    writeToConsole("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());
                }


            }

            if (normalPlay)
            {
                Player activePlayer = p1.priority() > p2.priority() ? p1 : p2;
                Player reactivePlayer = activePlayer.opponent;

                if (isMainGame)
                    writeToConsole(activePlayer + " goes first");

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
                        if (isMainGame)
                            writeToConsole(reactivePlayer + " is stunned and can't respond.");
                    }
                }
                else
                {
                    if (isMainGame)
                        writeToConsole(activePlayer + " cannot attack since (s)he is stunned!");

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
