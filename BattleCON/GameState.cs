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


    public abstract class NodeStart
    {
        public NodeEnd parent;

        public abstract double bestWinrate(Player p);
        
    }


    public abstract class NodeEnd
    {
        
        public NodeStart next;
        

        public abstract NodeEnd updateStats(Player winner);

    }
    

    public class SimpleStart : NodeStart
    {

        public Player player;
        public int games = 1;
        
        public SimpleEnd[] children;

        public override double bestWinrate(Player p)
        {
            if (children != null)
            {
                double best = 0;
                foreach (SimpleEnd child in children)
                    if (child != null)
                        if (child.winrate > best)
                            best = child.winrate;
                return best;
            }
            else
                return 0;
        }

    }


    public class SimpleEnd : NodeEnd
    {
        public double winrate;
        public SimpleStart owner;
        
        public int games = 0;
        public int wins = 0;
        
        public override NodeEnd updateStats(Player winner)
        {
            games++;
            if (owner.player == winner)
                wins++;
            winrate = (double)wins / games;
            return owner.parent;
        }

    }


    public class ParallelStart : NodeStart
    {
        
        public SimpleStart tree1;
        public SimpleStart tree2;

        public SortedDictionary<BitSequence, ParallelEnd> combinations;
        
        public ParallelStart()
        {
            tree1 = new SimpleStart();
            tree2 = new SimpleStart();
        }

        public override double bestWinrate(Player p)
        {
            if (tree1.player == p)
                return tree1.bestWinrate(p);
            else
                return tree2.bestWinrate(p);
        }

    }


    public class ParallelEnd
    {
        public SimpleEnd top1;
        public SimpleEnd top2;

        public ParallelStart owner;

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
        private static double ANTE_DELTA = 0.02;

        public NodeStart rootNode;

        public NodeStart currentStart;
        public NodeEnd currentEnd;
        
        

        public PlayoutStartType pst = PlayoutStartType.Normal;
        public Player playoutStartPlayer = null;
        public bool playoutPreviousAnte;

        public List<int> registeredChoices;
        public int currentRegisteredChoice;

        public GameState checkPoint;

        

        public bool pureRandom;
        public bool pureRandom1;
        public bool pureRandom2;

        public SimpleStart currentStart1;
        public SimpleStart currentStart2;

        public static double EXPLORATION_WEIGHT;
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
            currentStart = rootNode;
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

                if (p1.isDead)
                    return p2;

                p1.makeSetupDecisions();

                if (p1.isDead)
                    return p2;

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
                {
                    writeToConsole("BEAT " + beat);
                }
                
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
                    {
                        flushConsole();

                        if (p1.isDead)
                            return;
                    }

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
                    {
                        if (p1.isDead)
                            return;

                        writeToConsole("CLASH! (both players " + p1.priority() + ')');
                    }

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
                        if (p1.isDead)
                            return;

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
                    if (p1.isDead)
                        return;

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

                if (p1.isDead)
                    return;

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


        internal void MCTS_playouts(Player player, PlayoutStartType rpst, GameState fillOrigin, NodeStart startNode)
        {

            GameState copy = new GameState(this);

            copy.addStartNode(startNode);

            bw.ReportProgress(3);

            for (int i = 1; i <= MAX_PLAYOUTS; i++)
            {
                if (i == 1 || i % PLAYOUT_SCREEN_UPDATE_RATE == 0)
                {

                    EXPLORATION_WEIGHT = 0.8;

                    playoutsDone = i;
                    bestWinrate = copy.rootNode.bestWinrate(player);

                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(fillOrigin, rpst, player);
                copy.currentRegisteredChoice = 0;
                
                Player winner = copy.playout();

                copy.updateStats(winner);

            }

            
        }

        private void addStartNode(NodeStart startNode)
        {
            this.currentStart = startNode;
        }



        internal AttackingPair MCTS_attackingPair(Player player)
        {
            ParallelStart rNode = new ParallelStart();
            
            MCTS_playouts(player, PlayoutStartType.AttackPairSelection, this, rNode);

            SimpleStart copyRootNode = rNode.tree1;

            int styleNumber = -1;
            int baseNumber = -1;

            // DEBUG

            SimpleEnd styleEnd;
            SimpleStart baseStart;
            SimpleEnd baseEnd;

            List<DebugNodeComparable> pairs = new List<DebugNodeComparable>();

            
            for (int i = 0; i < copyRootNode.children.Length; i++)
            {

                styleEnd = copyRootNode.children[i];

                baseStart = (SimpleStart)styleEnd.next;

                if (baseStart.children == null)
                    continue;

                for (int j = 0; j < baseStart.children.Length; j++)
                {
                    baseEnd = baseStart.children[j];

                    if (baseEnd == null)
                        continue;
                    
                    pairs.Add(new DebugNodeComparable(player.styles[i].name + " " + player.bases[j], baseEnd.games, baseEnd.winrate));

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

            SimpleEnd sn;

            for (int i = 0; i < copyRootNode.children.Length; i++)
            {
                sn = copyRootNode.children[i];
                if (sn.winrate > best)
                {
                    best = sn.winrate;
                    styleNumber = i;
                }
            }

            styleEnd = copyRootNode.children[styleNumber];
            baseStart = (SimpleStart)styleEnd.next;

            best = -1;

            for (int i = 0; i < baseStart.children.Length; i++)
            {
                sn = baseStart.children[i]; 
                if (sn.winrate > best)
                {
                    best = sn.winrate;
                    baseNumber = i;
                }
            }

            return new AttackingPair(styleNumber, baseNumber);
            
        }


        internal int MCTS_ante(Player player)
        {
            ParallelStart rNode = new ParallelStart();

            MCTS_playouts(player, PlayoutStartType.AnteSelection, this, rNode);

            SimpleStart copyRootNode = rNode.tree1;

            int bestAnte = -1;

            
            
            double best = -1;

            SimpleEnd sn;

            for (int i = 0; i < copyRootNode.children.Length; i++)
            {
                sn = copyRootNode.children[i];
                if (DEBUG_MESSAGES)
                    player.g.writeToConsole("DEBUG Ante " + i + ": " + sn.games + " " + sn.winrate);

                if (sn.winrate > best)
                {
                    best = sn.winrate;
                    bestAnte = i;
                }
            }

            // Now we can select the cheapest ante within delta of the best winrate

            for (int i = 0; i < copyRootNode.children.Length; i++)
            {
                sn = copyRootNode.children[i];
                if (sn.winrate >= best - ANTE_DELTA && i < bestAnte)
                {
                    bestAnte = i;
                }
            }


            return bestAnte;

        }


        internal int MCTS_clash(Player player)
        {
            
            ParallelStart rNode = new ParallelStart();
            
            MCTS_playouts(player, PlayoutStartType.ClashResolution, this, rNode);

            SimpleStart copyRootNode = rNode.tree1;

            int styleNumber = -1;

            double best = -1;

            SimpleEnd sn;

            for (int i = 0; i < copyRootNode.children.Length; i++)
            {
                sn = copyRootNode.children[i]; 
                if (sn.winrate > best)
                {
                    best = sn.winrate;
                    styleNumber = i;
                }
            }

            return styleNumber;

        }


        private int MCTS_beatResolution(int number, Player p)
        {

            SimpleStart rNode = new SimpleStart();

            MCTS_playouts(p, PlayoutStartType.BeatResolution, checkPoint, rNode);

            int choice = -1;

            double best = -1;

            SimpleEnd sn;

            for (int i = 0; i < rNode.children.Length; i++)
            {
                sn = rNode.children[i];
                if (sn.winrate > best)
                {
                    best = sn.winrate;
                    choice = i;
                }
            }

            return choice;

        }



        private void updateStats(Player winner)
        {
            NodeEnd n;

            if (currentParallelSequences != null)
                n = currentParallelSequences.updateStats(winner);
            else
                n = currentNode;

            while (n != null)
            {
                n = n.updateStats(winner);


                
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

            if (currentNode.next == null)
            {
                currentNode.next = new SimpleStart(false);

                pureRandom = true;
                return rnd.Next(number);
            }

            SimpleStart cn = (SimpleStart)currentNode.next;
            
            double bestUCT = 0;
            int result = -1;
            double UCTvalue = 0;
            SimpleEnd child;

            if (cn.children == null)
                cn.children = new SimpleEnd[number];

            for (int i = 0; i < number; i++)
            {
                if (cn.children[i] == null)
                    // Always play a random unexplored move first
                    UCTvalue = 10000 + rnd.Next(1000);
                else
                {
                    child = cn.children[i];
                    UCTvalue = (double) child.wins / child.games + EXPLORATION_WEIGHT * Math.Sqrt(Math.Log(cn.games) / child.games);
                }

                if (UCTvalue > bestUCT)
                {
                    result = i;
                    bestUCT = UCTvalue;
                }
            }

            if (cn.children[result] == null)
            {
                SimpleEnd newChild = new SimpleEnd();
                newChild.start = cn;
                newChild.player = p;
                cn.children[result] = newChild;
            }

            currentNode = cn.children[result];

            return result;

        }


        internal void MCTS_selectSetupCards(Player player)
        {

            ParallelStart rNode = new ParallelStart(2);


            MCTS_playouts(player, PlayoutStartType.SetupCardsSelection, this, rNode);

            MCTS_BestSequenceExtractor e = new MCTS_BestSequenceExtractor(rNode[0]);

            player.selectedCooldownStyle2 = e.getNextBest();
            player.selectedCooldownStyle1 = e.getNextBest();
            player.selectedCooldownBase2 = e.getNextBest();
            player.selectedCooldownBase1 = e.getNextBest();
            player.selectedFinisher = e.getNextBest();
            
        }
    }


}
