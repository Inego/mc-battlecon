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
        public Player player;
        public int games = 0;
        public int wins = 0;
        public MCTS_Node[] children;
        public MCTS_Node parent = null;
        public double winrate;

        internal double bestWinrate()
        {
            if (children != null)
            {
                double best = 0;
                foreach (MCTS_Node child in children)
                    if (child != null)
                        if (child.winrate > best)
                            best = child.winrate;
                return best;
            }
            else
                return 0;
        }
    }


    public enum SpecialSelectionStyle
    {
        None,
        Styles,
        Bases
    }

    public enum PlayoutStartType
    {
        Normal,
        AttackPairSelection,
        AnteSelection,
        ClashResolution,
        BeatResolution
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

        // MC updates
        public double bestWinrate;
        public int playoutsDone;
        
        
        // Monte Carlo Tree Search

        public static int MAX_PLAYOUTS = 100000;

        public MCTS_Node rootNode;
        public MCTS_Node currentNode;

        public PlayoutStartType pst = PlayoutStartType.Normal;
        public Player playoutStartPlayer = null;
        public bool playoutPreviousAnte;

        public bool pureRandom;
        private static double EXPLORATION_WEIGHT = 0.7;
        
        
        
        
        

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

        public void fillFromGameState(GameState g, PlayoutStartType pst, Player playoutStartPlayer)
        {
            p1.fillFromPlayer(g.p1);
            p2.fillFromPlayer(g.p2);
            firstToAnte = g.firstToAnte.first ? p1 : p2;
            beat = g.beat;

            playoutPreviousAnte = g.playoutPreviousAnte;

            // MCTS
            this.pst = pst;
            this.playoutStartPlayer = playoutStartPlayer.first ? p1 : p2;
            currentNode = rootNode;
            pureRandom = false;
            
        }

        // Cloning
        public GameState(GameState gameState)
        {
            p1 = new Player(gameState.p1.c, this);
            p2 = new Player(gameState.p2.c, this);

            p1.opponent = p2;
            p2.opponent = p1;

            rnd = gameState.rnd;

        }

        internal Player playout()
        {
            while (beat <= 15)
            {
                if (isMainGame)
                    writeToConsole("BEAT " + beat);
                
                this.nextBeat();

                if (p1.isDead)
                    return p2;

                if (p2.isDead)
                    return p1;

            }

            if (p1.health < p2.health)
                return p2;

            if (p1.health > p2.health)
                return p1;

            // DRAW!
            return null;

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

            if (pst == PlayoutStartType.Normal)
            {
                // Select random style
                p1.selectAttackingPair();
                p2.selectAttackingPair();
            }
            else if (pst == PlayoutStartType.AttackPairSelection)
            {
                playoutStartPlayer.selectAttackingPair();
                playoutStartPlayer.opponent.selectAttackingPair();

                pst = PlayoutStartType.Normal;

            }
           

            if (pst != PlayoutStartType.ClashResolution)
            {

                antePhase();

                p1.AnteEffects();
                p2.AnteEffects();

                p1.revealAttack();
                p2.revealAttack();

                // Reveal cards

                p1.RevealEffects();
                p2.RevealEffects();

            }

            // Determine a clash

            bool normalPlay = true;

            if (isMainGame)
                writeToConsole("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());


            if (pst == PlayoutStartType.ClashResolution)
            {
                playoutStartPlayer.selectNextForClash_MCTS();

                revealClash();
            }
                

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

                revealClash();

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

        private void revealClash()
        {
            p1.revealClash();
            p2.revealClash();
        }

        private void antePhase()
        {
            bool previous = (pst == PlayoutStartType.AnteSelection ? playoutPreviousAnte : true);

            Player anteingPlayer = (pst == PlayoutStartType.AnteSelection ? playoutStartPlayer : firstToAnte);


            while (true)
            {
                playoutPreviousAnte = previous;
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

            GameState copy = new GameState(this);

            copy.rootNode = new MCTS_Node();
            copy.rootNode.games = 1; // to not waste the first game
            

            for (int i = 0; i < MAX_PLAYOUTS; i++)
            {
                if (i % 1000 == 0)
                {
                    playoutsDone = i;
                    bestWinrate = copy.rootNode.bestWinrate();
                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(this, PlayoutStartType.AttackPairSelection, player);
                
                Player winner = copy.playout();

                copy.updateStats(winner);
                

            }

            int styleNumber = -1;
            int baseNumber = -1;

            double best = -1;

            for (int i = 0; i < copy.rootNode.children.Length; i++)
                if (copy.rootNode.children[i].winrate > best)
                {
                    best = copy.rootNode.children[i].winrate;
                    styleNumber = i;
                }

            MCTS_Node styleNode = copy.rootNode.children[styleNumber];

            best = -1;

            for (int i = 0; i < styleNode.children.Length; i++)
                if (styleNode.children[i].winrate > best)
                {
                    best = styleNode.children[i].winrate;
                    baseNumber = i;
                }

            return new AttackingPair(styleNumber, baseNumber);

            
        }


        internal int MCTS_ante(Player player)
        {

            GameState copy = new GameState(this);

            copy.rootNode = new MCTS_Node();
            copy.rootNode.games = 1; // to not waste the first game


            for (int i = 0; i < MAX_PLAYOUTS; i++)
            {
                if (i % 1000 == 0)
                {
                    playoutsDone = i;
                    bestWinrate = copy.rootNode.bestWinrate();
                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(this, PlayoutStartType.AnteSelection, player);

                Player winner = copy.playout();

                copy.updateStats(winner);

            }

            int styleNumber = -1;
            
            double best = -1;

            for (int i = 0; i < copy.rootNode.children.Length; i++)
                if (copy.rootNode.children[i].winrate > best)
                {
                    best = copy.rootNode.children[i].winrate;
                    styleNumber = i;
                }

            

            return styleNumber;


        }


        internal int MCTS_clash(Player player)
        {

            GameState copy = new GameState(this);

            copy.rootNode = new MCTS_Node();
            copy.rootNode.games = 1; // to not waste the first game

            for (int i = 0; i < MAX_PLAYOUTS; i++)
            {
                if (i % 1000 == 0)
                {
                    playoutsDone = i;
                    bestWinrate = copy.rootNode.bestWinrate();
                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(this, PlayoutStartType.ClashResolution, player);

                Player winner = copy.playout();

                copy.updateStats(winner);

            }

            int styleNumber = -1;

            double best = -1;

            for (int i = 0; i < copy.rootNode.children.Length; i++)
                if (copy.rootNode.children[i].winrate > best)
                {
                    best = copy.rootNode.children[i].winrate;
                    styleNumber = i;
                }

            return styleNumber;


        }



        private void updateStats(Player winner)
        {
            MCTS_Node n = currentNode;

            while (n != null)
            {
                n.games++;
                if (n.player == winner)
                    n.wins++;

                n.winrate = (double)n.wins / n.games; 

                n = n.parent;
            }
        }



        internal int UCTSelect(int number, Player p)
        {
            if (pureRandom)
                return rnd.Next(number);

            if (currentNode.games == 0)
            {
                pureRandom = true;
                return rnd.Next(number);
            }

            else
            {
                double bestUCT = 0;
                int result = -1;
                double UCTvalue = 0;
                MCTS_Node child;

                if (currentNode.children == null)
                    currentNode.children = new MCTS_Node[number];

                for (int i = 0; i < number; i++)
                {
                    if (currentNode.children[i] == null)
                        // Always play a random unexplored move first
                        UCTvalue = 10000 + rnd.Next(1000);
                    else
                    {
                        child = currentNode.children[i];
                        UCTvalue = (double) child.wins / child.games + EXPLORATION_WEIGHT * Math.Sqrt(Math.Log(currentNode.games) / child.games);
                    }

                    if (UCTvalue > bestUCT)
                    {
                        result = i;
                        bestUCT = UCTvalue;
                    }
                }

                if (currentNode.children[result] == null)
                {
                    currentNode.children[result] = new MCTS_Node();
                    currentNode.children[result].parent = currentNode;
                    currentNode.children[result].player = p;
                }

                currentNode = currentNode.children[result];

                return result;

            }

        }
    }


}
