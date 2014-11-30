using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace BattleCON
{

    public class GameSettings
    {
        public Character c1;
        public Character c2;

        public int kPlayouts;
    }


    public class MCTS_BestSequenceExtractor
    {
        private MCTS_Node node;

        public MCTS_BestSequenceExtractor(MCTS_Node initialNode)
        {
            node = initialNode;
        }

        public int getNextBest()
        {

            int result = -1;

            double best = -1;

            for (int i = 0; i < node.children.Length; i++)
            {
                if (node.children[i].winrate > best)
                {
                    best = node.children[i].winrate;
                    result = i;
                }
            }

            node = node.children[result];
            return result;

        }
    }


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


    public class DebugNodeComparable : IComparable<DebugNodeComparable>
    {
        public double winrate;
        public int games;
        public string name;

        public DebugNodeComparable(string name, int games, double winrate)
        {
            this.name = name;
            this.games = games;
            this.winrate = winrate;
        }

        public int CompareTo(DebugNodeComparable other)
        {
            return -winrate.CompareTo(other.winrate);
        }

        public override string ToString()
        {
            return name + " " + games + " " + winrate;
        }
    }


    public enum SpecialSelectionStyle
    {
        None,
        Styles,
        Bases,
        Finishers
    }


    public enum PlayoutStartType
    {
        Normal,
        SetupCardsSelection,
        AttackPairSelection,
        AnteSelection,
        ClashResolution,
        BeatResolution
    }


    public enum GameVariant
    {
        Core,
        AnteFinishers
    }

    
    public class GameState
    {
        public bool isMainGame;
        public bool isFinished = false;

        public List<string> consoleBuffer;
        public BackgroundWorker bw;
        public EventWaitHandle waitHandle;

        public GameVariant variant;

        public Player p1;
        public Player p2;

        Player firstToAnte;

        public int beat;

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
        public static int PLAYOUT_SCREEN_UPDATE_RATE = 10000;

        public MCTS_Node rootNode;
        public MCTS_Node currentNode;

        public PlayoutStartType pst = PlayoutStartType.Normal;
        public Player playoutStartPlayer = null;
        public bool playoutPreviousAnte;

        public List<int> registeredChoices;
        public int currentRegisteredChoice;

        public GameState checkPoint;

        public bool pureRandom;
        private static double EXPLORATION_WEIGHT;
        public static bool DEBUG_MESSAGES;
        
        

        public GameState(Character c1, Character c2, GameVariant variant,  BackgroundWorker bw, EventWaitHandle waitHandle)
        {
            beat = 1;

            this.variant = variant;

            p1 = new Player(c1, 2, this, true, true);
            p2 = new Player(c2, 6, this, false, false);

            firstToAnte = p1;

            p1.opponent = p2;
            p2.opponent = p1;

            isMainGame = true;

            registeredChoices = new List<int>();

            checkPoint = new GameState(this);

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

            registeredChoices = g.registeredChoices;

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

            variant = gameState.variant;

            p1.opponent = p2;
            p2.opponent = p1;

            rnd = gameState.rnd;

        }

        internal Player playout()
        {
            if (isMainGame)
            {
                // Select Setup Cards
                p2.makeSetupDecisions();
                p1.makeSetupDecisions();

                p1.applySetupDecisions();
                p2.applySetupDecisions();
            }
            else if (pst == PlayoutStartType.SetupCardsSelection)
            {
                playoutStartPlayer.makeSetupDecisions();
                playoutStartPlayer.opponent.makeSetupDecisions();

                p1.applySetupDecisions();
                p2.applySetupDecisions();

                pst = PlayoutStartType.Normal;
            }


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
                p2.selectAttackingPair();
                p1.selectAttackingPair();
                
            }
            else if (pst == PlayoutStartType.AttackPairSelection)
            {
                playoutStartPlayer.selectAttackingPair();
                playoutStartPlayer.opponent.selectAttackingPair();

                pst = PlayoutStartType.Normal;

            }
           
            bool normalPlay = true;

            if (pst != PlayoutStartType.BeatResolution)
            {

                if (pst != PlayoutStartType.ClashResolution)
                {

                    antePhase();

                    if (pst == PlayoutStartType.AnteSelection)
                        pst = PlayoutStartType.Normal;

                    p1.AnteEffects();
                    p2.AnteEffects();

                    p1.revealAttack();
                    p2.revealAttack();

                    // Reveal cards

                    p1.RevealEffects();
                    p2.RevealEffects();

                    if (isMainGame)
                        flushConsole();

                }

                // Determine a clash



                if (isMainGame)
                    writeToConsole("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());


                if (pst == PlayoutStartType.ClashResolution)
                {
                    playoutStartPlayer.selectNextForClash_MCTS();

                    revealClash();

                    pst = PlayoutStartType.Normal;

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

                    p2.selectNextForClash();
                    p1.selectNextForClash();
                    

                    revealClash();

                    if (isMainGame)
                    {
                        writeToConsole(p1 + " selected " + p1.attackStyle + ' ' + p1.attackBase);
                        writeToConsole(p2 + " selected " + p2.attackStyle + ' ' + p2.attackBase);
                        writeToConsole("Priorities: " + p1 + ' ' + p1.priority() + ", " + p2 + ' ' + p2.priority());
                    }

                }

            }

            if (normalPlay)
            {

                if (isMainGame)
                {
                    checkPoint.fillFromGameState(this, PlayoutStartType.BeatResolution, p1);
                    registeredChoices.Clear();
                }


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
                    if (reactivePlayer.isDead)
                        return;
                }
                else
                {
                    if (isMainGame)
                        writeToConsole(activePlayer + " cannot attack since (s)he is stunned!");
                }

                if (!reactivePlayer.isDead && !reactivePlayer.isStunned)
                {
                    reactivePlayer.attack(false);
                    if (activePlayer.isDead)
                        return;
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
            bool current;

            Player localFirstPlayer = (pst == PlayoutStartType.AnteSelection ? playoutStartPlayer : firstToAnte);
            Player anteingPlayer = localFirstPlayer;

            while (true)
            {
                playoutPreviousAnte = previous;
                AnteResult result = anteingPlayer.ante();

                current = (result == AnteResult.AntedTokens);

                if (!current && !previous || result == AnteResult.AntedFinisher)
                    // both passed or someone anted a finisher
                    break;

                anteingPlayer = anteingPlayer.opponent;

                // Short way for a special case
                if (anteingPlayer.availableTokens == 0 && !anteingPlayer.canAnteFinisher())
                    break;

                previous = current;

            }


        }

        internal void getUserChoice()
        {
            bw.ReportProgress(1);
            waitHandle.WaitOne();
        }


        internal MCTS_Node MCTS_playouts(Player player, PlayoutStartType rpst, GameState fillOrigin)
        {

            GameState copy = new GameState(this);

            copy.rootNode = new MCTS_Node();
            copy.rootNode.games = 1; // to not waste the first game

            bw.ReportProgress(3);

            // EXPLORATION_WEIGHT = 1.5;

            for (int i = 1; i <= MAX_PLAYOUTS; i++)
            {
                if (i == 1 || i % PLAYOUT_SCREEN_UPDATE_RATE == 0)
                {
                    //EXPLORATION_WEIGHT -= 0.1;

                    //EXPLORATION_WEIGHT = (i < MAX_PLAYOUTS / 10 ? 2 : 0.6);

                    EXPLORATION_WEIGHT = 0.8;

                    playoutsDone = i;
                    bestWinrate = copy.rootNode.bestWinrate();
                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(fillOrigin, rpst, player);
                copy.currentRegisteredChoice = 0;
                
                Player winner = copy.playout();

                copy.updateStats(winner);

            }

            return copy.rootNode;
        }



        internal AttackingPair MCTS_attackingPair(Player player)
        {

            MCTS_Node copyRootNoode = MCTS_playouts(player, PlayoutStartType.AttackPairSelection, this);

            int styleNumber = -1;
            int baseNumber = -1;

            // DEBUG

            MCTS_Node styleNode;
            MCTS_Node baseNode;

            List<DebugNodeComparable> pairs = new List<DebugNodeComparable>();

            for (int i = 0; i < copyRootNoode.children.Length; i++)
            {
                styleNode = copyRootNoode.children[i];
                if (styleNode.children == null)
                    continue;

                for (int j = 0; j < styleNode.children.Length; j++)
                {
                    baseNode = styleNode.children[j];

                    if (baseNode == null)
                        continue;
                    
                    pairs.Add(new DebugNodeComparable(player.styles[i].name + " " + player.bases[j], baseNode.games, baseNode.winrate));

                }

            }

            pairs.Sort();

            if (DEBUG_MESSAGES)
            {
                foreach (DebugNodeComparable d in pairs)
                {
                    writeToConsole(d.ToString());
                }
            }

            // DEBUG



            double best = -1;

            for (int i = 0; i < copyRootNoode.children.Length; i++)
                if (copyRootNoode.children[i].winrate > best)
                {
                    best = copyRootNoode.children[i].winrate;
                    styleNumber = i;
                }

            styleNode = copyRootNoode.children[styleNumber];

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

            MCTS_Node copyRootNoode = MCTS_playouts(player, PlayoutStartType.AnteSelection, this);

            int styleNumber = -1;
            
            double best = -1;

            for (int i = 0; i < copyRootNoode.children.Length; i++)
            {

                if (DEBUG_MESSAGES)
                    player.g.writeToConsole("DEBUG Ante " + i + ": " + copyRootNoode.children[i].games + " " + copyRootNoode.children[i].winrate);

                if (copyRootNoode.children[i].winrate > best)
                {
                    best = copyRootNoode.children[i].winrate;
                    styleNumber = i;
                }
            }

            return styleNumber;

        }


        internal int MCTS_clash(Player player)
        {

            MCTS_Node copyRootNoode = MCTS_playouts(player, PlayoutStartType.ClashResolution, this);

            int styleNumber = -1;

            double best = -1;

            for (int i = 0; i < copyRootNoode.children.Length; i++)
                if (copyRootNoode.children[i].winrate > best)
                {
                    best = copyRootNoode.children[i].winrate;
                    styleNumber = i;
                }

            return styleNumber;

        }


        private int MCTS_beatResolution(int number, Player p)
        {

            MCTS_Node copyRootNoode = MCTS_playouts(p, PlayoutStartType.BeatResolution, checkPoint);

            int choice = -1;

            double best = -1;

            for (int i = 0; i < copyRootNoode.children.Length; i++)
                if (copyRootNoode.children[i].winrate > best)
                {
                    best = copyRootNoode.children[i].winrate;
                    choice = i;
                }

            return choice;

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



        internal int UCTSelect(int number, Player p, bool beatResolution)
        {
            if (beatResolution)
            {
                if (isMainGame)
                    return MCTS_beatResolution(number, p);

                if (pst == PlayoutStartType.BeatResolution)
                {
                    if (currentRegisteredChoice < registeredChoices.Count)
                    {
                        currentRegisteredChoice++;
                        return registeredChoices[currentRegisteredChoice - 1];
                    }

                    pst = PlayoutStartType.Normal;

                }

            }


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


        internal void MCTS_selectSetupCards(Player player)
        {
            MCTS_Node copyRootNoode = MCTS_playouts(player, PlayoutStartType.SetupCardsSelection, this);

            MCTS_BestSequenceExtractor e = new MCTS_BestSequenceExtractor(copyRootNoode);

            player.selectedCooldownStyle2 = e.getNextBest();
            player.selectedCooldownStyle1 = e.getNextBest();
            player.selectedCooldownBase2 = e.getNextBest();
            player.selectedCooldownBase1 = e.getNextBest();
            player.selectedFinisher = e.getNextBest();
            
        }
    }


}
