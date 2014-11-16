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

        public override string getDescription()
        {
            return "Eligor begins a duel with 2 Vengeance Tokens. Whenever he takes damage from an attack, he gains Vengeance Tokens equal to the damage taken. He has a limit of 5 these tokens.\nEligor can ante Vengeance Tokens for Stun Guard 2 each. He gains Stun Immunity if he antes 5 tokens in a single beat.";
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


    class Aegis : BaseCard
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

        public override string getPowerText()
        {
            return "X";
        }

        internal override string getDescription()
        {
            return "Reveal: Soak 1 for each Vengeance Token you anted during this beat.\nThe power of this base is equal to the printed power of the nearest opponent's attack pair.\n(This is 0 if the opponent has X or N/A power on his base.)";
        }

    }


    class Vengeful : StyleCard
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
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Vengeful Before Activating: Advance 1 space.");
            p.UniversalMove(true, Direction.Forward, 1, 1);
        }

        public override void OnHit(Player p)
        {
            p.gainTokens(2);
        }

        internal override string getDescription()
        {
            return "Stun Guard 3\nEligor ignores all movement applied to him by opponents during this beat.\nBefore Activating: Advance 1 space.\nOn Hit: Gain 2 Vengeance Tokens.";
        }
    }


    class Counter : StyleCard
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
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + "'s Counter Before Activating: Since " + p.damageTaken + "damage taken, advance up to " + p.damageTaken + ".");
                p.UniversalMove(true, Direction.Forward, 0, p.damageTaken);
            }
        }

        internal override string getDescription()
        {
            return "Start of Beat: If an opponent's base has the same name as yours, that opponent is stunned.\nBefore Activating: If you took damage this beat, advance up to 1 space for each point of damage taken.";
        }

    }


    class Retribution : StyleCard
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
                if (p.g.isMainGame)
                    p.g.writeToConsole("Since " + p + " was hit, he may move directly to any space adjacent to an opponent who hit him.");

                // May move to any adjacent space to an opponent who hit
                List<int> newPos = new List<int>(3);
                newPos.Add(-1);

                if (p.opponent.position > 1 && p.position != p.opponent.position - 1)
                    newPos.Add(p.opponent.position - 1);

                if (p.opponent.position < 7 && p.position != p.opponent.position + 1)
                    newPos.Add(p.opponent.position + 1);

                if (newPos.Count > 1)
                {
                    int selected;
                    if (p.g.isMainGame && p.isHuman)
                    {
                        p.g.selectionHeader = "Select Retribution movement:";
                        int newposj;
                        for (int j = 0; j < newPos.Count; j++)
                        {
                            newposj = newPos[j];
                            p.g.selectionItems.Add(newposj == -1 ? "Don't move" : (newposj == p.opponent.position - 1 ? "Move to the left side of opponent" : "Move to the right side of opponent"));
                        }
                        p.g.getUserChoice();
                        selected = p.g.selectionResult;
                    }
                    else
                        selected = p.g.UCTSelect(newPos.Count, p, true);

                    if (p.g.isMainGame)
                        p.g.registeredChoices.Add(selected);

                    if (selected > 0)
                        p.position = newPos[selected];
                }
            }
        }

        internal override string getDescription()
        {
            return "Soak 2, gain a Vengeance Token for each point of damage soaked (max 2).\nBefore Activating: If you were hit during this beat, you may move directly to any space adjacent to an opponent who hit you.";
        }
    }


    class Chained : StyleCard
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

                int number;

                if (p.g.isMainGame && p.isHuman)
                {
                    p.g.selectionHeader = "Spend tokens to pull the opponent:";
                    for (int j = 0; j <= maxNumber; j++)
                    {
                        p.g.selectionItems.Add(j == 0 ? "None" : "Spend " + 1 + " tokens to pull " + j + " space");
                    }
                    p.g.getUserChoice();
                    number = p.g.selectionResult;
                }
                else
                    number = p.g.UCTSelect(maxNumber + 1, p, true);

                if (p.g.isMainGame)
                    p.g.registeredChoices.Add(number);
                    
                if (number > 0)
                {
                    p.opponent.Advance(number);
                    p.spendTokens(number);

                    if (p.g.isMainGame)
                        p.g.writeToConsole(p + " pulled " + p.opponent + " " + number + " spaces for " + number + " tokens.");
                }
            }
        }

        internal override string getDescription()
        {
            return "Before Activating: Discard any number of Vengeance tokens to pull the opponent 1 space per token discarded.";
        }
            
    }


    class Martial : StyleCard
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

        internal override string getDescription()
        {
            return "Before Activating: This attack has +2 Power if you have taken damage during this beat and +2 additional Power if you have 5 Vengeance Tokens in your token pool.";
        }
        

    }

}