﻿using System;
using System.Collections.Generic;
namespace BattleCON
{

    public class Shekhtur : Player
    {

        public override string getDescription()
        {
            return "Shekhtur begins a duel with 3 Malice Tokens. Each time she hits an opponent with an attack, she gains one Malice Token per point of damage dealt. She cannot possess more than 5 Malice Tokens at any time.\nShekhtur can ante Malice Tokens for +1 Priority each.";
        }


        public Shekhtur() : base()
        {
            c = CharacterClass.CharacterShekhtur;

            bases.Add(new Brand());

            styles.Add(new Combination());
            styles.Add(new Unleashed());
            styles.Add(new Reaver());
            styles.Add(new Jugular());
            styles.Add(new Spiral());

            availableTokens = 3;
            usedTokens = 2;
        }


        public override void OnDamage()
        {
            gainTokens(damageDealt);
        }


        public override void AnteEffects()
        {
            priorityModifier += antedTokens;
        }


    }


    class Brand : BaseCard
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

        public override void AfterActivating(Player p, List<NamedHandler> handlers)
        {
            if (p.hasHit)
            {
                List<int> toSpend = new List<int>();
                
                // Option 0 - always present, means "don't spend tokens"
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

                    int i;
                    int tokens;

                    if (p.g.isMainGame && p.isHuman)
                    {
                        p.g.selectionHeader = "Spend tokens to drain life from the opponent:";
                        for (int j = 0; j < toSpend.Count; j++)
                        {
                            tokens = toSpend[j];
                            p.g.selectionItems.Add(tokens == 0 ? "Do nothing" : "Spend " + tokens + " tokens to drain " + j + " life");
                        }
                        p.g.getUserChoice();
                        i = p.g.selectionResult;
                    }
                    else
                        i = p.g.SimpleUCTSelect(toSpend.Count, p);

                    if (p.g.isMainGame)
                        p.g.registeredChoices.Add(i);
                        

                    tokens = toSpend[i];

                    if (tokens > 0)
                    {
                        p.spendTokens(tokens);
                        p.drainLife(tokens / 2);
                        if (p.g.isMainGame)
                            p.g.writeToConsole(p + " spends " + tokens + " token(s) to drain " + (tokens / 2) + " life.");
                    }
                    else
                    {
                        if (p.g.isMainGame)
                            p.g.writeToConsole(p + " decides not to spend tokens to drain life.");
                    }

                }

                else
                {
                    if (p.g.isMainGame)
                        p.g.writeToConsole(p + " doesn''t have enough tokens to drain life.");
                }
            }

            
        }

        internal override string getDescription()
        {
            return "This attack ignores Stun Guard if your priority is 6 or greater.\nAfter Activating: If this attack hit, you may spend 2 or 4 Malice Tokens. For every 2 tokens you spent, the opponent you hit loses 1 life, and you gain 1 life.";
        }
        

    }

    class Jugular : StyleCard
    {
        public Jugular()
        {
            name = "Jugular";
            power = 1;
            priority = 2;
        }

        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate()
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + "'s Jugular On Hit: Move " + p.opponent + " 1 space.");

                // Move the opponent 1 space
                p.UniversalMove(false, Direction.Both, 1, 1);
            });
        }

        public override void EndOfBeat(Player p)
        {
            // Gain or lose Malice Tokens until you have exactly 3.
            p.availableTokens = 3;
            p.usedTokens = 2;
        }

        internal override string getDescription()
        {
            return "On Hit: Move the opponent 1 space.\nEnd of Beat: Gain or lose Malice Tokens until you have exactly 3.";
        }
        
    }

    class Combination : StyleCard
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

        public override bool ignoresOpponentSoak(Player p)
        {
            return (p.priority() >= 7);
        }

        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            if (p.hitOpponentLastBeat)
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole("Combination: +2 power since hit opponent last beat.");
                p.powerModifier += 2;
            }
        }

        internal override string getDescription()
        {
            return "This attack does not hit opponents at range 3 or greater. This attack ignores Soak if your priority is 7 or greater.\nOn Hit: If you hit the opponent last beat, this attack has +2 Power.";
        }
        

    }


    class Spiral : StyleCard
    {
        public Spiral()
        {
            name = "Spiral";
            priority = -1;
        }

        public override void BeforeActivating(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate()
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + "'s Spiral Before Activating: Advance up to 3 spaces.");
                MovementResult mr = p.UniversalMove(true, Direction.Forward, 0, 3);
                p.powerModifier -= mr.distance;
                if (p.g.isMainGame && mr.distance > 0)
                    p.g.writeToConsole(this + " lost " + mr.distance + " power because of advance");
            });
        }

        internal override string getDescription()
        {
            return "Before Activating: Advance up to 3 spaces. You have -1 Power for each space moved by this effect (to a minimum of 0).";
        }
        

    }


    class Reaver : StyleCard
    {
        public Reaver()
        {
            name = "Reaver";
            hiRange = 1;
        }

        public override void OnDamage(Player p)
        {
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Reaver On Damage: Push " + p.opponent + " " + p.damageDealt + " space for damage dealt.");
            p.UniversalMove(false, Direction.Backward, p.damageDealt, p.damageDealt);
        }

        public override void EndOfBeat(Player p)
        {
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Reaver at End of Beat: Advance 1 or 2 spaces");
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }

        internal override string getDescription()
        {
            return "On Damage: Push the opponent one space per point of damage dealt.\nEnd of Beat: Advance 1 or 2 spaces.";
        }
        
    }

    class Unleashed : StyleCard
    {
        public Unleashed()
        {
            name = "Unleashed";
            hiRange = 1;
            power = -1;
        }

        public override void AfterActivating(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate()
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + "'s Unleashed After Activating: Retreat 1 or 2 spaces");
                p.UniversalMove(true, Direction.Backward, 1, 2);
            });
        }

        public override void EndOfBeat(Player p)
        {
            p.gainTokens(2);
            p.nextBeatPowerModifier += 1;
        }

        internal override string getDescription()
        {
            return "After Activating: Retreat 1 or 2 spaces.\nEnd of Beat: Gain 2 Malice Tokens. You have +1 Power next beat.";
        }
        
    }


    class SoulBreaker : Finisher
    {
        public SoulBreaker()
        {
            name = "Soul Breaker";
            lowRange = 1;
            hiRange = 1;
            power = 3;
            priority = 3;
        }

        internal override string getDescription()
        {
            return "On Damage: The opponent is stunned and is not able to declare antes for the rest of the duel (except mandatory antes dictated by his Unique Ability).";
        }

        public override void OnDamage(Player p)
        {
            p.opponent.BecomeStunned();
            p.opponent.cannotAnte = true;

        }

    }

    class CoffinNails : Finisher
    {
        public CoffinNails()
        {
            name = "Coffin Nails";
            lowRange = 1;
            hiRange = 1;
            power = 3;
            priority = 3;
        }

        internal override string getDescription()
        {
            return "The opponent is stunned and loses all Soak and Stun Guard for the rest of the duel.";
        }

        public override void OnDamage(Player p)
        {
            p.opponent.BecomeStunned();
            p.opponent.soakDisabled = true;
            p.opponent.stunGuardDisabled = true;

        }

    }
}