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
            S
            if (size < 0 && concentration < 1
                || size > 0 && concentration < size)
                return 0;

            int selected;

            if (g.isMainGame && isHuman)
            {
                g.selectionHeader = "Spend Concentration:";

                g.selectionItems.Add("Don't spend");

                if (size > 0)
                    g.selectionItems.Add(cr(size) + " for " + size.ToString() + " concentration");
                else
                {
                    int top = cap == -1 ? concentration : Math.Min(concentration, cap);
                    for (int i = 1; i <= top; i++)
                        g.selectionItems.Add(cr(i) + " for " + i.ToString() + " concentration");
                }

                g.getUserChoice();
                selected = g.selectionResult;
            }
            else
            {
                int count;
                if (size == -1)
                    
                selected = g.SimpleUCTSelect(newPos.Count, p);
            }


            if (p.g.isMainGame)
                p.g.registeredChoices.Add(selected);

            return 0;
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

            int spent = m.SpendConcentration(-1, 1, delegate(int i) {
                return i.ToString() + " Soak";
            });
        }

    }


    class Petrifying : StyleCard
    {

    }


    class Magnificent : StyleCard
    {

    }


    class Barrier : StyleCard
    {

    }


    class Sorceress : StyleCard
    {

    }


    class Nullifying : StyleCard
    {

    }


    internal class AstralTrance : Finisher
    {

    }


    internal class AstralCannon : Finisher
    {

    }




}
