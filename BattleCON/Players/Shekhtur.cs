using System.Collections.Generic;
namespace BattleCON
{
    class Brand : Card
    {
        public Brand()
        {
            name = "Brand";
            lowRange = 1;
            hiRange = 2;
            power = 3;
            priority = 2;
        }


        protected override bool ignoresStunGuard(Player p)
        {
            return (p.priority() >= 6);
            
        }

        protected override void AfterActivating(Player p)
        {
            if (p.hasHit)
            {
                List<int> toSpend = new List<int>();
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
                    int tokens = toSpend[p.g.rnd.Next(0, toSpend.Count)];

                    if (tokens > 0)
                    {
                        p.spendTokens(tokens);
                        p.drainLife(tokens / 2);
                    }


                }
            }

            
        }

    }

}