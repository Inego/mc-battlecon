﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace BattleCON
{

    public class MCTS_Node
    {
        public int games = 0;
        public int wins = 0;
        public MCTS_Node[] children;
        public MCTS_Node parent = null;
    }


    


    public enum SpecialSelectionStyle
    {
        None,
        Styles,
        Bases
    }
    
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

        //public Random rnd = new Random(2);
        public Random rnd = new Random();


        // Game - Interface interaction
        public string selectionHeader;
        public List<string> selectionItems = new List<string>();
        public int selectionResult;
        public SpecialSelectionStyle sss;
        public Player selectionPlayer;
        
        
        // Monte Carlo Tree Search

        public static int MAX_PLAYOUTS = 10000;

        private MCTS_Node rootNode;
        

        public GameState(Character c1, Character c2,  BackgroundWorker bw, EventWaitHandle waitHandle)
        {
            beat = 1;

            p1 = new Player(c1, 2, this, true, true);
            p2 = new Player(c2, 6, this, false, false);

            firstToAnte = p1;

            p1.opponent = p2;
            p2.opponent = p1;

            isMainGame = true;
            consoleBuffer = new List<string>();

            this.bw = bw;
            this.waitHandle = waitHandle;
        }

        public void fillFromGameState(GameState g)
        {
            p1.fillFromPlayer(g.p1);
            p2.fillFromPlayer(g.p2);
            firstToAnte = g.firstToAnte;
            beat = g.beat;
            
        }

        // Cloning
        public GameState(GameState gameState)
        {
            p1 = new Player(gameState.p1.c, this);
            p2 = new Player(gameState.p2.c, this);

            rnd = gameState.rnd;

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

                if (!(p1.isHuman || p2.isHuman))
                    flushConsole();
            }

            

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

                    
                }
                else
                {
                    if (isMainGame)
                        writeToConsole(activePlayer + " cannot attack since (s)he is stunned!");

                }

                if (!reactivePlayer.isDead && !reactivePlayer.isStunned)
                {
                    reactivePlayer.attack(false);
                }
                else
                {
                    if (isMainGame && reactivePlayer.isStunned)
                        writeToConsole(reactivePlayer + " is stunned and can't respond.");
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

                // Short way for a special case
                if (anteingPlayer.availableTokens == 0)
                    break;

                previous = current;

            }


        }

        internal void getUserChoice()
        {
            bw.ReportProgress(1);
            waitHandle.WaitOne();
        }


        internal AttackingPair MCTS_attackingPair(Player player)
        {

            AttackingPair result = null;

            GameState copy = new GameState(this);

            copy.rootNode = new MCTS_Node();

            

            for (int i = 0; i < MAX_PLAYOUTS; i++)
            {
                copy.fillFromGameState(this);

                // Remove attacking pair if it has been selected
                if (p1.attackBase != null)
                    p1.returnAttackingPair();
                

            }

            return result;
            
        }

        
    }


}
