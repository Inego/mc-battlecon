using System;
namespace BattleCON
{
    class Drive : BaseCard
    {

        public Drive()
        {
            name = "Drive";
            lowRange = 1;
            hiRange = 1;
            power = 3;
            priority = 4;
        }

        public override void BeforeActivating(Player p)
        {

            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Drive Before Activating: Advance 1 or 2 spaces.");

            // Advance 1 or 2 spaces
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }

        internal override string getDescription()
        {
            return "Before Activating: Advance 1 or 2 spaces.";
        }

    }


    class Strike : BaseCard
    {

        public Strike()
        {
            name = "Strike";
            lowRange = 1;
            hiRange = 1;
            power = 4;
            priority = 3;
        }

        public override void CommonProperties(Player p)
        {
            p.stunGuard += 5;
        }

        internal override string getDescription()
        {
            return "Stun Guard 5";
        }


    }


    class Dash : BaseCard
    {

        public Dash()
        {
            name = "Dash";
            priority = 9;
        }

        public override void CommonProperties(Player p)
        {
            p.canHit = false;
        }

        public override void AfterActivating(Player p)
        {
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Dash After Activating: Move 1, 2 or 3 spaces.");

            MovementResult mr = p.UniversalMove(true, Direction.Both, 1, 3);

            if (mr.pastOpponent)
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + " dashed past " + p.opponent);
                p.opponent.canHit = false;
            }

        }

        public override string getRangeText()
        {
            return "N/A";
        }

        public override string getPowerText()
        {
            return "N/A";
        }

        internal override string getDescription()
        {
            return "This attack does not hit opponents.\nAfter Activating: Move 1, 2 or 3 spaces. If you moved past an opponent during this movement, this opponent cannot hit you during this beat.";
        }


    }


    class Shot : BaseCard
    {

        public Shot()
        {
            name = "Shot";
            lowRange = 1;
            hiRange = 4;
            power = 3;
            priority = 2;
        }

        public override void CommonProperties(Player p)
        {
            p.stunGuard += 2;
        }

        internal override string getDescription()
        {
            return "Stun Guard 2";
        }


    }


    class Burst : BaseCard
    {

        public Burst()
        {
            name = "Burst";
            lowRange = 2;
            hiRange = 3;
            power = 3;
            priority = 1;
        }

        public override void StartOfBeat(Player p)
        {
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Burst at Start of Beat: Retreat 1 or 2 spaces");


            // Retreat 1 or 2 spaces
            p.UniversalMove(true, Direction.Backward, 1, 2);

        }

        internal override string getDescription()
        {
            return "Start of Beat: Retreat 1 or 2 spaces.";
        }

    }


    class Grasp : BaseCard
    {

        public Grasp()
        {
            name = "Grasp";
            lowRange = 1;
            hiRange = 1;
            power = 2;
            priority = 5;
        }

        public override void OnHit(Player p)
        {
            if (p.g.isMainGame)
                p.g.writeToConsole(p + "'s Grasp On Hit: Move the opponent 1 space.");

            // Move opponent 1 space
            p.UniversalMove(false, Direction.Both, 1, 1);
        }

        internal override string getDescription()
        {
            return "On Hit: Move the opponent 1 space.";
        }


    }

}