using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleCON
{

    public abstract class SomethingOnScreen
    {
        public static Font regionCaptionFont = new Font("Tahoma", 16, FontStyle.Bold);
        public static Font regionTinyCaptionFont = new Font(FontFamily.GenericMonospace, 8);
        public static Brush alphaWhite = new SolidBrush(Color.FromArgb(220, Color.White));

        public int x1;
        public int y1;
        public int width;
        public int height;

        public SomethingOnScreen(int x1, int y1, int width, int height)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.width = width;
            this.height = height;
        }

        internal bool hasMouse(int mouseX, int mouseY)
        {
            return (mouseX >= x1 && mouseX <= x1 + width - 1
                && mouseY >= y1 && mouseY <= y1 + height - 1);
        }

        internal abstract void highlight(BattleBoard bb);
    }


    public class CharacterOnScreen : SomethingOnScreen
    {
        public Character c;

        public static int characterCardWidth = 300;
        public static int characterCardHeight = 250;

        public static Brush characterCardBkBrush = new SolidBrush(Color.FromArgb(240, Color.PaleGoldenrod));

        public CharacterOnScreen(int x1, int y1, int width, int height, Character c) : base(x1, y1, width, height)
        {
            this.c = c;
        }

        internal override void highlight(BattleBoard bb)
        {

            int x = bb.mouseX;
            int y = bb.mouseY;

            if (x + characterCardWidth > bb.Width)
                x = bb.Width - characterCardWidth;

            if (y + characterCardHeight > bb.Height)
                y = bb.Height - characterCardHeight;

            Graphics g = bb.drawingGraphics2;

            g.FillRectangle(characterCardBkBrush, x, y, characterCardWidth, characterCardHeight);

            TextRenderer.DrawText(g, c.getDescription(), SystemFonts.SmallCaptionFont, new Rectangle(x + 5, y + 5, characterCardWidth - 10, characterCardHeight - 10), Color.Black, /*TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |*/ TextFormatFlags.WordBreak);
            
        }

    }


    public class CardOnScreen : SomethingOnScreen
    {
        public Card c;

        public bool highlightable;

        private static int screenCardWidth = 250;
        private static int screenCardHeight = 280;

        private static Brush rangeBrush    = new SolidBrush(Color.FromArgb(150, Color.LightSeaGreen));
        private static Brush powerBrush    = new SolidBrush(Color.FromArgb(150, Color.Red));
        private static Brush priorityBrush = new SolidBrush(Color.FromArgb(150, Color.Orange));

        public CardOnScreen(int x1, int y1, int width, int height, Card c, bool highlightable) : base(x1, y1, width, height)
        {
            this.c = c;
            this.highlightable = highlightable;
        }

        static int stripeOffset = 30;

        internal void drawStripe(Graphics g, string caption, int x, int y, Brush b, string value)
        {
            g.DrawString(caption, regionTinyCaptionFont, Brushes.Black, x + 5, y + stripeOffset);
            g.FillRectangle(b, x, y + stripeOffset + 15, screenCardWidth / 2, 22);
            g.DrawString(value, regionCaptionFont, Brushes.Black, x + 5, y + stripeOffset + 13);
        }



        internal override void highlight(BattleBoard bb)
        {
            int x = bb.mouseX;
            int y = bb.mouseY;

            if (x + screenCardWidth > bb.Width)
                x = bb.Width - screenCardWidth;

            if (y + screenCardHeight > bb.Height)
                y = bb.Height - screenCardHeight;

            Graphics g = bb.drawingGraphics2;


            if (highlightable)
                g.DrawRectangle(Pens.Green, x1 - 2, y1 - 2, width + 4, height + 4);

            g.FillRectangle(alphaWhite, x, y, screenCardWidth, screenCardHeight);
            
            g.DrawString(c.name, regionCaptionFont, Brushes.Black, x + 5, y + 5);
            g.DrawLine(Pens.Black, x, y + 30, x + screenCardWidth, y + 30);

            drawStripe(g, "range",    x, y,       rangeBrush,    c.getRangeText());
            drawStripe(g, "power",    x, y + 40,  powerBrush,    c.getPowerText());
            drawStripe(g, "priority", x, y + 80, priorityBrush, c.getPriorityText());

            

            int descYBegin = 160;

            Rectangle descRect = new Rectangle(x + 5, y + descYBegin, screenCardWidth - 10, screenCardHeight - descYBegin - 5);


            TextRenderer.DrawText(g, c.getDescription(), SystemFonts.SmallCaptionFont, descRect, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            
            

            
        }
    }


    public partial class BattleBoard : UserControl
    {
        private bool initializationComplete;
        private bool isDisposing;
        private BufferedGraphicsContext backbufferContext;
        private BufferedGraphics backbufferGraphics;
        private Graphics drawingGraphics;

        private BufferedGraphics backbufferGraphics2;
        public Graphics drawingGraphics2;

        public List<SomethingOnScreen> regionsOnScreen = new List<SomethingOnScreen>();

        public GameState gs = null;


        public BattleBoard()
        {
            InitializeComponent();

            // Set the control style to double buffer.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // Assign our buffer context.
            backbufferContext = BufferedGraphicsManager.Current;
            initializationComplete = true;

            RecreateBuffers();

            Redraw(true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecreateBuffers();
            Redraw(true);
        }


        private void RecreateBuffers()
        {
            // Check initialization has completed so we know backbufferContext has been assigned.
            // Check that we aren't disposing or this could be invalid.
            if (!initializationComplete || isDisposing)
                return;

            // We recreate the buffer with a width and height of the control. The "+ 1" 
            // guarantees we never have a buffer with a width or height of 0. 
            backbufferContext.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            // Dispose of old backbufferGraphics (if one has been created already)
            if (backbufferGraphics != null)
            {
                backbufferGraphics.Dispose();
                backbufferGraphics2.Dispose();
            }

            // Create new backbufferGrpahics that matches the current size of buffer.
            backbufferGraphics = backbufferContext.Allocate(this.CreateGraphics(),
                new Rectangle(0, 0, Math.Max(this.Width, 1), Math.Max(this.Height, 1)));

            backbufferGraphics2 = backbufferContext.Allocate(this.CreateGraphics(),
                new Rectangle(0, 0, Math.Max(this.Width, 1), Math.Max(this.Height, 1)));

            // Assign the Graphics object on backbufferGraphics to "drawingGraphics" for easy reference elsewhere.
            drawingGraphics = backbufferGraphics.Graphics;
            drawingGraphics2 = backbufferGraphics2.Graphics;

            // This is a good place to assign drawingGraphics.SmoothingMode if you want a better anti-aliasing technique.

            // Invalidate the control so a repaint gets called somewhere down the line.
            this.Invalidate();
        }

        public const int playerPanelHeight = 150;
        public const int playerPanelWidth = 550;
        public const int panelsVertSpacing = 10;
        public const int battleSpaceHeight = 150;
        public const int battlespaceVertOffset = 40;
        public const int spcSize = 60;
        public const int spcSpacing = 10;
        public const int cardWidth = 80;
        public const int cardHeight= 17;
        public const int cardSpacing= 5;
        public const int cooldownOffset = 50;



        private Font playerFont = new Font(FontFamily.GenericSerif, 32);
        private Font boldFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        
        public int mouseX = -1;
        public int mouseY = -1;
        public SomethingOnScreen currentRegion = null;


        private void DrawPlayer(Player p, int y)
        {
            drawingGraphics.DrawRectangle(Pens.Black, 0, y, playerPanelWidth, playerPanelHeight);

            drawingGraphics.DrawString(p.ToString(), SystemFonts.CaptionFont, p.first ? Brushes.Blue : Brushes.Red, 5, y + 5);

            regionsOnScreen.Add(new CharacterOnScreen(5, y + 5, 50, 15, p.c));


            drawingGraphics.DrawString(p.health.ToString(), boldFont, Brushes.Black, 5, y + 20);

            if (p.antedTokens > 0)
                drawingGraphics.DrawString("Anted " + p.antedTokens.ToString(), SystemFonts.CaptionFont, Brushes.Black, 5, y + 40);

            drawingGraphics.DrawString("Tokens " + p.availableTokens.ToString() + '/' + p.usedTokens.ToString(), SystemFonts.CaptionFont, Brushes.Black, 5, y + 55);

            bool highlightToSelect = (p == p.g.selectionPlayer && p.g.sss == SpecialSelectionStyle.Styles);

            for (int i = 1; i <= p.styles.Count; i++)
                drawCard(p.styles[i - 1], 100 + (i - 1) * (cardWidth + cardSpacing), y + 10, CardBorderStyle.available, highlightToSelect);

            highlightToSelect = (p == p.g.selectionPlayer && p.g.sss == SpecialSelectionStyle.Bases);

            for (int i = 1; i <= p.bases.Count; i++)
                drawCard(p.bases[i - 1], 100 + (i - 1) * (cardWidth + cardSpacing), y + 40, CardBorderStyle.available, highlightToSelect);

            // Cooldown 2
            drawCard(p.CooldownStyle2, cooldownOffset, y + 80, CardBorderStyle.cooldown, false);
            drawCard(p.CooldownBase2, cooldownOffset, y + 100, CardBorderStyle.cooldown, false);

            // Cooldown 1
            drawCard(p.CooldownStyle1, cooldownOffset + cardSpacing + cardWidth, y + 80, CardBorderStyle.cooldown, false);
            drawCard(p.CooldownBase1, cooldownOffset + cardSpacing + cardWidth, y + 100, CardBorderStyle.cooldown, false);

            // Finisher
            drawCard(p.finisher, cooldownOffset + 200, y + 80, CardBorderStyle.finisher, false);


        }

        public enum CardBorderStyle
        {
            available,
            cooldown,
            player1,
            player2,
            finisher
        }

        private void drawCard(Card card, int x, int y, CardBorderStyle cbs, bool highlightToSelect)
        {

            if (card == null)
                return;

            Pen p = null;
            Brush b = null;

            switch (cbs)
            {
                case CardBorderStyle.available:
                    p = Pens.Black;
                    b = Brushes.Black;
                    break;
                case CardBorderStyle.cooldown:
                    p = Pens.Gray;
                    b = Brushes.Gray;
                    break;
                case CardBorderStyle.player1:
                    p = Pens.Blue;
                    b = Brushes.Blue;
                    break;
                case CardBorderStyle.player2:
                    p = Pens.Red;
                    b = Brushes.Red;
                    break;
                case CardBorderStyle.finisher:
                    p = Pens.Blue;
                    b = Brushes.Orange;
                    break;

            }

            if (highlightToSelect)
                drawingGraphics.FillRectangle(Brushes.PaleGreen, x, y, cardWidth, cardHeight);

            drawingGraphics.DrawRectangle(p, x, y, cardWidth, cardHeight);
            drawingGraphics.DrawString(card.name, SystemFonts.CaptionFont, b, x + 5, y);

            regionsOnScreen.Add(new CardOnScreen(x, y, cardWidth, cardHeight, card, highlightToSelect));

        }


        private void DrawPlayerPosition(int y, Player p)
        {

            int i = p.position;
            Brush b = p.first ? Brushes.Blue : Brushes.Red;
            drawingGraphics.DrawString(p.first ? "1" : "2", playerFont, b, spcSpacing + (i - 1) * (spcSize + spcSpacing), y + battlespaceVertOffset);
            drawingGraphics.DrawString(p.c.name, SystemFonts.SmallCaptionFont, b, spcSpacing + (i - 1) * (spcSize + spcSpacing), y + battlespaceVertOffset + 40);
            
        }

        private void DrawBattleSpace(int y)
        {
            drawingGraphics.DrawRectangle(Pens.Black, 0, y, playerPanelWidth, battleSpaceHeight);
            drawingGraphics.DrawString("BEAT " + gs.beat, SystemFonts.CaptionFont, Brushes.Black, 5, y+ 5);

            for (int i = 1; i <= 7; i++)
            {
                drawingGraphics.DrawRectangle(Pens.DarkKhaki, spcSpacing + (i - 1) * (spcSize + spcSpacing), y + battlespaceVertOffset, spcSize, spcSize);
            }

            DrawPlayerPosition(y, gs.p1);
            DrawPlayerPosition(y, gs.p2);

            // Attack pairs
            DrawPlayerAttackPair(gs.p2, y + 10);
            DrawPlayerAttackPair(gs.p1, y + battleSpaceHeight - cardHeight - 10);

        }

        private void DrawPlayerAttackPair(Player player, int y)
        {
            CardBorderStyle cbs = player.first ? CardBorderStyle.player1 : CardBorderStyle.player2;
            drawCard(player.attackStyle, 200, y, cbs, false);
            drawCard(player.attackBase, 200 + cardSpacing + cardWidth, y, cbs, false);
        }


        public void Redraw(bool forceRefresh)
        {
            drawingGraphics.Clear(SystemColors.Control);

            regionsOnScreen.Clear();

            if (gs != null)
            {
                DrawPlayer(gs.p2, 0);
                DrawBattleSpace(playerPanelHeight + panelsVertSpacing);
                DrawPlayer(gs.p1, playerPanelHeight + battleSpaceHeight + 2 * panelsVertSpacing);

            }

            backbufferGraphics.Render(drawingGraphics2);

            // Force the control to both invalidate and update. 
            if (forceRefresh)
            {
                this.Refresh();
            }
        }

        

        protected override void OnPaint(PaintEventArgs pe)
        {
            // If we've initialized the backbuffer properly, render it on the control. 
            // Otherwise, do just the standard control paint.
            if (!isDisposing && backbufferGraphics2 != null)
                backbufferGraphics2.Render(pe.Graphics);
        }

        internal void checkMouseMove(int mouseX, int mouseY)
        {
            this.mouseX = mouseX;
            this.mouseY = mouseY;

            //Pen p = null;

            if (checkMouseInRegionOnScreen())
            {
                RefreshUpperLayer();
                //p = Pens.Red;
            }
            else
            {
                //p = currentRegion == null ? Pens.Black : Pens.Blue;
                if (currentRegion != null)
                    RefreshUpperLayer();

                    
            }

            //drawingGraphics2.DrawEllipse(p, mouseX - 10, mouseY - 10, 20, 20);

            
            
        }

        private void RefreshUpperLayer()
        {
            backbufferGraphics.Render(drawingGraphics2);

            if (currentRegion != null)
                currentRegion.highlight(this);

            Refresh();
        }

        private bool checkMouseInRegionOnScreen()
        {
            SomethingOnScreen newRegion = null;
            foreach (SomethingOnScreen s in regionsOnScreen)
            {
                if (s.hasMouse(mouseX, mouseY))
                {
                    newRegion = s;
                    break;
                }
            }

            if (newRegion != currentRegion)
            {
                currentRegion = newRegion;
                return true;
            }

            return false;
        }
    }
}
