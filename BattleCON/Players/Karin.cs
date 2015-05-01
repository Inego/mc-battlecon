using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{
    public class Karin : Player
    {

        public int jager;

        public Karin() : base()
        {
            c = CharacterClass.CharacterKarin;

            bases.Add(new Claw());

            styles.Add(new Howling());
            styles.Add(new Coordinated());
            styles.Add(new Dual());
            styles.Add(new Feral());
            styles.Add(new FullMoon());

        }


        public override void init()
        {
            jager = first ? 3 : 5;
        }


        public override void fillFromPlayer(Player player)
        {
            base.fillFromPlayer(player);
            Karin k = (Karin)player;
            jager = k.jager;
        }

        internal override void Draw(Graphics drawingGraphics, int y, int battleSpaceY)
        {
            drawingGraphics.DrawString("JAGER",
                SystemFonts.DefaultFont,
                first ? Brushes.Blue : Brushes.Red,
                BattleBoard.spcSpacing + 7 + (jager - 1) * (BattleBoard.spcSize + BattleBoard.spcSpacing),
                battleSpaceY + (first ? 105 : 25));
        }


        internal void MoveJager(bool obligatory)
        {
            List<int> positions = new List<int>(3);
            
            if (!obligatory)
                positions.Add(0);

            if (jager > 1)
                positions.Add(-1);

            if (jager < 7)
                positions.Add(1);

            int moveNumber;
            int i;

            if (g.isMainGame && isHuman)
            {
                g.selectionHeader = "Select Jager's movement:";
                
                string movementText;
                for (int j = 0; j < positions.Count; j++)
                {
                    i = positions[j];
                    if (i == 0)
                        movementText = "Don't move";
                    else if (i == 1)
                        movementText = "To the right";
                    else
                        movementText = "To the left";
                    g.selectionItems.Add(movementText);
                }
                    
                g.getUserChoice();
                moveNumber = g.selectionResult;
            }
            else
                moveNumber = g.SimpleUCTSelect(positions.Count, this);

            if (g.isMainGame)
                g.registeredChoices.Add(moveNumber);

            i = positions[moveNumber];

            jager += i;

            if (g.isMainGame)
            {
                if (i == 0)
                    g.writeToConsole(this + " didn't move Jager.");
                else if (i == 1)
                    g.writeToConsole(this + " moved Jager to the right.");
                else
                    g.writeToConsole(this + " moved Jager to the left.");
            }
            
        }

        internal void TeleportJager()
        {

            List<int> positions = new List<int>(7);
        }
    }
    

    class Claw : BaseCard
    {
        public Claw()
        {
            name = "Claw";
            lowRange = 1;
            hiRange = 2;
            power = 2;
            priority = 4;
        }


        internal override string getDescription()
        {
            return "Before Activating: Advance 1 or 2 spaces.\nOn Hit: Move Jager one space. If he moves out of the opponent's space, this attack has +2 power.";
        }


        public override void BeforeActivating(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate() {
                p.UniversalMove(true, Direction.Forward, 1, 2);
            });
        }


        public override void OnHit(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate()
            {
                Karin k = (Karin)p;
                if (k.jager == p.opponent.position)
                    k.powerModifier += 2;
                k.MoveJager(true);
            });
        }

    }


    class Howling : StyleCard
    {
        public Howling()
        {
            name = "Howling";
            lowRange = -1;
            priority = 1;
        }

        internal override string getDescription()
        {
            return "Stun Immunity. This attack calculates its range from Jager's position.\nOn Hit: This attack has +2 power if Jager occupies the same space as the opponent.\nEnd of Beat: Move Jager up to one space.";
        }

    }


    class Coordinated : StyleCard
    {
        public Coordinated()
        {
            name = "Coordinated";
            hiRange = 1;
        }

        internal override string getDescription()
        {
            return "Opponents cannot move into Jager's space this beat. An opponent who would do so takes 2 damage instead.\nEnd of Beat: Move Jager up to 1 space.";
        }
    }


    class Dual : StyleCard
    {
        public Dual()
        {
            name = "Dual";
        }

        internal override string getDescription()
        {
            return "Before Activating: Move Jager to any unoccupied space. An opponent who was in the same space as Jager moves with him.";
        }

        public override void BeforeActivating(Player p, List<NamedHandler> handlers)
        {
            addHandler(handlers, delegate()
            {
                Karin k = (Karin)p;
                bool withOpponent = (p.opponent.position == k.jager && p.opponent.canMove);
                k.TeleportJager();
                if (withOpponent)
                    p.opponent.position = k.jager;

            });
        }
    }

    class Feral : StyleCard
    {
        public Feral()
        {
            name = "Feral";
            priority = 1;
        }

        internal override string getDescription()
        {
            return "Start of Beat: Advance 1 or 2 spaces.\nOn Hit: Retreat 2 spaces. If the opponent is in the same space as Jager, he is stunned.\nEnd of Beat: Move up to 1 space.";
        }
    }

    class FullMoon : StyleCard
    {
        public FullMoon()
        {
            name = "Full Moon";
            hiRange = 1;
            priority = 1;
        }

        internal override string getDescription()
        {
            return "This attack has Soak 2 if Jager is between Karin and the opponent, and +2 Power if Jager is behind the opponent.\nStart of Beat: If Jager's space is not occupied by an opponent, Jager and Karin may swap places.";
        }
    }


    class LunarCross : Finisher
    {
        public LunarCross()
        {
            name = "Lunar Cross";
            power = 6;
            priority = 5;
        }

        public override string getRangeText()
        {
 	        return "X";
        }

        internal override string getDescription()
        {
            return "Before Activating: If Jager's space is unoccupied, Jager and Karin swap places. The range of this attack is all spaces occupied by opponents you switched sides with this beat.";
        }
    }

    class RedMoonRage : Finisher
    {
        public RedMoonRage()
        {
            name = "Red Moon Rage";
            power = 10;
            priority = 12;
        }

        public override string getRangeText()
        {
            return "X";
        }

        internal override string getDescription()
        {
            return "An opponent is in range of this attack if Jager and Karin are adjacent to that opponent on opposite sides.";
        }
    }

}
