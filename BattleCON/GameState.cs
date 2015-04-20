using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using System.Diagnostics;

namespace BattleCON
{

    public static class R
    {
        static Random rnd = new Random(1);

        public static int n(int number)
        {
            return rnd.Next(number);
        }
    }

    public static class Constants
    {
        public static double EXPLORATION_WEIGHT = 0.8;
    }

    public class GameSettings
    {
        public Character c1;
        public Character c2;

        public int kPlayouts;
    }


    public class MCTS_BestSequenceExtractor
    {
        private SimpleStart node;

        public MCTS_BestSequenceExtractor(SimpleStart initialNode)
        {
            node = initialNode;
        }

        public MCTS_BestSequenceExtractor(ParallelStart rNode, Player player)
        {
            node = (player.first ? rNode.tree1 : rNode.tree2);
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

            node = node.children[result].next as SimpleStart;
            return result;

        }
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

        public SimpleStart()
        {
            
        }

        public SimpleStart(Player p)
        {
            this.player = p;
        }

        public SimpleStart(NodeEnd parent, Player p)
            : this(p)
        {
            this.parent = parent;
            parent.next = this;
        }


        internal SimpleEnd GetChild(int result)
        {
            if (children[result] == null)
            {
                SimpleEnd newChild = new SimpleEnd();
                newChild.owner = this;
                children[result] = newChild;
            }

            return children[result];
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

        internal SimpleStart GetOrigin()
        {
            SimpleEnd z = this;

            while (z.owner.parent != null)
                z = (SimpleEnd)z.owner.parent;

            return z.owner;
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

        public ParallelStart(Player p1, Player p2)
        {
            tree1 = new SimpleStart(p1);
            tree2 = new SimpleStart(p2);
        }

        public ParallelStart(Player p1, Player p2, NodeEnd parent)
            : this(p1, p2)
        {
            this.parent = parent;
            parent.next = this;
        }

        

        public override double bestWinrate(Player p)
        {
            return (p.first ? tree1 : tree2).bestWinrate(p);
        }

    }


    public class ParallelEnd : NodeEnd
    {
        public SimpleEnd top1;
        public SimpleEnd top2;

        public ParallelStart owner;

        public override NodeEnd updateStats(Player winner)
        {
            top1.updateStats(winner);
            top2.updateStats(winner);

            return owner.parent;
        }

    }


    public class MoveSequence
    {
        public bool pureRandom;
        public SimpleStart start;
        public SimpleEnd current;
        public MoveSequence opponent;
        public MoveManager pmm;


        public MoveSequence(MoveManager pmm)
        {
            this.pmm = pmm;
        }
        

        public void Reset(SimpleStart start)
        {
            pureRandom = false;
            this.start = start;
            this.current = null;
        }


        public int UCT_select(int number, Player p)
        {
            if (pureRandom)
                return R.n(number);

            int result = -1;

            SimpleStart cn;

            if (current == null)
            {
                cn = start;
            }
            else if (current.next == null)
            {
                new SimpleStart(current, p);

                pureRandom = true;

                result = R.n(number);

                if (opponent.pureRandom)
                    pmm.pureRandom = true;

                return result;
            }
            else 
                cn = (SimpleStart)current.next;

            double bestUCT = -1;
            double UCTvalue = 0;
            SimpleEnd child;

            if (cn.children == null)
                cn.children = new SimpleEnd[number];

            for (int i = 0; i < number; i++)
            {
                if (cn.children[i] == null)
                    // Always play a random unexplored move first
                    UCTvalue = 10000 + R.n(1000);
                else
                {
                    child = cn.children[i];
                    UCTvalue = (double)child.wins / child.games + Constants.EXPLORATION_WEIGHT * Math.Sqrt(Math.Log(cn.games) / child.games);
                }

                if (UCTvalue > bestUCT)
                {
                    result = i;
                    bestUCT = UCTvalue;
                }
            }

            current = cn.GetChild(result);

            return result;

        }

        internal void RegisterOpponentMove(int number, int result, Player player)
        {
            SimpleStart cn;

            if (current == null)
            {
                cn = start;
            }
            else
            {
                if (current.next == null)
                {
                    new SimpleStart(current, player);
                    pureRandom = true;
                    if (opponent.pureRandom)
                    {
                        pmm.pureRandom = true;
                    }
                    return;
                }
                cn = (SimpleStart)current.next;
            }
                
            if (cn.children == null)
                cn.children = new SimpleEnd[number];

            current = cn.GetChild(result);

        }

        internal void updateStats(Player winner)
        {
            SimpleEnd c = current;
            while (c != null)
                c = (SimpleEnd)c.updateStats(winner);
        }
    }


    public class MoveManager
    {
        public bool pureRandom = false;

        public bool parallel;

        public NodeEnd commonEnd;

        // SINGLE MODE

        public SimpleStart sStart;
        public SimpleEnd   sEnd;

        // PARALLEL MODE
        
        private BitSequence bitSequence;
        
        public MoveSequence s1;
        public MoveSequence s2;
        
        public Player p1;
        public Player p2;

        public ParallelStart pStart;

        private NodeStart rootNode;
        private bool started;
        

        public MoveManager(NodeStart rootNode, Player p1, Player p2)
        {
            

            s1 = new MoveSequence(this);
            s2 = new MoveSequence(this);
            
            s1.opponent = s2;
            s2.opponent = s1;

            this.p1 = p1;
            this.p2 = p2;

            this.rootNode = rootNode;

        }


        public void ParallelInitialize()
        {
            if (pureRandom)
                return;

            FinalizePrevious();

            parallel = true;

            bitSequence = new BitSequence();

            if (commonEnd == null)
            {
                pStart = (ParallelStart)rootNode;
            }
            else
            {
                pStart = (ParallelStart)commonEnd.next;
                if (pStart == null)
                    pStart = new ParallelStart(p1, p2, commonEnd);
            }

            s1.Reset(pStart.tree1);
            s2.Reset(pStart.tree2);
            
        }


        public int ParallelSelect(int number, Player p)
        {
            if (pureRandom)
                return R.n(number);

            MoveSequence active = (p == p1 ? s1 : s2);

            int result = active.UCT_select(number, p);

            if (!pureRandom)
                bitSequence.AddBits((uint)result, number);

            return result;
            
        }


        public void ParallelFinalize()
        {
            ParallelEnd found;

            if (pStart.combinations == null)
                pStart.combinations = new SortedDictionary<BitSequence, ParallelEnd>();

            pStart.combinations.TryGetValue(bitSequence, out found);

            if (found == null)
            {
                found = new ParallelEnd();
                found.owner = pStart;

                pStart.combinations[bitSequence] = found;
            }
            else
            {
                //int z = 1;
            }

            //if (found.top1 != null)
            //    Debug.Assert(found.top1.GetOrigin() == s1.current.GetOrigin());

            //if (found.top2 != null)
            //    Debug.Assert(found.top2.GetOrigin() == s2.current.GetOrigin());

            // Must update tops even if they are here
            found.top1 = s1.current;
            found.top2 = s2.current;

            commonEnd = found;
            
        }


        public void SingleInitialize()
        {
            if (pureRandom)
                return;

            FinalizePrevious();

            if (commonEnd == null)
            {
                sStart = (SimpleStart)rootNode;
            }
            else
            {
                sStart = commonEnd.next as SimpleStart; // May be null if commonEnd points to another ParallelStart
            }

            sEnd = null;

            parallel = false;

        }


        internal int SingleSelect(int number, Player p)
        {

            if (pureRandom)
                return R.n(number);

            SimpleStart cn;

            if (sEnd == null)
            {
                if (sStart == null)
                {
                    sStart = new SimpleStart(commonEnd, p);
                }

                cn = sStart;
            }
            else
            { 
                if (sEnd.next == null)
                {
                    sEnd.next = new SimpleStart(p);
                    pureRandom = true;
                    return R.n(number);
                }
                cn = (SimpleStart)sEnd.next;
            }

            double bestUCT = -1;
            int result = -1;
            double UCTvalue = 0;
            SimpleEnd child;

            if (cn.children == null)
                cn.children = new SimpleEnd[number];

            for (int i = 0; i < number; i++)
            {
                if (cn.children[i] == null)
                    // Always play a random unexplored move first
                    UCTvalue = 10000 + R.n(1000);
                else
                {
                    child = cn.children[i];
                    UCTvalue = (double)child.wins / child.games + Constants.EXPLORATION_WEIGHT * Math.Sqrt(Math.Log(cn.games) / child.games);
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
                newChild.owner = cn;
                cn.children[result] = newChild;
            }

            sEnd = cn.children[result];

            return result;
        }


        private void SingleFinalize()
        {
            if (sEnd == null)
                // Situations like this can happen when single initialization
                // turned into 0 single moves.
                // In this case sEnd is still null and we don't need to overwrite commonEnd with it.
                return;


            commonEnd = sEnd;            
        }


        private void FinalizePrevious()
        {
            if (started)
            {
                started = false;
                return;
            }

            if (parallel)
                ParallelFinalize();
            else
                SingleFinalize();
        }


        public NodeStart updateStats(Player winner)
        {
            NodeEnd n;
            NodeStart lastStart;

            if (parallel)
            {
                s1.updateStats(winner);
                s2.updateStats(winner);
                n = pStart.parent;
                lastStart = pStart;
            }
            else
            {
                n = sEnd; // SimpleEnd
                lastStart = sEnd.owner;
            }

            while (n != null)
            {
                if (n is ParallelEnd)
                    lastStart = ((ParallelEnd)n).owner;
                else
                    lastStart = ((SimpleEnd)n).owner;
                n = n.updateStats(winner);
            }

            return lastStart;

        }

        public NodeStart TraceOrigin()
        {
            NodeEnd n;
            NodeStart lastStart;

            if (parallel)
            {
                n = pStart.parent;
                lastStart = pStart;
            }
            else
            {
                n = sEnd; // SimpleEnd
                lastStart = sEnd.owner;
            }

            while (n != null)
            {
                if (n is ParallelEnd) {
                    lastStart = ((ParallelEnd)n).owner;
                }
                else
                {
                    lastStart = ((SimpleEnd)n).owner;
                }
                n = lastStart.parent;
                
            }

            return lastStart;

        }



                

        internal void ResetToRoot()
        {
            this.started = true;
            commonEnd = null;
            pureRandom = false;
            
        }


        internal int ParallelSelectWithCloning(int number, Player player)
        {
            if (pureRandom)
                return R.n(number);

            int result = ParallelSelect(number, player);

            MoveSequence opponentSequence = (player == p1 ? s2 : s1);

            if (!opponentSequence.pureRandom)
                opponentSequence.RegisterOpponentMove(number, result, player);

            return result;
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

        public PlayoutStartType pst = PlayoutStartType.Normal;
        public Player playoutStartPlayer = null;
        public bool playoutPreviousAnte;

        public List<int> registeredChoices;
        public int currentRegisteredChoice;

        public GameState checkPoint;

        public bool pureRandom;

        public static bool DEBUG_MESSAGES;
        
        public MoveManager moveManager;
       
 

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

        
        // Cloning
        public GameState(GameState gameState)
        {

            p1 = new Player(gameState.p1.c, this);
            p2 = new Player(gameState.p2.c, this);

            variant = gameState.variant;

            p1.opponent = p2;
            p2.opponent = p1;

        }

        public GameState(GameState gameState, NodeStart rootNode)
            : this(gameState)
        {
            moveManager = new MoveManager(rootNode, p1, p2);
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
            
            pureRandom = false;

            moveManager.ResetToRoot();
            
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

                moveManager.ParallelInitialize();
                
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

                // Debugging
                NodeStart z = moveManager.TraceOrigin();
                // Debugging

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
                moveManager.ParallelInitialize();
                p1.selectAttackingPair();
                p2.selectAttackingPair();
                
            }
            else if (pst == PlayoutStartType.AttackPairSelection)
            {
                moveManager.ParallelInitialize();

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

                    moveManager.ParallelInitialize();

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

                moveManager.SingleInitialize();

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

                if (p1.isDead)
                    return;

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


        internal void MCTS_playouts(Player player, PlayoutStartType rpst, GameState fillOrigin, NodeStart startNode)
        {

            GameState copy = new GameState(this, startNode);

            if (startNode is ParallelStart)
            {
                ParallelStart ps = (ParallelStart)startNode;
                ps.tree1.player = copy.p1;
                ps.tree2.player = copy.p2;
            }
            else
            {
                SimpleStart ss = (SimpleStart)startNode;
                ss.player = (player == p1 ? copy.p1 : copy.p2);
            }

            bw.ReportProgress(3);

            for (int i = 1; i <= MAX_PLAYOUTS; i++)
            {
                if (i == 1 || i % PLAYOUT_SCREEN_UPDATE_RATE == 0)
                {

                    playoutsDone = i;
                    bestWinrate = startNode.bestWinrate(player);

                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(fillOrigin, rpst, player);
                copy.currentRegisteredChoice = 0;
                
                Player winner = copy.playout();

                NodeStart updateResult = copy.updateStats(winner);

                if(updateResult != startNode)
                    throw new NotImplementedException("oops");

            }

            
        }


        private NodeStart updateStats(Player winner)
        {
            return moveManager.updateStats(winner);
        }


        private void initializeMoveManager(NodeStart startNode)
        {
            throw new NotImplementedException();
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


        internal int SimpleUCTSelect(int number, Player p)
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

            return moveManager.SingleSelect(number, p);

        }


        internal void MCTS_selectSetupCards(Player player)
        {

            ParallelStart rNode = new ParallelStart();


            MCTS_playouts(player, PlayoutStartType.SetupCardsSelection, this, rNode);

            MCTS_BestSequenceExtractor e = new MCTS_BestSequenceExtractor(rNode, player);

            player.selectedCooldownStyle2 = e.getNextBest();
            player.selectedCooldownStyle1 = e.getNextBest();
            player.selectedCooldownBase2 = e.getNextBest();
            player.selectedCooldownBase1 = e.getNextBest();
            player.selectedFinisher = e.getNextBest();
            
        }

    }


}
