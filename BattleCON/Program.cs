using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{

    enum Direction { Forward, Backward, Both };

    class MovementResult
    {
        public bool advance; // false if retreat
        public int distance;
        public bool pastOpponent;

        public static MovementResult noMovement = new MovementResult(true, 0, false);

        public MovementResult(bool advance, int distance, bool pastOpponent)
        {
            this.advance = advance;
            this.distance = distance;
            this.pastOpponent = pastOpponent;

        }


    }

    class Player
    {
        public int health;
        public bool isDead = false;
        public int position;
        public Player opponent;

        public int antedTokens;
        public int availableTokens;
        public int usedTokens;

        public int stunGuard;
        public bool canHit;

        public bool hasHit;

        public int soak;

        public Card attackStyle;
        public Card attackBase;


        public void resetBeat()
        {
            stunGuard = 0;
            canHit = true;
            antedTokens = 0;
            soak = 0;

            hasHit = false;
        }

        public GameState g;

        public Character c;
        

        public Player(Character c, int position)
        {
            health = 20;
            this.c = c;
            this.position = position;

            resetBeat();
        }

        public MovementResult Advance(int i)
        {
            bool result;
            if (opponent.position > position)
            {
                result = position + i >= opponent.position;
                position += i + (result ? 1 : 0);
            }
            else
            {
                result = position - i <= opponent.position;
                position -= i + (result ? 1 : 0);
            }
            return new MovementResult(true, i, result);
        }

        public MovementResult Retreat(int i)
        {
            if (opponent.position > position)
                position -= i;
            else
                position += i;
            return new MovementResult(false, i, false);
        }

        public MovementResult MoveSelf(int i)
        {
            if (i < 0)
            {
                return Retreat(-i);
            }

            return Advance(i);

        }


        public int GetPossibleAdvance()
        {
            if (opponent.position > position)
                return 6 - position;
            else
                return position - 2;
        }

        public int GetPossibleRetreat()
        {
            if (opponent.position > position)
                return position - 1;
            else
                return 7 - position;
        }


        internal void MoveOpponent(int i)
        {
            opponent.MoveSelf(i);
        }


        internal MovementResult UniversalMove(bool self, Direction direction, int loRange, int hiRange)
        {
            List<int> moves = new List<int>(13);

            Player p = self ? this : opponent;

            int maxMoves;
            int maxPossible;
            int i;

            if (direction == Direction.Both || direction == Direction.Forward)
            {
                maxPossible = p.GetPossibleAdvance();

                if (maxPossible >= loRange)
                {
                    maxMoves = Math.Min(maxPossible, hiRange);
                    for (i = loRange; i <= maxMoves; i++)
                        moves.Add(i);
                }
            }

            if (direction == Direction.Both || direction == Direction.Backward)
            {
                maxPossible = p.GetPossibleRetreat();

                if (maxPossible >= loRange)
                {
                    maxMoves = Math.Min(maxPossible, hiRange);
                    for (i = loRange; i <= maxMoves; i++)
                    {
                        if (direction == Direction.Both && i == 0)
                            continue;
                        moves.Add(-i);
                    }
                }
            }

            if (moves.Count > 0)
            {
                int moveNumber;
                if (moves.Count == 1)
                    moveNumber = 0;
                else
                    moveNumber = this.g.rnd.Next(moves.Count);

                return p.MoveSelf(moveNumber);

            }

            return MovementResult.noMovement;


        }


        public int priority()
        {
            return attackBase.priority + attackStyle.priority;
        }

        public void loseLife(int pts)
        {
            health -= pts;

            if (health <= 0 && !opponent.isDead)
            {
                isDead = true;
            }
        }

        internal void spendTokens(int tokens)
        {
            availableTokens -= tokens;
            usedTokens += tokens;
        }

        internal void drainLife(int p)
        {
            health += p;
            opponent.loseLife(p);
        }
    }


  
    class GameState
    {
        Player p1;
        Player p2;

        int beat;

        public Random rnd = new Random();

        public GameState(Character c1, Character c2)
        {
            beat = 0;

            p1 = new Player(c1, 2);
            p2 = new Player(c2, 6);

            p1.opponent = p2;
            p2.opponent = p1;

            p1.g = this;
            p2.g = this;
        }
    }


    class Card
    {
        protected string name;

        protected int lowRange = 0;
        protected int hiRange = 0;
        public int power = 0;
        public int priority = 0;

        virtual protected void CommonProperties(Player p)
        {

        }

        virtual protected void Reveal(Player p)
        {

        }

        virtual protected void StartOfBeat(Player p)
        {

        }

        virtual protected void BeforeActivating(Player p)
        {

        }


        virtual protected void OnHit(Player p)
        {

        }

        virtual protected void OnDamage(Player p)
        {

        }

        virtual protected void AfterActivating(Player p)
        {

        }

        virtual protected void EndOfBeat(Player p)
        {

        }

        virtual protected int getAttackPower(Player p)
        {
            return this.power;
        }

        virtual protected bool ignoresStunGuard(Player p)
        {
            return false;
        }

    }

    class Drive : Card
    {

        public Drive()
        {
            name = "Drive";
            lowRange = 1;
            hiRange = 1;
            power = 3;
            priority = 4;
        }

        protected override void BeforeActivating(Player p)
        {
            // Advance 1 or 2 spaces
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }

    }


    class Strike : Card
    {

        public Strike()
        {
            name = "Strike";
            lowRange = 1;
            hiRange = 1;
            power = 4;
            priority = 3;
        }

        protected override void CommonProperties(Player p)
        {
            p.stunGuard += 5;
        }


    }


    class Dash : Card
    {

        public Dash()
        {
            name = "Dash";
            priority = 9;
        }

        protected override void CommonProperties(Player p)
        {
            p.canHit = false;
        }

        protected override void AfterActivating(Player p)
        {
            MovementResult mr = p.UniversalMove(true, Direction.Both, 1, 3);

            if (mr.pastOpponent)
                p.opponent.canHit = false;

        }


    }


    class Shot : Card
    {

        public Shot()
        {
            name = "Shot";
            lowRange = 1;
            hiRange = 4;
            power = 3;
            priority = 2;
        }

        protected override void CommonProperties(Player p)
        {
            p.stunGuard += 2;
        }


    }


    class Burst : Card
    {

        public Burst()
        {
            name = "Burst";
            lowRange = 2;
            hiRange = 3;
            power = 3;
            priority = 1;
        }

        protected override void StartOfBeat(Player p)
        {
            // Retreat 1 or 2 spaces
            p.UniversalMove(true, Direction.Backward, 1, 2);

        }

    }


    class Grasp : Card
    {

        public Grasp()
        {
            name = "Grasp";
            lowRange = 1;
            hiRange = 1;
            power = 2;
            priority = 5;
        }

        protected override void OnHit(Player p)
        {
            // Move opponent 1 space

            p.UniversalMove(false, Direction.Both, 1, 1);

        }


    }


    class Character
    {
        string name;

        public static Character shekhtur;
        public static Character eligor;
        
        static Character() {

            shekhtur = new Character("Shekhtur");
            eligor = new Character("Eligor");
            
        }

        public Character(string name)
        {
            this.name = name;
        }
    }

   
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hi!");
            Console.ReadLine();
        }
        
    }
}
