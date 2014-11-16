using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{

    public class AttackingPair
    {
        public int styleNumber;
        public int baseNumber;

        public AttackingPair(int styleCard, int baseCard)
        {
            this.styleNumber = styleCard;
            this.baseNumber = baseCard;
        }
    }


    public class Player
    {
        public int health;
        public bool isDead = false;
        public int position;
        public Player opponent;

        public bool first;

        public int antedTokens;
        public int availableTokens;
        public int usedTokens;

        public int stunGuard;
        public bool stunImmunity;
        public bool isStunned;
        public bool canHit;

        public bool ignoresAppliedMovement;

        public bool hasHit;
        public bool wasHit;
        public bool hitOpponentLastBeat;

        public int soakedDamage;

        public int damageDealt;
        public int damageTaken;

        public int soak;

        public int powerModifier;
        public int priorityModifier;

        public int nextBeatPowerModifier;

        public List<Card> bases = new List<Card>(5);
        public List<Card> styles = new List<Card>(3);
        public List<Card> clashPool = new List<Card>(4);


        public int selectedStyle;
        public int selectedBase;

        public Card attackStyle;
        public Card attackBase;

        public Card CooldownBase1;
        public Card CooldownBase2;
        public Card CooldownStyle1;
        public Card CooldownStyle2;

        

        public GameState g;

        public Character c;

        public bool isHuman;


        public void fillFromPlayer(Player player)
        {
            health = player.health;
            isDead = player.isDead;
            position = player.position;
            first = player.first;
            antedTokens = player.antedTokens;
            availableTokens = player.availableTokens;
            usedTokens = player.usedTokens;
            stunGuard = player.stunGuard;
            stunImmunity = player.stunImmunity;
            isStunned = player.isStunned;
            canHit = player.canHit;
            ignoresAppliedMovement = player.ignoresAppliedMovement;
            hasHit = player.hasHit;
            wasHit = player.wasHit;
            hitOpponentLastBeat = player.hitOpponentLastBeat;
            soakedDamage = player.soakedDamage;
            damageDealt = player.damageDealt;
            damageTaken = player.damageTaken;
            soak = player.soak;
            powerModifier = player.powerModifier;
            priorityModifier = player.priorityModifier;
            nextBeatPowerModifier = player.nextBeatPowerModifier;
            
            bases.Clear();
            foreach (Card z in player.bases)
                bases.Add(z);

            styles.Clear();
            foreach (Card z in player.styles)
                styles.Add(z);

            clashPool.Clear();
            foreach (Card z in player.clashPool)
                clashPool.Add(z);

            attackStyle = player.attackStyle;
            attackBase = player.attackBase;
            selectedStyle = player.selectedStyle;
            selectedBase = player.selectedBase;
            CooldownBase1 = player.CooldownBase1;
            CooldownBase2 = player.CooldownBase2;
            CooldownStyle1 = player.CooldownStyle1;
            CooldownStyle2 = player.CooldownStyle2;

        }

        
        // For cloning
        public Player(Character c, GameState gs)
        {
            this.g = gs;
            this.c = c;
        }


        public Player(Character c, int position, GameState gs, bool first, bool isHuman)
        {
            this.g = gs;
            this.first = first;
            this.isHuman = isHuman;

            health = 20;
            this.c = c;
            c.init(this);
            this.position = position;
            this.hasHit = false;

            this.nextBeatPowerModifier = 0;

            // Six standard bases
            bases.Add(new Burst());
            bases.Add(new Drive());
            bases.Add(new Strike());
            bases.Add(new Shot());

            CooldownBase1 = new Dash();
            CooldownBase2 = new Grasp();

            resetBeat();
        }


        public void resetBeat()
        {

            stunGuard = 0;
            canHit = true;
            antedTokens = 0;
            soak = 0;

            hitOpponentLastBeat = hasHit;
            hasHit = false;
            wasHit = false;
            stunImmunity = false;
            isStunned = false;

            ignoresAppliedMovement = false;

            damageDealt = 0;
            damageTaken = 0;

            powerModifier = nextBeatPowerModifier;

            if (g.isMainGame && nextBeatPowerModifier > 0)
                g.writeToConsole(this + " has " + powerModifier + " power next beat.");

            nextBeatPowerModifier = 0;

            priorityModifier = 0;

        }

        public MovementResult Advance(int i)
        {
            bool result;
            if (opponent.position > position)
            {
                result = position + i >= opponent.position;
                position += i + (result ? 1 : 0);
            }
            else
            {
                result = position - i <= opponent.position;
                position -= i + (result ? 1 : 0);
            }
            return new MovementResult(true, i, result);
        }

        public MovementResult Retreat(int i)
        {
            if (opponent.position > position)
                position -= i;
            else
                position += i;
            return new MovementResult(false, i, false);
        }

        public MovementResult MoveSelf(int i)
        {
            if (i < 0)
            {
                return Retreat(-i);
            }

            return Advance(i);

        }


        public int GetPossibleAdvance()
        {
            if (opponent.position > position)
                return 6 - position;
            else
                return position - 2;
        }

        public int GetPossibleRetreat()
        {
            if (opponent.position > position)
                return position - 1;
            else
                return 7 - position;
        }


        internal void MoveOpponent(int i)
        {
            opponent.MoveSelf(i);
        }


        internal MovementResult UniversalMove(bool self, Direction direction, int loRange, int hiRange)
        {
            if (!self && opponent.ignoresAppliedMovement)
                return MovementResult.noMovement;


            List<int> moves = new List<int>(13);

            Player p = self ? this : opponent;

            int maxMoves;
            int maxPossible;
            int i;

            if (direction == Direction.Both || direction == Direction.Forward)
            {
                maxPossible = p.GetPossibleAdvance();

                maxMoves = Math.Min(maxPossible, hiRange);
                for (i = Math.Min(loRange, maxPossible); i <= maxMoves; i++)
                    moves.Add(i);
                
            }

            if (direction == Direction.Both || direction == Direction.Backward)
            {
                maxPossible = p.GetPossibleRetreat();

                
                maxMoves = Math.Min(maxPossible, hiRange);
                for (i = Math.Min(loRange, maxPossible); i <= maxMoves; i++)
                {
                    if (direction == Direction.Both && i == 0)
                        continue;
                    moves.Add(-i);
                }
                
            }

            if (moves.Count > 0)
            {
                int moveNumber;
                if (moves.Count == 1)
                    moveNumber = 0;
                else
                {
                    if (g.isMainGame && isHuman)
                    {
                        g.selectionHeader = "Select " + (self ? "your" : "opponent") + " movement:";
                        for (int j = 0; j < moves.Count; j++)
                            g.selectionItems.Add(movementText(moves[j], self));
                        g.getUserChoice();
                        moveNumber = g.selectionResult;
                    }
                    else
                        moveNumber = g.UCTSelect(moves.Count, this, true);

                    if (p.g.isMainGame)
                        p.g.registeredChoices.Add(moveNumber);
                }

                int movement = moves[moveNumber];

                MovementResult mr = p.MoveSelf(movement);

                if (g.isMainGame && mr.distance > 0)
                {
                    if (self)
                        g.writeToConsole(this + (mr.advance ? " advances " : " retreats ") + mr.distance);
                    else
                        g.writeToConsole(this + (mr.advance ? " pulls " : " pushes ") + opponent + ' ' + mr.distance);
                    g.flushConsole();
                }

                return mr;

            }

            return MovementResult.noMovement;


        }

        private string movementText(int p, bool self)
        {
            if (p == 0)
                return "Don't move";
            if (p > 0)
                return (self ? "Advance " : "Pull ") + p;
            else
                return (self ? "Retreat " : "Push ") + (-p);
        }


        public int priority()
        {
            return priorityModifier + attackBase.priority + attackStyle.priority;
        }


        public int loseLife(int pts)
        {
            int toLose = Math.Min(health - 1, pts);
            health -= toLose;
            return toLose;
        }


        internal void spendTokens(int tokens)
        {
            availableTokens -= tokens;
            usedTokens += tokens;
        }

        internal void drainLife(int p)
        {
            health += opponent.loseLife(p);

        }

        internal int rangeToOpponent()
        {
            int d = position - opponent.position;
            return d < 0 ? -d : d;
        }

        internal void gainTokens(int number)
        {
            int toGain = Math.Min(usedTokens, number);
            if (toGain > 0)
            {
                if (g.isMainGame)
                    g.writeToConsole(this + " gains " + toGain + " tokens.");
                availableTokens += toGain;
                usedTokens -= toGain;
            }
        }

        internal void selectAttackingPair()
        {
            int styleNumber = -1;
            int baseNumber = -1;
            

            // Select style

            // Supposedly here are always > 1 cards to choose from

            if (g.isMainGame)
            {
                if (isHuman)
                {
                    g.selectionHeader = "Select attacking style:";
                    g.selectionPlayer = this;
                    g.sss = SpecialSelectionStyle.Styles;
                    for (int j = 0; j < styles.Count; j++)
                        g.selectionItems.Add(styles[j].name);
                    g.getUserChoice();
                    styleNumber = g.selectionResult;
                }
                else
                {
                    g.writeToConsole(this + " is selecting the attack pair...");

                    AttackingPair ap = g.MCTS_attackingPair(this);

                    g.writeToConsole(this + " has selected the attack pair.");

                    styleNumber = ap.styleNumber;
                    baseNumber = ap.baseNumber;
                }

            }
            else
                styleNumber = g.UCTSelect(styles.Count, this, false);

            selectedStyle = styleNumber;
            //styles.RemoveAt(styleNumber);

            // Select base

            if (g.isMainGame)
            {

                if (isHuman)
                {
                    g.selectionHeader = "Select attacking base:";
                    g.sss = SpecialSelectionStyle.Bases;
                    for (int j = 0; j < bases.Count; j++)
                        g.selectionItems.Add(bases[j].name);
                    g.getUserChoice();
                    baseNumber = g.selectionResult;
                }
                // else it's AI and it's already selected the base, see above

            }
            else
                baseNumber = g.UCTSelect(bases.Count, this, false);

            selectedBase = baseNumber;
            //bases.RemoveAt(baseNumber);

            //if (g.isMainGame)
            //    g.writeToConsole(this + " selected " + attackStyle + ' ' + attackBase);
        }

        internal virtual bool ante()
        {
            if (availableTokens == 0)
            {
                if (g.isMainGame)
                    g.writeToConsole(this + " has no tokens.");
                return false;
            }

            int toAnte;

            if (g.isMainGame)
            {

                if (isHuman)
                {
                    g.selectionHeader = "Make your ante:";
                    for (int j = 0; j < availableTokens + 1; j++)
                        g.selectionItems.Add(j == 0 ? "Ante nothing" : "Ante " + j + " tokens");
                    g.getUserChoice();
                    toAnte = g.selectionResult;
                }
                else
                {
                    g.writeToConsole(this + " is selecting ante...");
                    toAnte = g.MCTS_ante(this);
                    // Honesty
                    
                    
                }
            }
            else
            {
                toAnte = g.UCTSelect(availableTokens + 1, this, false);

                if (g.pst == PlayoutStartType.AnteSelection)
                {
                    opponent.selectAttackingPair();
                    g.pst = PlayoutStartType.Normal;
                }
                                
            }

            if (toAnte > 0)
            {
                if (g.isMainGame)
                    g.writeToConsole(this + " antes " + toAnte + " tokens.");

                antedTokens += toAnte;
                availableTokens -= toAnte;
                usedTokens += toAnte;

                return true;

            }

            if (g.isMainGame)
                g.writeToConsole(this + " antes no tokens.");

            return false;
        }

        internal void AnteEffects()
        {
            c.AnteEffects(this);
        }

        internal void RevealEffects()
        {
            attackBase.Reveal(this);
        }

        internal void selectNextForClash()
        {
            

            int selected;

            if (bases.Count == 1)
                selected = 0;
            else
            {

                if (g.isMainGame)
                {
                    if (isHuman)
                    {
                        g.selectionHeader = "CLASH - select another base:";
                        g.sss = SpecialSelectionStyle.Bases;
                        for (int j = 0; j < bases.Count; j++)
                            g.selectionItems.Add(bases[j].name);
                        g.getUserChoice();
                        selected = g.selectionResult;
                    }
                    else
                    {
                        selected = g.MCTS_clash(this);
                    }
                }
                else
                {
                    selected = g.UCTSelect(bases.Count, this, false);
                }
                
            }

            selectedBase = selected;

            //clashPool.Add(attackBase);
            //attackBase = bases[selected];
            //bases.RemoveAt(selected);
        }

        internal void recycle()
        {
            // 1. Return from clash pool

            if (clashPool.Count > 0)
            {
                foreach (Card c in clashPool)
                    bases.Add(c);

                clashPool.Clear();
            }

            // 2. Outer

            if (CooldownBase2 != null)
                bases.Add(CooldownBase2);

            if (CooldownStyle2 != null)
                styles.Add(CooldownStyle2);

            // 3. Inner
            CooldownBase2 = CooldownBase1;
            CooldownStyle2 = CooldownStyle1;

            // 4. Attack pair
            CooldownBase1 = attackBase;
            CooldownStyle1 = attackStyle;

            attackBase = null;
            attackStyle = null;


        }

        internal void resolveStartOfBeat()
        {
            attackBase.StartOfBeat(this);
            attackStyle.StartOfBeat(this);
        }

        internal void attack(bool active)
        {
            if (isStunned)
                return;

            attackBase.BeforeActivating(this);
            attackStyle.BeforeActivating(this);


            // Check can hit
            attackStyle.checkCanHit(this);

            if (canHit && attackBase.lowRange > 0)
            {

                // Check opponent in range
                int dst = rangeToOpponent();

                if (dst >= attackBase.lowRange + attackStyle.lowRange
                    && dst <= attackBase.hiRange + attackStyle.hiRange)
                {
                    if (g.isMainGame)
                        g.writeToConsole(this + " hits.");

                    // Hit.
                    hasHit = true;
                    opponent.wasHit = true;

                    attackBase.OnHit(this);
                    attackStyle.OnHit(this);

                    int power = getTotalPower();

                    if (g.isMainGame)
                        g.writeToConsole(this + "'s power is " + power);

                    if (power > 0)
                    {
                        // Soak.
                        if (!attackStyle.ignoresSoak(this))
                        {
                            opponent.soakedDamage = Math.Min(power, opponent.soak);
                            if (opponent.soakedDamage > 0)
                            {
                                if (g.isMainGame)
                                    g.writeToConsole(opponent + " soaked " + opponent.soakedDamage);
                                
                                opponent.attackStyle.OnSoak(opponent);
                            }
                        }
                        else
                        {
                            if (g.isMainGame)
                                g.writeToConsole(this + "'s attack style ignores Soak.");
                        }
                        damageDealt = power - opponent.soakedDamage;
                        opponent.damageTaken = damageDealt;

                        if (damageDealt > 0)
                        {
                            if (g.isMainGame)
                                g.writeToConsole(this + " deals " + damageDealt + " damage to " + opponent + '.');


                            attackBase.OnDamage(this);
                            attackStyle.OnDamage(this);

                            if (!opponent.stunImmunity && (opponent.stunGuard < damageDealt || attackBase.ignoresStunGuard(this)))
                            {
                                
                                if (g.isMainGame && active && opponent.stunGuard > 0 && attackBase.ignoresStunGuard(this))
                                    g.writeToConsole(this + "'s " + attackBase + " ignores " + opponent + "'s Stun Guard of " + opponent.stunGuard + ".");

                                opponent.isStunned = true;
                            }
                            else
                            {
                                if (g.isMainGame && active)
                                {
                                    if (opponent.stunImmunity)
                                        g.writeToConsole(opponent + " has Stun Immunity.");
                                    if (opponent.stunGuard >= damageDealt)
                                        g.writeToConsole(opponent + "'s Stun Guard of " + opponent.stunGuard + " saves from being stunned.");

                                }
                                
                            }

                            opponent.health -= damageDealt;

                            if (opponent.health <= 0 && !isDead)
                            {
                                if (g.isMainGame)
                                    g.writeToConsole(opponent + " IS DEAD!");
                                opponent.isDead = true;
                            }
                        }

                    }

                }

                else
                {
                    if (g.isMainGame)
                        g.writeToConsole(this + " missed: opponent out of range.");
                }

            }

            else
            {
                if (g.isMainGame)
                    g.writeToConsole(this + "'s attack cannot hit.");
            }

            attackBase.AfterActivating(this);
            attackStyle.AfterActivating(this);
            
        }

        private int getTotalPower()
        {
            int basePower = attackBase.getAttackPower(this);

            if (basePower == 0)
                return 0;

            return basePower + attackStyle.power + powerModifier;
        }


        internal void resolveEndOfBeat()
        {
            attackBase.EndOfBeat(this);
            attackStyle.EndOfBeat(this);
        }

        internal void applyCommonProperties()
        {
            attackBase.CommonProperties(this);
            attackStyle.CommonProperties(this);
        }

        public override string ToString()
        {
            if (c == null)
                return "<no character>";
            else
                return c.name;
        }

        internal void returnAttackingPair()
        {
            styles.Add(attackStyle);
            bases.Add(attackBase);
            attackStyle = null;
            attackBase = null;
        }

        internal void selectNextForClash_MCTS()
        {
            selectNextForClash();

            g.pst = PlayoutStartType.Normal;

            

            // Honesty

            // Try to guess his base

            opponent.selectedBase = g.UCTSelect(opponent.bases.Count, opponent, false);
            
        }

        internal void revealAttack()
        {
            attackStyle = styles[selectedStyle];
            styles.RemoveAt(selectedStyle);

            attackBase = bases[selectedBase];
            bases.RemoveAt(selectedBase);
        }

        internal void revealClash()
        {
            clashPool.Add(attackBase);
            attackBase = bases[selectedBase];
            bases.RemoveAt(selectedBase);
        }
    }

}
