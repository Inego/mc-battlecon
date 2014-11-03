using System.Collections.Generic;
namespace BattleCON
{
    class Brand : Card
    {
        public Brand()
        {
            name = "Brand";
            lowRange = 1;
            hiRange = 2;
            power = 3;
            priority = 2;
        }


        protected override bool ignoresStunGuard(Player p)
        {
            return (p.priority() >= 6);
            
        }

        protected override void AfterActivating(Player p)
        {
            if (p.hasHit)
            {
                List<int> toSpend = new List<int>();
                toSpend.Add(0);

                if (p.availableTokens >= 2)
                {
                    toSpend.Add(2);

                    if (p.availableTokens >= 4)
                    {
                        toSpend.Add(4);
                    }
                }

                if (toSpend.Count > 1)
                {
                    int tokens = toSpend[p.g.rnd.Next(0, toSpend.Count)];

                    if (tokens > 0)
                    {
                        p.spendTokens(tokens);
                        p.drainLife(tokens / 2);
                    }


                }
            }

            
        }

    }

    class Jugular : Card
    {
        public Jugular()
        {
            power = 1;
            priority = 2;
        }

        protected override void OnHit(Player p)
        {
            // Move the opponent 1 space
            p.UniversalMove(false, Direction.Both, 1, 1);
        }

        protected override void EndOfBeat(Player p)
        {
            // Gain or lose Malice Tokens until you have exactly 3.
            p.availableTokens = 3;
            p.usedTokens = 2;
        }
    }

    class Combination : Card
    {
        public Combination()
        {
            power = 2;
        }

        protected override void checkCanHit(Player p)
        {
            if (p.rangeToOpponent() >= 3)
                p.canHit = false;
        }

        protected override bool ignoresSoak(Player p)
        {
            return (p.priority() >= 7);
        }

        protected override void OnHit(Player p)
        {
            if (p.hitOpponentLastBeat)
                p.powerModifier += 2;            
        }

    }


    class Spiral : Card
    {
        public Spiral()
        {
            priority = -1;
        }

        protected override void BeforeActivating(Player p)
        {
            MovementResult mr = p.UniversalMove(true, Direction.Forward, 0, 3);
            p.powerModifier -= mr.distance;
        }
    }


    class Reaver : Card
    {
        public Reaver()
        {
            hiRange = 1;
        }

        protected override void OnDamage(Player p)
        {
            p.UniversalMove(false, Direction.Backward, p.damageDealt, p.damageDealt);
        }

        protected override void EndOfBeat(Player p)
        {
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }
    }

    class Unleashed : Card
    {
        public Unleashed()
        {
            hiRange = 1;
            power = -1;
        }

        protected override void AfterActivating(Player p)
        {
            p.UniversalMove(true, Direction.Backward, 1, 2);
        }

        protected override void EndOfBeat(Player p)
        {
            p.gainTokens(2);
            p.nextBeatPowerModifier += 2;
        }
    }

}