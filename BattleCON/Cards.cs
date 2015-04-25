using System.Collections.Generic;
namespace BattleCON
{

    public enum CardType
    {
        baseCard,
        styleCard,
        finisher
    }


    public delegate void TriggerHandler();


    public class NamedHandler
    {
        public string name;
        public TriggerHandler handler;

        public NamedHandler(string name, TriggerHandler handler)
        {
            this.name = name;
            this.handler = handler;
        }
    }


    public abstract class Card
    {
        internal string name;

        public int lowRange = 0;
        public int hiRange = 0;
        public int power = 0;
        public int priority = 0;

        public CardType type;

        internal void addHandler(List<NamedHandler> list, TriggerHandler handler) {
            list.Add(new NamedHandler(name, handler));
        }

        virtual public void CommonProperties(Player p)
        {

        }

        virtual public void Reveal(Player p)
        {

        }

        virtual public void StartOfBeat(Player p)
        {

        }

        virtual public void BeforeActivating(Player p, List<NamedHandler> handlers)
        {

        }


        virtual public void OnHit(Player p, List<NamedHandler> handlers)
        {

        }

        virtual public void OnSoak(Player p)
        {
        }

        virtual public void OnDamage(Player p)
        {

        }

        virtual public void AfterActivating(Player p, List<NamedHandler> handlers)
        {

        }

        virtual public void EndOfBeat(Player p)
        {

        }

        virtual public int getAttackPower(Player p)
        {
            return this.power;
        }

        virtual public bool ignoresStunGuard(Player p)
        {
            return false;
        }

        virtual public bool ignoresOpponentSoak(Player p)
        {
            return false;
        }

        virtual public void checkCanHit(Player p)
        {
        }

        public override string ToString()
        {
            return name;
        }

        public virtual string getRangeText()
        {
            string result;
            if (lowRange == 0 && hiRange == 0)
                return "-";
            else if (lowRange == hiRange)
                result = lowRange.ToString();
            else
                result = lowRange.ToString() + "-" + hiRange.ToString();

            return (type == CardType.styleCard ? "+" : "") + result;
        }


        public virtual string getPowerText()
        {
            return (type == CardType.styleCard && power > 0 ? "+" : "") + (power != 0 ? power.ToString() : "-");
        }

        public string getPriorityText()
        {
            return (type == CardType.styleCard && priority > 0 ? "+" : "") + (priority != 0 ? priority.ToString() : "-");
        }



        internal abstract string getDescription();
        
    }


    public class BaseCard : Card
    {
        public BaseCard()
        {
            type = CardType.baseCard;
        }

        internal override string getDescription()
        {
            return "A Base card.";
        }

    }

    public class StyleCard : Card
    {

        public static StyleCard blank = new StyleCard();


        public StyleCard()
        {
            type = CardType.styleCard;
        }

        internal override string getDescription()
        {
            return "A Style card.";
        }

    }


    public class Finisher : Card
    {
        public Finisher()
        {
            type = CardType.finisher;
        }

        internal override string getDescription()
        {
            return "A Finisher.";
        }

    }

    


}