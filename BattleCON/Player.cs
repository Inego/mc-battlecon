﻿using System;
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

    public enum AnteResult
    {
        Pass,
        AntedTokens,
        AntedFinisher
    }
    


    public class Player
    {
        public int health;
        public bool cannotDie = false;
        public bool isDead = false;
        public int position;
        public Player opponent;

        public bool first;

        public int antedTokens;
        public int availableTokens;
        public int usedTokens;
        public bool cannotAnte = false; // long

        public int stunGuard;
        public bool stunGuardDisabled = false; // long
        public bool stunImmunity;
        public bool isStunned;
        public bool canHit;

        public bool ignoresAppliedMovement;

        public bool hasHit;
        public bool wasHit;
        public bool hitOpponentLastBeat;

        public bool canMoveNextBeat = true;
        public bool canMove;

        public bool soakDisabled = false; // long
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

        public int selectedCooldownBase1;
        public int selectedCooldownBase2;
        public int selectedCooldownStyle1;
        public int selectedCooldownStyle2;

        public int selectedFinisher;
        public Finisher finisher;
        public bool finisherPlayed = false;
        

        public GameState g;

        public Character c;

        public bool isHuman;


        public void fillFromPlayer(Player player)
        {
            health = player.health;
            cannotDie = player.cannotDie;
            isDead = player.isDead;
            position = player.position;
            first = player.first;
            antedTokens = player.antedTokens;
            availableTokens = player.availableTokens;
            usedTokens = player.usedTokens;
            cannotAnte = player.cannotAnte;
            stunGuard = player.stunGuard;
            stunGuardDisabled = player.stunGuardDisabled;
            stunImmunity = player.stunImmunity;
            isStunned = player.isStunned;
            canHit = player.canHit;
            ignoresAppliedMovement = player.ignoresAppliedMovement;
            hasHit = player.hasHit;
            wasHit = player.wasHit;
            hitOpponentLastBeat = player.hitOpponentLastBeat;
            soakedDamage = player.soakedDamage;
            soakDisabled = player.soakDisabled;
            damageDealt = player.damageDealt;
            damageTaken = player.damageTaken;
            soak = player.soak;
            powerModifier = player.powerModifier;
            priorityModifier = player.priorityModifier;
            nextBeatPowerModifier = player.nextBeatPowerModifier;

            canMoveNextBeat = player.canMoveNextBeat;
            canMove = player.canMove;
            
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

            finisher = player.finisher;
            finisherPlayed = player.finisherPlayed;

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
            this.selectedFinisher = -1;

            health = 20;
            this.c = c;
            
            this.position = position;
            this.hasHit = false;

            this.nextBeatPowerModifier = 0;

            // Six standard bases
            bases.Add(new Grasp());
            bases.Add(new Dash());
            bases.Add(new Burst());
            bases.Add(new Drive());
            bases.Add(new Strike());
            bases.Add(new Shot());

            c.init(this);

            resetBeat();
        }


        public void resetBeat()
        {

            stunGuard = 0;
            canHit = true;
            antedTokens = 0;
            soak = 0;

            cannotDie = false;

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

            canMove = canMoveNextBeat;

            if (g.isMainGame && !canMove)
                g.writeToConsole(this + " cannot move next beat.");

            canMoveNextBeat = true;

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


        internal MovementResult UniversalMove(bool self, Direction direction, int loRange, int hiRange)
        {
            if (!self && opponent.ignoresAppliedMovement)
                return MovementResult.noMovement;

            Player p = self ? this : opponent;

            if (!p.canMove)
                return MovementResult.noMovement;

            List<int> moves = new List<int>(13);

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
                        moveNumber = g.SimpleUCTSelect(moves.Count, this);

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
                styleNumber = g.ParallelUCTSelect(styles.Count, this);

            selectedStyle = styleNumber;

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
                baseNumber = g.ParallelUCTSelect(bases.Count, this);

            selectedBase = baseNumber;

        }

        internal virtual AnteResult ante()
        {
            if (cannotAnte)
            {
                if (g.isMainGame)
                    g.writeToConsole(this + " cannot ante.");
                return AnteResult.Pass;
            }


            bool canAnteFinisherFlag = canAnteFinisher();


            if (availableTokens == 0 && !canAnteFinisherFlag)
            {
                if (g.isMainGame)
                    g.writeToConsole(this + " has nothing to ante.");
                return AnteResult.Pass;
            }

            int toAnte;

            if (g.isMainGame)
            {

                if (isHuman)
                {
                    g.selectionHeader = "Make your ante:";
                    for (int j = 0; j < availableTokens + 1; j++)
                        g.selectionItems.Add(j == 0 ? "Ante nothing" : "Ante " + j + " tokens");
                    if (canAnteFinisherFlag)
                        g.selectionItems.Add(finisher.ToString() + " (Finisher)");
                    g.getUserChoice();
                    toAnte = g.selectionResult;
                }
                else
                {
                    g.writeToConsole(this + " is selecting ante...");
                    toAnte = g.MCTS_ante(this);
                }
            }
            else
            {

                toAnte = g.ParallelUCTSelectWithCloning(availableTokens + (canAnteFinisherFlag ? 2 : 1), this);

                if (g.pst == PlayoutStartType.AnteSelection)
                {
                    opponent.selectAttackingPair();
                    g.pst = PlayoutStartType.Normal;
                }
                                
            }

            if (toAnte > 0)
            {
                if (g.isMainGame)
                {
                    g.writeToConsole(this + " antes " + (toAnte == availableTokens + 1 ? finisher.ToString() : (toAnte + " tokens.")));
                }

                if (toAnte == availableTokens + 1) // FINISHER
                {
                    // Base and style go back
                    attackStyle = StyleCard.blank;
                    attackBase = finisher;
                    finisherPlayed = true;
                    return AnteResult.AntedFinisher;
                }
                
                antedTokens += toAnte;
                availableTokens -= toAnte;
                usedTokens += toAnte;

                return AnteResult.AntedTokens;

            }

            if (g.isMainGame)
                g.writeToConsole(this + " antes no tokens.");

            return AnteResult.Pass;
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
                    selected = g.moveManager.ParallelSelect(bases.Count, this);
                }
                
            }

            selectedBase = selected;

        }

        internal void recycle()
        {
            if (attackBase is Finisher)
            {
                attackBase = null;
                attackStyle = null;
                return;
            }


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
                        if (opponent.soak > 0)
                        {
                            if (!(attackStyle.ignoresOpponentSoak(this) || opponent.soakDisabled))
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
                                opponent.soakedDamage = 0;

                                if (g.isMainGame)
                                {
                                    if (attackStyle.ignoresOpponentSoak(this))
                                        g.writeToConsole(this + "'s attack style ignores Soak.");
                                    else if (opponent.soakDisabled)
                                        g.writeToConsole(opponent + "'s Soak is disabled.");

                                }
                            }

                            damageDealt = power - opponent.soakedDamage;
                        }
                        else
                        {
                            damageDealt = power;
                        }
                        
                        
                        opponent.damageTaken = damageDealt;

                        if (damageDealt > 0)
                        {

                            if (g.isMainGame)
                                g.writeToConsole(this + " deals " + damageDealt + " damage to " + opponent + '.');


                            attackBase.OnDamage(this);
                            attackStyle.OnDamage(this);


                            opponent.c.OnDamageTaken(opponent);
                            c.OnDamage(this);

                            if (!opponent.stunImmunity && (opponent.stunGuard < damageDealt || attackBase.ignoresStunGuard(this) || opponent.stunGuardDisabled))
                            {

                                if (g.isMainGame && active && opponent.stunGuard > 0)
                                {
                                    if (attackBase.ignoresStunGuard(this))
                                        g.writeToConsole(this + "'s " + attackBase + " ignores " + opponent + "'s Stun Guard of " + opponent.stunGuard + ".");
                                    else if (opponent.stunGuardDisabled)
                                        g.writeToConsole(opponent + "'s Stun Guard is disabled.");
                                }

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

                            if (opponent.cannotDie && opponent.health < 1)
                                opponent.health = 1;

                            if (opponent.health <= 0 && !isDead)
                            {
                                if (g.isMainGame)
                                    g.writeToConsole(opponent + " IS DEAD!");
                                opponent.isDead = true;
                                return;
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
            int myBase = selectedBase;

            g.moveManager.ParallelInitialize();

            // Before selecting clash, emulate parallel base selection
            selectedBase = g.moveManager.ParallelSelect(bases.Count, this);
            opponent.selectedBase = g.moveManager.ParallelSelect(opponent.bases.Count, opponent);

            selectNextForClash();

            g.pst = PlayoutStartType.Normal;

            throw new NotImplementedException();
            
            

            
            
        }

        internal void revealAttack()
        {

            if (attackBase is Finisher)
                return;

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

        internal bool canAnteFinisher()
        {
            return (g.variant == GameVariant.AnteFinishers && health <= 7 && !finisherPlayed);
        }

        internal void makeSetupDecisions()
        {

            if (g.isMainGame)
            {
                if (isHuman)
                {
                    g.selectionPlayer = this;

                    g.selectionHeader = "Select the 2nd Discard Style:";
                    g.sss = SpecialSelectionStyle.Styles;
                    for (int i = 0; i < 5; i++)
                        g.selectionItems.Add(styles[i].ToString());
                    g.getUserChoice();
                    selectedCooldownStyle2 = g.selectionResult;

                    g.selectionHeader = "Select the 1st Discard Style:";
                    g.sss = SpecialSelectionStyle.Styles;
                    for (int i = 0; i < 5; i++)
                        if (i != selectedCooldownStyle2)
                            g.selectionItems.Add(styles[i].ToString());
                    g.getUserChoice();
                    selectedCooldownStyle1 = g.selectionResult;


                    g.selectionHeader = "Select the 2nd Discard Base:";
                    g.sss = SpecialSelectionStyle.Bases;
                    for (int i = 0; i < 7; i++)
                        g.selectionItems.Add(bases[i].ToString());
                    g.getUserChoice();
                    selectedCooldownBase2 = g.selectionResult;

                    g.selectionHeader = "Select the 1st Discard Base:";
                    g.sss = SpecialSelectionStyle.Bases;
                    for (int i = 0; i < 7; i++)
                        if (i != selectedCooldownBase2)
                            g.selectionItems.Add(bases[i].ToString());
                    g.getUserChoice();
                    selectedCooldownBase1 = g.selectionResult;


                    g.selectionHeader = "Select the Finisher:";

                    g.sss = SpecialSelectionStyle.Finishers;

                    g.selectionItems.Add(c.finisher1.ToString());
                    g.selectionItems.Add(c.finisher2.ToString());
                    g.getUserChoice();
                    selectedFinisher = g.selectionResult;
                }
                else
                {
                    g.writeToConsole(this + " is selecting setup cards...");
                    g.MCTS_selectSetupCards(this);
                    g.writeToConsole(this + " has selected setup cards.");

                }

            }
            else
            {
                selectedCooldownStyle2 = g.moveManager.ParallelSelect(5, this);
                selectedCooldownStyle1 = g.moveManager.ParallelSelect(4, this);
                selectedCooldownBase2 = g.moveManager.ParallelSelect(7, this);
                selectedCooldownBase1 = g.moveManager.ParallelSelect(6, this);
                selectedFinisher = g.moveManager.ParallelSelect(2, this);
            }

            
        }

        internal void applySetupDecisions()
        {
            CooldownStyle2 = styles[selectedCooldownStyle2];
            styles.RemoveAt(selectedCooldownStyle2);
            
            CooldownStyle1 = styles[selectedCooldownStyle1];
            styles.RemoveAt(selectedCooldownStyle1);

            CooldownBase2 = bases[selectedCooldownBase2];
            bases.RemoveAt(selectedCooldownBase2);

            CooldownBase1 = bases[selectedCooldownBase1];
            bases.RemoveAt(selectedCooldownBase1);


            finisher = selectedFinisher == 0 ? c.finisher1 : c.finisher2;
        }
    }

}
