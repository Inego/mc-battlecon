using System;
namespace BattleCON
{
    class Drive : Card
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
            // Advance 1 or 2 spaces
            p.UniversalMove(true, Direction.Forward, 1, 2);
        }

    }


    class Strike : Card
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


    }


    class Dash : Card
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
            MovementResult mr = p.UniversalMove(true, Direction.Both, 1, 3);

            if (mr.pastOpponent)
            {
                if (p.g.isMainGame)
                    p.g.writeToConsole(p + " dashed past " + p.opponent);
                p.opponent.canHit = false;
            }

        }


    }


    class Shot : Card
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


    }


    class Burst : Card
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
            // Retreat 1 or 2 spaces
            p.UniversalMove(true, Direction.Backward, 1, 2);

        }

    }


    class Grasp : Card
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
            // Move opponent 1 space

            p.UniversalMove(false, Direction.Both, 1, 1);

        }


    }

}