using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{
    class Marmelee : Player
    {

        public int concentration = 2; 

        public Marmelee() : base()
        {
            c = CharacterClass.CharacterMarmelee;

            bases.Add(new Meditation());

            styles.Add(new Petrifying());
            styles.Add(new Magnificent());
            styles.Add(new Barrier());
            styles.Add(new Sorceress());
            styles.Add(new Nullifying());
            
        }


        public override string getDescription()
        {
            return "Marmelee begins a duel with 2 Concentration Counters. During the recycle step, she gains 1 Concentration Counter, and has a maximum of 5 of these tokens.\nWhenever Marmelee is stunned, she loses all Concentration Counters she posesses.";
        }


        public int SpendConcentration(int size, int cap, CountRepresenter cr)
        {
            int realCap = concentration / size;

            if (cap != -1)
                realCap = Math.Min(cap, realCap);

            if (realCap == 0)
                return 0;

            int selected;

            if (g.isMainGame && isHuman)
            {
                g.selectionHeader = "Spend Concentration:";

                g.selectionItems.Add("Don't spend");

                for (int i = 1; i <= realCap; i++)
                    g.selectionItems.Add(cr(i) + " for " + i.ToString() + " concentration");

                g.getUserChoice();
                selected = g.selectionResult;
            }
            else
            {
                if (!g.moveManager.pureRandom && g.moveManager.parallel)
                    throw new NotImplementedException("nein");
                selected = g.SimpleUCTSelect(realCap + 1, this);
            }

            concentration -= selected * size;


            if (g.isMainGame)
            {
                g.registeredChoices.Add(selected);
                g.writeToConsole(this + " gains " + cr(selected) + " for " + (selected * size) + " concentration.");
            }

            return selected;
        }


        internal override void recycle()
        {
            base.recycle();

            if (concentration < 5)
            {
                concentration++;
                if (g.isMainGame)
                    g.writeToConsole(this + " concentrates.");
            }
        }

    }


    class Meditation : BaseCard
    {

        public Meditation()
        {
            name = "Meditation";
            hiRange = 1;
            lowRange = 1;
            power = 2;
            priority = 3;
        }

        public override void StartOfBeat(Player p)
        {
            Marmelee m = (Marmelee)p;

            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Meditation Start of Beat: Buy Soak.");

            int spent = m.SpendConcentration(1, -1, delegate(int i) {
                return i.ToString() + " Soak";
            });

            if (spent > 0)
                p.soak += spent;                
        }


        public override void EndOfBeat(Player p)
        {
            Marmelee m = (Marmelee)p;

            if (m.concentration < 5)
            {
                m.concentration++;
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + "'s Meditation End of Beat: gain 1 Concentration.");
            }
            
        }

        internal override string getDescription()
        {
            return "Start of Beat: Spend any number of Concentration counters for Soak 1 each.\nEnd of Beat: Gain one Concentration Counter.";
        }

    }


    class Petrifying : StyleCard
    {
        public Petrifying()
        {
            name = "Petrifying";
        }

    }


    class Magnificent : StyleCard
    {
        public Magnificent()
        {
            name = "Magnificent";
        }

    }


    class Barrier : StyleCard
    {
        public Barrier()
        {
            hiRange = 1;
            power = -1;
            name = "Barrier";
        }

        internal override string getDescription()
        {
            return "Start of Beat: You may spend 4 Concentration. If you do, attacks do not hit you during this beat.\nBefore Activating, range 1: You may spend any number of Concentration Counters to push an opponent at range 1 one space per token.";
        }


        public override void StartOfBeat(Player p)
        {
            Marmelee m = (Marmelee)p;
            int spent = m.SpendConcentration(4, 1, delegate(int i) { return "Immunity to getting hit"; });
            if (spent > 1)
            {
                m.opponent.canHit = false;                
            }
        }
    }


    class Sorceress : StyleCard
    {
        public Sorceress()
        {
            name = "Sorceress";
        }
    }


    class Nullifying : StyleCard
    {
        public Nullifying()
        {
            name = "Nullifying";
        }
    }


    internal class AstralTrance : Finisher
    {
        public AstralTrance()
        {
            name = "AstralTrance";
        }
    }


    internal class AstralCannon : Finisher
    {
        public AstralCannon()
        {
            name = "AstralCannon";
        }
    }


}
