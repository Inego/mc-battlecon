using System;
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
        
        
        // Monte Carlo Tree Search

        public static int MAX_PLAYOUTS = 10000;

        public MCTS_Node rootNode;
        public MCTS_Node currentNode;

        PlayoutStartType pst = PlayoutStartType.Normal;
        Player playoutStartPlayer = null;
        public bool pureRandom;
        
        

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
                if (p1.attackStyle != null)
                    p1.returnAttackingPair();

                playoutStartPlayer.selectAttackingPair();
                playoutStartPlayer.opponent.selectAttackingPair();

                pst = PlayoutStartType.Normal;

            }
            

            
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
            copy.rootNode.games = 1; // to not waste the first game
            

            for (int i = 0; i < MAX_PLAYOUTS; i++)
            {
                copy.fillFromGameState(this, PlayoutStartType.AttackPairSelection, player);
                
                Player winner = copy.playout();

                copy.updateStats(winner);
                

            }

            return result;
            
        }

        private void updateStats(Player winner)
        {
            MCTS_Node n = currentNode;

            while (n != null)
            {
                n.games++;
                if (n.player == winner)
                    n.wins++;

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
                        UCTvalue = child.wins / child.games + Math.Sqrt(Math.Log(currentNode.games) / (5 * child.games));
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
