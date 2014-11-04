using System;
using System.Collections.Generic;

namespace BattleCON
{

    class Aegis : Card
    {
        public Aegis()
        {
            name = "Aegis";
            lowRange = 1;
            hiRange = 1;
        }

        protected override void Reveal(Player p)
        {
            p.soak += p.antedTokens;
        }

        protected override int getAttackPower(Player p)
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

        protected override void CommonProperties(Player p)
        {
            p.stunGuard += 3;
            p.ignoresAppliedMovement = true;
        }

        protected override void BeforeActivating(Player p)
        {
            p.UniversalMove(true, Direction.Forward, 1, 1);
        }

        protected override void OnHit(Player p)
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

        protected override void StartOfBeat(Player p)
        {
            if (p.opponent.attackBase.name == p.attackBase.name)
                p.isStunned = true;
        }

        protected override void BeforeActivating(Player p)
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

        protected override void CommonProperties(Player p)
        {
            p.soak += 2;
        }

        protected override void OnSoak(Player p)
        {
            p.gainTokens(Math.Min(2, p.soakedDamage));
        }

        protected override void BeforeActivating(Player p)
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

                }
            }
        }
    }
}