using System;
using System.Collections.Generic;

namespace BattleCON
{

    public class Eligor : Character
    {

        public Eligor()
        {
            name = "Eligor";
        }

        public override void init(Player p)
        {
            p.bases.Add(new Aegis());

            p.styles.Add(new Retribution());
            p.styles.Add(new Chained());
            p.styles.Add(new Martial());

            p.CooldownStyle1 = new Vengeful();
            p.CooldownStyle2 = new Counter();

            p.availableTokens = 2;
            p.usedTokens = 3;
        }

        public override void OnDamageTaken(Player p)
        {
            p.gainTokens(p.damageTaken);
        }

        public override void AnteEffects(Player p)
        {
            if (p.antedTokens == 5)
                p.stunImmunity = true;
            else
                p.stunGuard += 2 * p.antedTokens;
        }

    }


    class Aegis : Card
    {
        public Aegis()
        {
            name = "Aegis";
            lowRange = 1;
            hiRange = 1;
        }

        public override void Reveal(Player p)
        {
            p.soak += p.antedTokens;
        }

        public override int getAttackPower(Player p)
        {
            if (p.opponent.attackBase.power > 0)
                return p.opponent.attackStyle.power + p.opponent.attackBase.power;

            // otherwise 0
            return 0;
        }

    }


    class Vengeful : Card
    {
        public Vengeful()
        {
            name = "Vengeful";
            power = 1;
        }

        public override void CommonProperties(Player p)
        {
            p.stunGuard += 3;
            p.ignoresAppliedMovement = true;
        }

        public override void BeforeActivating(Player p)
        {
            p.UniversalMove(true, Direction.Forward, 1, 1);
        }

        public override void OnHit(Player p)
        {
            p.gainTokens(2);
        }
    }


    class Counter : Card
    {
        public Counter()
        {
            name = "Counter";
            power = 1;
            priority = -1;
        }

        public override void StartOfBeat(Player p)
        {
            if (p.opponent.attackBase.name == p.attackBase.name)
                p.opponent.isStunned = true;
        }

        public override void BeforeActivating(Player p)
        {
            if (p.damageTaken > 0)
            {
                p.UniversalMove(true, Direction.Forward, 0, p.damageTaken);
            }
        }

    }


    class Retribution : Card
    {
        public Retribution()
        {
            name = "Retribution";
            priority = -1;
        }

        public override void CommonProperties(Player p)
        {
            p.soak += 2;
        }

        public override void OnSoak(Player p)
        {
            p.gainTokens(Math.Min(2, p.soakedDamage));
        }

        public override void BeforeActivating(Player p)
        {
            if (p.wasHit)
            {
                // May move to any adjacent space to an opponent who hit
                List<int> newPos = new List<int>(3);
                newPos.Add(-1);

                if (p.opponent.position > 1 && p.position != p.opponent.position - 1)
                    newPos.Add(p.opponent.position - 1);

                if (p.opponent.position < 7 && p.position != p.opponent.position + 1)
                    newPos.Add(p.opponent.position + 1);

                if (newPos.Count > 1)
                {
                    int selected = p.g.rnd.Next(newPos.Count);
                    if (selected > 0)
                        p.position = newPos[selected];
                }
            }
        }
    }


    class Chained : Card
    {
        public Chained()
        {
            name = "Chained";
            priority = -1;
        }

        public override void BeforeActivating(Player p)
        {
            // Discard any number of Vengeance tokens to pull the opponent 1 space per token discarded
            int maxNumber = Math.Min(p.availableTokens, p.opponent.GetPossibleAdvance());
            if (maxNumber > 0)
            {
                int number = p.g.rnd.Next(maxNumber + 1);
                if (number > 0)
                {
                    p.opponent.Advance(number);
                    p.spendTokens(number);
                }
            }
        }
            
    }


    class Martial : Card
    {
        public Martial()
        {
            name = "Martial";
            hiRange = 1;
            power = 1;
            priority = -1;
        }

        public override void BeforeActivating(Player p)
        {
            if (p.damageTaken > 0)
                p.powerModifier += 2;
            if (p.availableTokens == 5)
                p.powerModifier += 2;
        }
    }

}