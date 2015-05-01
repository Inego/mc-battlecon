using System;
using System.Collections.Generic;
using System.Drawing;
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
                selected = g.SimpleUCTSelect(realCap + 1, this);
            }

            concentration -= selected * size;


            if (g.isMainGame && selected > 0)
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


        public override void fillFromPlayer(Player player)
        {
            base.fillFromPlayer(player);
            Marmelee m = (Marmelee)player;
            concentration = m.concentration;
        }


        internal override void BecomeStunned()
        {
            base.BecomeStunned();

            if (concentration > 0)
            {
                concentration = 0;

                if (g.isMainGame)
                    g.writeToConsole(this + " is stunned and has lost her concentration!");
            }
        }

        internal override void Draw(Graphics drawingGraphics, int y, int battleSpaceY)
        {
            drawingGraphics.DrawString("Concentration: " + concentration, SystemFonts.DefaultFont, Brushes.Black, 5, y + 130);
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
            power = 1;
            priority = -1;
        }

        internal override string getDescription()
        {
            return "Start of Beat: You may spend 3 Concentration to become the Active player during this beat, disregarding priority.\nOn Hit: You may spend 2 Concentration to stun the opponent.";
        }


        public override void StartOfBeat(Player p)
        {
            Marmelee m = (Marmelee)p;
            if (p.g.activePlayer != p)
            {
                int spent = m.SpendConcentration(3, 1, delegate(int i) { return "Become Active player"; });
                if (spent > 0)
                {
                    p.g.activePlayerOverride = p;
                }
            }
            
        }

        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;
            int spent = m.SpendConcentration(2, 1, delegate(int i) { return "Stun the opponent"; });
            if (spent > 0)
            {
                m.opponent.BecomeStunned();
            }
        }

    }


    class Magnificent : StyleCard
    {
        public Magnificent()
        {
            name = "Magnificent";
            lowRange = 1;
            hiRange = 2;
            power = -1;
        }

        internal override string getDescription()
        {
            return "On Hit: You may spend any number of Concentration Counters for +1 Power each.\nAfter Activating: you may spend two Concentration Counters to move directly to any unoccupied space.";
        }

        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;
            int spent = m.SpendConcentration(1, -1, delegate(int i) { return "+" + i + " Power"; });
            if (spent > 0)
            {
                m.powerModifier += spent;
            }
        }

        public override void AfterActivating(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;

            if (m.canMove && m.concentration >= 2)
            {
                addHandler(handlers, delegate()
                {
                    int spent = m.SpendConcentration(2, 1, delegate(int i) { return "Move directly to any unoccupied space"; });
                    if (spent > 0)
                    {
                        m.Teleport();
                    }
                });
            }
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
            if (spent > 0)
            {
                m.opponent.canHit = false;                
            }
        }


        public override void BeforeActivating(Player p, List<NamedHandler> handlers)
        {
            if (p.rangeToOpponent() == 1)
            {
                int maxPush = p.opponent.GetPossibleRetreat(false);

                if (maxPush > 0)
                {
                    addHandler(handlers, delegate() {
                        Marmelee m = (Marmelee)p;
                        int spent = m.SpendConcentration(1, maxPush, delegate(int i) { return "Push opponent " + i + " space"; });
                        if (spent > 0)
                        {
                            p.UniversalMove(false, Direction.Backward, spent, spent);
                        }
                    });
                }
                
            }
        }
    }


    class Sorceress : StyleCard
    {
        public Sorceress()
        {
            name = "Sorceress";
            priority = -1;
        }

        internal override string getDescription()
        {
            return "Before Activating: You may spend a Concentration Counter to give this attack +0~2 range.\nEnd of Beat: You may spend a Concentration Counter to move 1 space.";
        }

        public override void BeforeActivating(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;
            int spent = m.SpendConcentration(1, 1, delegate(int i) { return "Range +0~2"; });
            if (spent > 0)
            {
                p.hiRangeModifier += 2;
            }
        }


        public override void EndOfBeat(Player p)
        {
            if (p.canMove)
            {
                Marmelee m = (Marmelee)p;
                int spent = m.SpendConcentration(1, 1, delegate(int i) { return "Move 1 space"; });
                if (spent > 0)
                {
                    p.UniversalMove(true, Direction.Both, 1, 1);
                }
            }
        }
    }


    class Nullifying : StyleCard
    {
        public Nullifying()
        {
            name = "Nullifying";
            hiRange = 1;
            priority = 1;
        }

        internal override string getDescription()
        {
            return "Start of Beat: Retreat 1 space.\nOn Hit: You may spend any number of Concentration Counters to weaken the opponent 1 Power per token spent.";
        }

        public override void StartOfBeat(Player p)
        {
            p.UniversalMove(true, Direction.Backward, 1, 1);
        }


        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;

            int spent = m.SpendConcentration(1, -1, delegate(int i)
            {
                return "Weaken opponent " + i.ToString();
            });

            if (spent > 0)
                p.opponent.powerModifier -= spent;
        }
    }


    internal class AstralTrance : Finisher
    {
        public AstralTrance()
        {
            name = "Astral Trance";
        }

        internal override string getDescription()
        {
            return "Soak 5, this attack does not hit.\nAfter Activating: Regain Concentration up to maximum.";
        }

        public override void CommonProperties(Player p)
        {
            p.soak += 5;
            p.canHit = false;
        }

        public override string getPowerText()
        {
            return "N/A";
        }

        public override string getRangeText()
        {
            return "N/A";
        }

        public override void AfterActivating(Player p, List<NamedHandler> handlers)
        {
            Marmelee m = (Marmelee)p;

            if (m.concentration < 5)
            {
                m.concentration = 5;
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + " regains maximum Concentration!");
            }
        }
    }


    internal class AstralCannon : Finisher
    {
        public AstralCannon()
        {
            name = "Astral Cannon";
            lowRange = 2;
            hiRange = 4;
            priority = 4;
        }


        public override void CommonProperties(Player p)
        {
            p.stunImmunity = true;
        }


        public override void StartOfBeat(Player p)
        {
            Marmelee m = (Marmelee)p;

            m.powerModifier += m.concentration * 2;

            if (m.g.isMainGame)
                m.g.writeToConsole(p + "'s Astral Cannon Power: " + (m.concentration * 2));

            m.concentration = 0;
        }

        

        internal override string getDescription()
        {
            return "Stun Immunity\nStart of Beat: Discard all Concentration Counters. This attack has +2 Power per token discarded.";
        }
    }


}
