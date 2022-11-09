using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;

public interface ISchetsTool
{
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void MuisLos(SchetsControl s, Point p);
    void Letter(SchetsControl s, char c);
}

// virtuals vd abstact maken omdat dan geen junk nodig is bij de subklassen
public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {   kwast = new SolidBrush(s.PenKleur);
        s.Schets.saveStatus = false; // toegevoegd2
    }
    public virtual void MuisDrag(SchetsControl s, Point p) { }
    public virtual void Letter(SchetsControl s, char c) { }
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = 
            gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString   (tekst, font, kwast, 
                                            this.startpunt, StringFormat.GenericTypographic);
            // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
            startpunt.X += (int)sz.Width;
            s.Invalidate();
        }
    }
}

public abstract class TweepuntTool : StartpuntTool
{
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                            , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                            );
    }
    public static Pen MaakPen(Brush b, int dikte)
    {   Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {   base.MuisVast(s, p);
        kwast = Brushes.Gray;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {   s.Refresh();
        this.Bezig(s.CreateGraphics(), this.startpunt, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {   base.MuisLos(s, p);
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    //zet bezig naar virtual
    public virtual void Bezig(Graphics g, Point p1, Point p2) { }
        
    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {   this.Bezig(g, p1, p2);
    }
}

// Rechthoektypen en lijn tekenen is nu object maken en toevoegen aan lijst die in klasse Schets getekend wordt.
// Bij alles worden oude override Bezig weggehaald worden
public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
        RechthoekObject rechthoekO = new RechthoekObject(this.startpunt, p, kwast);
        s.Schets.ObjectenLijst.Add(rechthoekO);
        s.Invalidate();
    }
}
    
public class VolRechthoekTool : RechthoekTool
{
    public override string ToString() { return "vlak"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
        RechthoekObjectVol rechthoekO = new RechthoekObjectVol(this.startpunt, p, kwast);
        s.Schets.ObjectenLijst.Add(rechthoekO);
        s.Invalidate();
    }
}

public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
        LijnObject lijnO = new LijnObject(this.startpunt, p, kwast);
        s.Schets.ObjectenLijst.Add(lijnO);
        s.Invalidate();
    }
}

// Pen is nu kleine stukjes lijn met Muislos this.
public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {   this.MuisLos(s, p);
        this.MuisVast(s, p);
    }
}
   
// Aanpassen: gum is geen subklasse van PenTool meer want het werkt volledig anders. muisdrag en letter niet aanwezig. wel als geklikt check welke er is aangeklikt.

public class GumTool : StartpuntTool
{
    public override string ToString() { return "gum"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
    }

    public override void MuisVast(SchetsControl s, Point p)
    {
        for (int i = s.Schets.ObjectenLijst.Count - 1; i >= 0; i--)
        {
            if (s.Schets.ObjectenLijst[i].BenIkGeklikt(p))
            {
                s.Schets.ObjectenLijst.RemoveAt(i);
                s.Invalidate();
                break;
            }
        }
    }
}

// Nieuwe ovaaltools toevoegen
public class OvaalTool : TweepuntTool
{
    public override string ToString() { return "ovaalkader"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
        OvaalObject ovaal0 = new OvaalObject(this.startpunt, p, kwast);
        s.Schets.ObjectenLijst.Add(ovaal0);
        s.Invalidate();
    }
}

public class VolOvaalTool : OvaalTool
{
    public override string ToString() { return "ovaal"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }

    public override void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
        OvaalObjectVol ovaal0 = new OvaalObjectVol(this.startpunt, p, kwast);
        s.Schets.ObjectenLijst.Add(ovaal0);
        s.Invalidate();
    }
}