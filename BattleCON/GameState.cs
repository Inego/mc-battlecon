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
        //static Random rnd = new Random(1);
        static Random rnd = new Random();

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
        public CharacterClass c1;
        public CharacterClass c2;

        public int kPlayouts;
    }


    public class BestSequenceExtractor
    {
        private SimpleStart node;
        private bool pureRandom = false;
        private GameState g;
        private bool suboptimal = false;

        public BestSequenceExtractor(GameState g, SimpleStart initialNode)
        {
            node = initialNode;
            this.g = g;
        }

        public BestSequenceExtractor(GameState g, ParallelStart rNode, Player player)
        {
            node = (player.first ? rNode.tree1 : rNode.tree2);
            this.g = g;
            suboptimal = true;
        }

        public int getNextBest(int number)
        {
            int result = -1;

            if (pureRandom)
            {
                result = R.n(number);
                if (GameState.DEBUG_MESSAGES)
                    g.writeDebug("Pure random, selected " + result + " from " + number);
                return result;
            }

            if (node == null || node.children == null)
            {
                pureRandom = true;
                result = R.n(number);
                if (GameState.DEBUG_MESSAGES)
                    g.writeDebug("SWITCHED to Pure random, selected " + result + " from " + number);
                return result;
            }

            double best = -1;
            int games = 0;
            bool hasVoid = false;

            for (int i = 0; i < node.children.Length; i++)
            {
                if (GameState.DEBUG_MESSAGES)
                {
                    if (node.children[i] == null)
                        g.writeDebug(i.ToString() + "  NULL");
                    else
                        g.writeDebug(i.ToString() + "  " + node.children[i].winrate + " " + node.children[i].games);
                }

                if (node.children[i] == null || node.children[i].games == 0)
                {
                    hasVoid = true;
                    continue;
                }

                games += node.children[i].games;

                if (node.children[i].winrate > best)
                {
                    best = node.children[i].winrate;
                    result = i;
                }

            }

            if (result == -1 || hasVoid)
            {
                pureRandom = true;
                result = R.n(number);
                if (GameState.DEBUG_MESSAGES)
                    g.writeDebug("Couldn't find a suitable move, SWITCHED to Pure random, selected " + result + " from " + number);
                return result;
            }

            node = node.children[result].next as SimpleStart;

            if (GameState.DEBUG_MESSAGES)
                g.writeDebug("SELECTED " + result + " from " + number);
            
            return result;

        }

        public void SelectFixed(int number)
        {
            if (pureRandom == true)
                return;
            if (node == null || node.children == null) { 
                pureRandom = true;
                return;
            }

            SimpleEnd sEnd = node.children[number];

            if (sEnd == null)
            {
                pureRandom = true;
            }
            else
            {
                node = sEnd.next as SimpleStart;
            }

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
            owner.games++;
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
            NodeEnd z = top1;


            while (z != null)
                z = z.updateStats(winner);

            z = top2;

            while (z != null)
                z = z.updateStats(winner);

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

        internal NodeStart rootNode;
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

            if (found.top1 != null)
                Debug.Assert(found.top1.GetOrigin() == s1.current.GetOrigin());

            if (found.top2 != null)
                Debug.Assert(found.top2.GetOrigin() == s2.current.GetOrigin());

            //if (s1.current == null || s2.current == null)
            //    throw new NotImplementedException("crap");

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

            //if (parallel)
            //    throw new NotImplementedException("Da heck");

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
                    new SimpleStart(sEnd, p);
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

            //if (TraceOrigin() != rootNode)
            //    throw new NotImplementedException("dam");

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

            if (parallel || sEnd == null)
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

            if (parallel || sEnd == null && sStart == null)
            {
                n = pStart.parent;
                lastStart = pStart;
            }
            else
            {
                if (sEnd == null)
                {
                    n = sStart.parent;
                    lastStart = sStart;
                }
                else
                {
                    n = sEnd; // SimpleEnd
                    lastStart = sEnd.owner;
                }
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
        
        public PlayoutStartType pst = PlayoutStartType.Normal;
        public Player playoutStartPlayer = null;
        public bool playoutPreviousAnte;

        public List<int> registeredChoices;
        public int currentRegisteredChoice;

        public GameState checkPoint;

        public bool pureRandom;

        public static bool DEBUG_MESSAGES;
        
        public MoveManager moveManager;
        public bool terminated;
        
        // Beat-specific variables reflecting various events
        public Player activePlayer;
        public Player activePlayerOverride;
       
 

        public GameState(CharacterClass c1, CharacterClass c2, GameVariant variant,  BackgroundWorker bw, EventWaitHandle waitHandle)
        {
            beat = 1;

            this.variant = variant;

            p1 = Player.New(c1, 2, this, true, true);
            p2 = Player.New(c2, 6, this, false, false);

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

            p1 = Player.Clone(gameState.p1, this);
            p2 = Player.Clone(gameState.p2, this);

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

            activePlayerOverride = null;

            // MCTS
            this.pst = pst;
            this.playoutStartPlayer = playoutStartPlayer.first ? p1 : p2;
            
            pureRandom = false;

            if (moveManager != null)
                moveManager.ResetToRoot();
            
        }


        internal Player playout()
        {
            if (isMainGame)
            {
                writeToConsole("");
                writeToConsole("--- NEW GAME STARTED ---");

                // Select Setup Cards
                p2.makeSetupDecisions();

                p1.makeSetupDecisions();

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
                    if (terminated)
                        return null;

                    writeToConsole("");
                    writeToConsole("BEAT " + beat);
                }
                
                this.nextBeat();



                // Debugging
                //if (moveManager != null)
                //{
                //    NodeStart z = moveManager.TraceOrigin();
                //    if (z != moveManager.rootNode)
                //        throw new NotImplementedException("gobshite");
                //}
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
            if (terminated)
                return;
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

            if (pst == PlayoutStartType.Normal || pst == PlayoutStartType.AnteSelection)
            {
                if (!isMainGame)
                    moveManager.ParallelInitialize();

                p2.selectAttackingPair();
                p1.selectAttackingPair();
                
            }
            else if (pst == PlayoutStartType.AttackPairSelection)
            {
                moveManager.ParallelInitialize();

                p2.selectAttackingPair();
                p1.selectAttackingPair();

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

                    if (moveManager != null)
                    {
                        if (p1.bases.Count > 1 && p2.bases.Count > 1)
                            moveManager.ParallelInitialize();                        
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

                else
                {
                    moveManager.SingleInitialize();
                    //moveManager.TraceOrigin();
                }

                activePlayer = p1.priority() > p2.priority() ? p1 : p2;
                Player reactivePlayer = activePlayer.opponent;

                if (isMainGame)
                    writeToConsole(activePlayer + " goes first");

                activePlayer.applyCommonProperties();
                reactivePlayer.applyCommonProperties();

                activePlayer.resolveStartOfBeat();
                reactivePlayer.resolveStartOfBeat();

                if (activePlayerOverride == reactivePlayer)
                {
                    activePlayer = reactivePlayer;
                    reactivePlayer = activePlayer.opponent;
                }


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

            // Clear the beat event variables
            activePlayerOverride = null;

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

                if (!anteingPlayer.canAnteFinisher() && !anteingPlayer.opponent.canAnte())
                {
                    if (isMainGame)
                        writeToConsole("Anteing finished since " + anteingPlayer.opponent + " cannot ante.");
                    break;
                }

                anteingPlayer = anteingPlayer.opponent;

                previous = current;

            }

        }


        internal void getUserChoice()
        {
            if (terminated)
            {
                selectionResult = 0;
                return;
            }

            bw.ReportProgress(1);
            waitHandle.WaitOne();
        }


        internal void MCTS_playouts(Player player, PlayoutStartType rpst, GameState fillOrigin, NodeStart startNode)
        {
            if (terminated)
                return;

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
                if (i == 1 || i % PLAYOUT_SCREEN_UPDATE_RATE == 0 || i == MAX_PLAYOUTS)
                {
                    if (terminated)
                        return;

                    playoutsDone = i;
                    bestWinrate = startNode.bestWinrate(player);

                    bw.ReportProgress(2);
                }

                copy.fillFromGameState(fillOrigin, rpst, player);
                copy.currentRegisteredChoice = 0;
                
                Player winner = copy.playout();

                NodeStart updateResult = copy.updateStats(winner);

                //if (updateResult != startNode)
                //    throw new NotImplementedException("oops");

                //if (startNode is ParallelStart)
                //{
                //    if (((ParallelStart)startNode).tree1.games != i + 1)
                //        throw new NotImplementedException("nope 1");
                //    if (((ParallelStart)startNode).tree2.games != i + 1)
                //        throw new NotImplementedException("nope 2");
                //}
                //else
                //{
                //    if (((SimpleStart)startNode).games != i + 1)
                //        throw new NotImplementedException("nope 3");
                //}

            }
            
        }


        private NodeStart updateStats(Player winner)
        {
            return moveManager.updateStats(winner);
        }


        internal AttackingPair MCTS_attackingPair(Player player)
        {
            ParallelStart rNode = new ParallelStart();
            
            MCTS_playouts(player, PlayoutStartType.AttackPairSelection, this, rNode);

            BestSequenceExtractor b = new BestSequenceExtractor(this, rNode, player);



            return new AttackingPair(b.getNextBest(player.styles.Count), b.getNextBest(player.bases.Count));
            
        }


        internal int MCTS_ante(Player player, int number)
        {
            ParallelStart rNode = new ParallelStart();

            MCTS_playouts(player, PlayoutStartType.AnteSelection, this, rNode);


            BestSequenceExtractor b = new BestSequenceExtractor(this, rNode, player);

            b.SelectFixed(player.selectedStyle);
            b.SelectFixed(player.selectedBase);

            return b.getNextBest(number);

        }


        internal int MCTS_clash(Player player, int number)
        {

            ParallelStart rNode = new ParallelStart();
            
            MCTS_playouts(player, PlayoutStartType.ClashResolution, this, rNode);

            BestSequenceExtractor b = new BestSequenceExtractor(this, rNode, player);

            return b.getNextBest(number);

        }


        private int MCTS_beatResolution(int number, Player p)
        {

            SimpleStart rNode = new SimpleStart();

            MCTS_playouts(p, PlayoutStartType.BeatResolution, checkPoint, rNode);

            BestSequenceExtractor b = new BestSequenceExtractor(this, rNode);

            return b.getNextBest(number);

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

            BestSequenceExtractor e = new BestSequenceExtractor(this, rNode, player);

            player.selectedCooldownStyle2 = e.getNextBest(5);
            player.selectedCooldownStyle1 = e.getNextBest(4);
            player.selectedCooldownBase2 = e.getNextBest(7);
            player.selectedCooldownBase1 = e.getNextBest(6);
            player.selectedFinisher = e.getNextBest(2);
            
        }


        internal void writeDebug(string p)
        {
            writeToConsole("[DEBUG] " + p);
        }
    }


}
