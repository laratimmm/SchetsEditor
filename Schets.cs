using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

public class Schets
{
    public Bitmap bitmap;
    public bool saveStatus = false;//Toegevoegd2

    public List<TekenObject> ObjectenLijst = new List<TekenObject>();
    public Schets()
    {
        bitmap = new Bitmap(1, 1);
    }
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap(Math.Max(sz.Width, bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
        for (int i = 0; i < ObjectenLijst.Count; i++)
        {
            ObjectenLijst[i].TekenZelf(gr);
        }
    }
    // Schoon maakt nu de lijst leeg
    public void Schoon()
    {
        ObjectenLijst.Clear();
    }

    // Moet nog aangepast worden. miss per object?
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }

    public void ThisSave(string format = "jpg")
    {
        if (format == "jpg")
            bitmap.Save("img.jpg", ImageFormat.Jpeg);
        if (format == "png")
            bitmap.Save("img.png", ImageFormat.Png);
        if (format == "bmp")
            bitmap.Save("img.bmp", ImageFormat.Bmp);
    }
}

// Vanaf hier nieuwe subklassenstructuur voor alle getekende objecten
public class TekenObject
{
    public Point p1;
    public Point p2;
    public Pen pen;
    public int randdikte;

    public virtual void TekenZelf(Graphics g)
    {
        g.DrawRectangle(pen, TweepuntTool.Punten2Rechthoek(p1, p2));
    }

    public virtual bool BenIkGeklikt(Point p)
    {
        return false;
    }
}

public class RechthoekObject : TekenObject
{
    public RechthoekObject(Point p1, Point p2, Brush kwast)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.pen = TweepuntTool.MaakPen(kwast, 3);
    }

    public override void TekenZelf(Graphics g)
    { 
        g.DrawRectangle(pen, TweepuntTool.Punten2Rechthoek(p1, p2));
    }

    // groot rechthoek - klein rechthoek = rand, maar alleen als de rechthoek niet even groot is als de randdikte
    public override bool BenIkGeklikt(Point p)
    {
        if ((((p1.X <= p.X) && (p.X <= p2.X)) || ((p1.X >= p.X) && (p.X >= p2.X))) && (((p1.Y <= p.Y) && (p.Y <= p2.Y)) || ((p1.Y >= p.Y) && (p.Y >= p2.Y))))
            if ((Math.Abs(p1.X - p2.X) > randdikte) && (((p1.X <= p.X) && (p.X <= p2.X)) || ((p1.X >= p.X) && (p.X >= p2.X))))
                return false;
            else
                return true;
        else
            return false;
    }
}

public class RechthoekObjectVol : RechthoekObject
{
    public Brush kwast;

    public RechthoekObjectVol(Point p1, Point p2, Brush kwast) : base(p1,p2,kwast)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.kwast = kwast;
    }

    public override void TekenZelf(Graphics g)
    {
        g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }

    public override bool BenIkGeklikt(Point p)
    {
        if ((((p1.X <= p.X) && (p.X <= p2.X)) || ((p1.X >= p.X) && (p.X >= p2.X))) && (((p1.Y <= p.Y) && (p.Y <= p2.Y)) || ((p1.Y >= p.Y) && (p.Y >= p2.Y))))
            return true;
        else
            return false;
    }
}

public class OvaalObject : TekenObject
{
    public OvaalObject(Point p1, Point p2, Brush kwast)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.pen = TweepuntTool.MaakPen(kwast, 3);
    }

    public override void TekenZelf(Graphics g)
    {
        g.DrawEllipse(pen, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class OvaalObjectVol : OvaalObject
{
    public Brush kwast;

    public OvaalObjectVol(Point p1, Point p2, Brush kwast) : base(p1, p2, kwast)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.kwast = kwast;
    }

    public override void TekenZelf(Graphics g)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class LijnObject : TekenObject
{
    public LijnObject(Point p1, Point p2, Brush kwast)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.pen = TweepuntTool.MaakPen(kwast, 3);
    }

    public override void TekenZelf(Graphics g)
    {
        g.DrawLine(pen, p1, p2);
    }
}