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

}