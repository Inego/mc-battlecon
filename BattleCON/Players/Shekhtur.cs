using System;
using System.Collections.Generic;
namespace BattleCON
{

    public class Shekhtur : Character
    {
        public Shekhtur()
        {
            name = "Shekhtur";

        }

        public override void init(Player p)
        {
            p.bases.Add(new Brand());

            p.styles.Add(new Reaver());
            p.styles.Add(new Jugular());
            p.styles.Add(new Spiral());

            p.CooldownStyle1 = new Unleashed();
            p.CooldownStyle2 = new Combination();

            p.availableTokens = 3;
            p.usedTokens = 2;
        }


        public override void OnDamage(Player p)
        {
            p.gainTokens(p.damageDealt);
        }

        public override void AnteEffects(Player p)
        {
            p.priorityModifier += p.antedTokens;
        }


    }


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


        public override bool ignoresStunGuard(Player p)
        {
            return (p.priority() >= 6);
            
        }

        public override void AfterActivating(Player p)
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
            name = "Jugular";
            power = 1;
            priority = 2;
        }

        public override void OnHit(Player p)
        {
            // Move the opponent 1 space
            p.UniversalMove(false, Direction.Both, 1, 1);
        }

        public override void EndOfBeat(Player p)
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
            name = "Combination";
            power = 2;
        }

        public override void checkCanHit(Player p)
        {
            if (p.rangeToOpponent() >= 3)
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(name + " is too far from opponent, can't hit.");
                p.canHit = false;
            }
        }

        public override bool ignoresSoak(Player p)
        {
            return (p.priority() >= 7);
        }

        public override void OnHit(Player p)
        {
            if (p.hitOpponentLastBeat)
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole("Combination: +2 power since hit opponent last beat.");
                p.powerModifier += 2;
            }
        }

    }


    class Spiral : Card
    {
        public Spiral()
        {
            name = "Spiral";
            priority = -1;
        }

        public override void BeforeActivating(Player p)
        {
            MovementResult mr = p.UniversalMove(true, Direction.Forward, 0, 3);
            p.powerModifier -= mr.distance;
            if (p.g.isMainGame && mr.distance > 0)
                p.g.writeToConsole(this + " lost " + mr.distance + " power because of advance");
        }
    }


    class Reaver : Card
    {
        public Reaver()
        {
            name = "Reaver";
            hiRange = 1;
        }

        public override void OnDamage(Player p)
        {
            p.UniversalMove(false, Direction.Backward, p.damageDealt, p.damageDealt);
        }

        public override void EndOfBeat(Player p)
        {
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }
    }

    class Unleashed : Card
    {
        public Unleashed()
        {
            name = "Unleashed";
            hiRange = 1;
            power = -1;
        }

        public override void AfterActivating(Player p)
        {
            p.UniversalMove(true, Direction.Backward, 1, 2);
        }

        public override void EndOfBeat(Player p)
        {
            p.gainTokens(2);
            p.nextBeatPowerModifier += 1;
        }
    }

}