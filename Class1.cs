using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsFormsApp1
{
    [Serializable]
    public class Circle
    {
        public Rectangle rect;
        public bool isVisible;
        public bool isLinked;
        public bool isOut;
        public bool isIn;

    }

    [Serializable]
    public class Connection
    {
        public Point from;
        public Point to;

        public Block fromBlock;
        public Block toBlock;

        public Circle fromCircle;
        public Circle toCircle;

        public Connection( Point from, Point to, Block fromBlock, Block toBlock, Circle fromCircle, Circle toCircle)
        {
            this.from = from;
            this.to = to;
            this.fromBlock = fromBlock;
            this.toBlock = toBlock;
            this.fromCircle = fromCircle;
            this.toCircle = toCircle;
                
        }

        


    }
    
    [Serializable]
    public abstract class Block
    {

        virtual public bool IsTextEnabled { get; }

        public string text = "";
        public Point center;
        protected int circleRadius = 5;
        public List<Circle> outCircles = new List<Circle>();
        public List<Circle> inCircles = new List<Circle>();

        protected int operationBlockWidth = 100;
        protected int operationBlockHeight = 70;
        protected int decisionBlockSize = 100;
        protected int StartStopBlockRadius = 40;

        public List<Connection> parents = new List<Connection>();
        public List<Connection> childs = new List<Connection>();


        public Block(  Point center)
        {
           
            
            this.center = center;

        }

        public abstract void Draw(PictureBox pictureBox, DashStyle dashStyle = DashStyle.Custom);
        public abstract void Erase(PictureBox pictureBox);
        public abstract bool IsVisible(Point point);
        public abstract void SetNewLocation(Point point);
        public abstract int GetWidth();
        public abstract int GetHeight();
        

        protected void DrawingCircle(PictureBox pictureBox, Color penColor, Brush brush, Circle circle)
        {
            Rectangle rect = circle.rect;

            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {

                flagGraphics.FillEllipse(brush, rect.X, rect.Y, rect.Height, rect.Width);


                //drawing rectangle
                using (Pen pen = new Pen(penColor, 2))
                {
                    pen.Alignment = PenAlignment.Inset;

                    flagGraphics.DrawEllipse(pen, rect.X, rect.Y, rect.Height, rect.Width);
                }

            }
            pictureBox.Invalidate();
        }

    }

    [Serializable]
    public class OperationBlock : Block
    {
        private Circle inCircle;
        private Circle outCircle;
        public Rectangle rect;

        public override bool IsTextEnabled { get; } = true;

        public OperationBlock(Rectangle rect, BlockType blockType, Point center) : base( center)
        {
            inCircle = new Circle();
            outCircle = new Circle();
            this.inCircles.Add(inCircle);
            this.outCircles.Add(outCircle);
            inCircle.isVisible = true;
            outCircle.isVisible = true;
            this.rect = rect;
           
        }

        public override int GetHeight()
        {
            return this.rect.Height;
        }
        public override int GetWidth()
        {
            return this.rect.Width;
        }

        public override void SetNewLocation(Point e)
        {
            this.rect = new Rectangle(e.X - operationBlockWidth / 2, e.Y - operationBlockHeight / 2, operationBlockWidth, operationBlockHeight);
        }

        public void Drawing(PictureBox pictureBox, DashStyle dashStyle, Color penColor, Color solidBrushColor, Brush brush)
        {
            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {
                
                flagGraphics.FillRectangle(Brushes.White, rect.X, rect.Y, rect.Width, rect.Height);

                flagGraphics.FillEllipse(brush, inCircle.rect.X, inCircle.rect.Y, inCircle.rect.Height, inCircle.rect.Width);
               

                //drawing rectangle
                using (Pen pen = new Pen(penColor, 2))
                {
                    pen.Alignment = PenAlignment.Inset;
                    pen.DashStyle = dashStyle;
                    flagGraphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);


                }

                //rysowanie stringa w środku:

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (Font drawFont = new Font("Arial", 7))
                    using (SolidBrush drawBrush = new SolidBrush(solidBrushColor))
                        flagGraphics.DrawString(text, drawFont, drawBrush, rect, sf);
                }

                inCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width / 2) - circleRadius, (int)rect.Y - circleRadius, 2 * circleRadius, 2 * circleRadius);
                outCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width / 2) - circleRadius, (int)rect.Y + (int)rect.Height - circleRadius, 2 * circleRadius, 2 * circleRadius);
            }

           
                if ( penColor == Color.Black)
                {
                    if(inCircle.isVisible)
                         this.DrawingCircle(pictureBox, penColor, Brushes.White, inCircle);
                    if(outCircle.isVisible)
                        this.DrawingCircle(pictureBox, penColor, Brushes.Black, outCircle);
                }
                else
                {
                    this.DrawingCircle(pictureBox, penColor, Brushes.White, inCircle);
                    this.DrawingCircle(pictureBox, penColor, Brushes.White, outCircle);
                }
                   
            

            pictureBox.Invalidate();
        }

        public override void Draw(PictureBox pictureBox, DashStyle dashStyle = DashStyle.Custom)
        {
            this.Drawing(pictureBox, dashStyle, Color.Black, Color.Black, Brushes.White);
        }


        public override void Erase(PictureBox pictureBox)
        {
            this.Drawing(pictureBox, DashStyle.Custom, Color.White, Color.White, Brushes.White);
        }

        public override bool IsVisible(Point point)
        {
            return this.rect.Contains(point);
        }
    }

    [Serializable]
    public class StartBlock : Block
    {
        private Circle outCircle;
        public Rectangle rect;
        public override bool IsTextEnabled { get; } = false;

        public StartBlock(Rectangle rect, BlockType block, Point point) : base( point)
        {
            text = "START";
            outCircle = new Circle();
            outCircles.Add(outCircle);
            outCircle.isVisible = true;
            this.rect = rect;
        }

        public override int GetHeight()
        {
            return this.rect.Height;
        }
        public override int GetWidth()
        {
            return this.rect.Width;
        }

        public override void SetNewLocation(Point e)
        {
            this.rect = rect = new Rectangle(e.X - StartStopBlockRadius, e.Y - StartStopBlockRadius, 2 * StartStopBlockRadius, 2 * StartStopBlockRadius);
        }

        public void Drawing(PictureBox pictureBox, DashStyle dashStyle, Color penColor, Color solidBrushColor, Brush brush)
        {
            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {
                
                flagGraphics.FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
                using (Pen pen = new Pen(penColor, 2))
                {
                    pen.Alignment = PenAlignment.Inset;
                    pen.DashStyle = dashStyle;
                    flagGraphics.DrawEllipse(pen, rect);

                    //rysowanie stringa w środku:
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;

                        using (Font drawFont = new Font("Arial", 16))
                        using (SolidBrush drawBrush = new SolidBrush(solidBrushColor))
                            flagGraphics.DrawString(this.text, drawFont, drawBrush, rect, sf);

                    }

                }

                outCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width / 2) - circleRadius, (int)rect.Y + (int)rect.Height - circleRadius, 2 * circleRadius, 2 * circleRadius);

            }

            if(penColor != Color.White)
            {
                if(outCircle.isVisible)
                    DrawingCircle(pictureBox, Color.Black, Brushes.Black, this.outCircle);

            }
            else
            {
                DrawingCircle(pictureBox, Color.White, Brushes.White, this.outCircle);
            }


            pictureBox.Invalidate();
        }

        public override void Draw(PictureBox pictureBox, DashStyle dashStyle = DashStyle.Custom)
        {

            this.Drawing(pictureBox, dashStyle, Color.Green, Color.Black, Brushes.White);
        }

        public override void Erase(PictureBox pictureBox)
        {
            this.Drawing(pictureBox, DashStyle.Custom, Color.White, Color.White, Brushes.White);
        }

        public override bool IsVisible(Point point)
        {
            return this.rect.Contains(point);
        }

    }

    [Serializable]
    public class StopBlock : Block
    {
        private Circle inCircle;
        private Rectangle rect;

        public override bool IsTextEnabled { get; } = false;

        public StopBlock(Rectangle rect, BlockType block, Point point) : base(  point)
        {
            text = "STOP";
            inCircle = new Circle();
            inCircles.Add(inCircle);
            inCircle.isVisible = true;
            this.rect = rect;
        }

        public override int GetHeight()
        {
            return this.rect.Height;
        }
        public override int GetWidth()
        {
            return this.rect.Width;
        }

        public override void SetNewLocation(Point e)
        {
            this.rect = rect = new Rectangle(e.X - StartStopBlockRadius, e.Y - StartStopBlockRadius, 2 * StartStopBlockRadius, 2 * StartStopBlockRadius);
        }

        public void Drawing(PictureBox pictureBox, DashStyle dashStyle, Color penColor, Color solidBrushColor, Brush brush)
        {
            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {
                
                flagGraphics.FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
                using (Pen pen = new Pen(penColor, 2))
                {
                    pen.Alignment = PenAlignment.Inset;
                    pen.DashStyle = dashStyle;
                    flagGraphics.DrawEllipse(pen, rect);

                    //rysowanie stringa w środku:
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;

                        using (Font drawFont = new Font("Arial", 16))
                        using (SolidBrush drawBrush = new SolidBrush(solidBrushColor))
                            flagGraphics.DrawString(this.text, drawFont, drawBrush, rect, sf);

                    }

                }
                inCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width / 2) - circleRadius, (int)rect.Y - circleRadius, 2 * circleRadius, 2 * circleRadius);
            }

            if (penColor != Color.White)
            {
                if(inCircle.isVisible)
                    DrawingCircle(pictureBox, Color.Black, Brushes.White, this.inCircle);

            }
            else
            {
                DrawingCircle(pictureBox, Color.White, Brushes.White, this.inCircle);
            }
            pictureBox.Invalidate();
        }

        public override void Draw(PictureBox pictureBox, DashStyle dashStyle = DashStyle.Custom)
        {

            this.Drawing(pictureBox, dashStyle, Color.Red, Color.Black, Brushes.White);
        }

        public override void Erase(PictureBox pictureBox)
        {
            this.Drawing(pictureBox, DashStyle.Custom, Color.White, Color.White, Brushes.White);
        }

        public override bool IsVisible(Point point)
        {
            return this.rect.Contains(point);
        }
    }
    
    [Serializable]
    public class DecisionBlock : Block
    {
        private Circle inCircle;
        private Circle trueCircle;
        private Circle falseCircle;

        public override bool IsTextEnabled { get; } = true;

        private PointF[] rhombusPoints;

        public DecisionBlock(PointF[] rhombusPoints, BlockType blockType, Point point) : base(  point)
        {
            inCircle = new Circle();
            trueCircle = new Circle();
            falseCircle = new Circle();

            inCircles.Add(inCircle);

            outCircles.Add(trueCircle);
            outCircles.Add(falseCircle);

            inCircle.isVisible = true;
            trueCircle.isVisible = true;
            falseCircle.isVisible = true;

            this.rhombusPoints = rhombusPoints;
        }

        public override int GetHeight()
        {

            return Math.Abs((int)(rhombusPoints[1].Y - rhombusPoints[3].Y));
        }
        public override int GetWidth()
        {
            return Math.Abs((int)(rhombusPoints[0].X - rhombusPoints[2].X));
        }

        public override void SetNewLocation(Point e)
        {
            this.rhombusPoints = new PointF[]
                    {
                        new Point(e.X + decisionBlockSize/2, e.Y),
                        new Point(e.X, e.Y + decisionBlockSize/2),
                        new Point(e.X - decisionBlockSize/2, e.Y),
                        new Point(e.X, e.Y - decisionBlockSize/2)
                    };
        }

        public void DrawingTrueFalse(PictureBox pictureBox, Color solidBrushColor, Rectangle rect, string text)
        {
            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {

                //rysowanie stringa w środku:
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    Rectangle textRect = new Rectangle((int)(rect.X) , (int)(rect.Y ), (int)rect.Width , (int)rect.Height);
                    using (Font drawFont = new Font("Arial", 7))
                    using (SolidBrush drawBrush = new SolidBrush(solidBrushColor))
                        flagGraphics.DrawString(text, drawFont, drawBrush, textRect, sf);
                }

                pictureBox.Invalidate();

            }
        }

        public void EraseTrueFalse(PictureBox pictureBox,  Rectangle rect)
        {

            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
                flagGraphics.FillEllipse(Brushes.White, rect.X, rect.Y, rect.Width, rect.Height);


            
            pictureBox.Invalidate();
        }

        public void Drawing(PictureBox pictureBox, DashStyle dashStyle, Color penColor, Color solidBrushColor, Brush brush)
        {
            using (Graphics flagGraphics = Graphics.FromImage(pictureBox.Image))
            {
             

               
                using (GraphicsPath rhombusPath = new GraphicsPath())
                {
                    rhombusPath.AddPolygon(rhombusPoints);

                    Region region = new Region(rhombusPath);

                    RectangleF rect = region.GetBounds(flagGraphics);

                    flagGraphics.FillPath(brush, rhombusPath);
                    using (Pen pen = new Pen(penColor, 2))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        pen.DashStyle = dashStyle;
                        flagGraphics.DrawPath(pen, rhombusPath);

                        //rysowanie stringa w środku:
                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;

                            Rectangle textRect = new Rectangle((int)(rect.X + rect.Width / 4), (int)(rect.Y + rect.Height / 4), (int)rect.Width / 2, (int)rect.Height / 2);
                            using (Font drawFont = new Font("Arial", 7))
                            using (SolidBrush drawBrush = new SolidBrush(solidBrushColor))
                                flagGraphics.DrawString(this.text, drawFont, drawBrush, textRect, sf);
                        }

                    }

                    inCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width/2) - circleRadius, (int)rect.Y - circleRadius, 2 * circleRadius, 2 * circleRadius);
                    falseCircle.rect= new Rectangle((int)rect.X - circleRadius, (int)rect.Y + (int)(rect.Height / 2) - circleRadius, 2 * circleRadius, 2 * circleRadius);
                    trueCircle.rect = new Rectangle((int)rect.X + (int)(rect.Width) - circleRadius, (int)rect.Y + (int)(rect.Height / 2) - circleRadius, 2 * circleRadius, 2 * circleRadius);
                }
              
                if (penColor == Color.Black)
                {
                    if(inCircle.isVisible)
                        this.DrawingCircle(pictureBox, penColor, Brushes.White, inCircle);

                    if (trueCircle.isVisible)
                    {
                        this.DrawingCircle(pictureBox, penColor, Brushes.Black, trueCircle);
                    }
                        this.DrawingTrueFalse(pictureBox, penColor, new Rectangle(trueCircle.rect.X, trueCircle.rect.Y - 3 * circleRadius, 2 * circleRadius, 2 * circleRadius), "T");


                    if (falseCircle.isVisible)
                    {
                        this.DrawingCircle(pictureBox, penColor, Brushes.Black, falseCircle);
                    }
                        this.DrawingTrueFalse(pictureBox, penColor, new Rectangle(falseCircle.rect.X, falseCircle.rect.Y - 3 * circleRadius, 2 * circleRadius, 2 * circleRadius), "F");
                    

                }      
                else
                {
                    this.DrawingCircle(pictureBox, penColor, Brushes.White, inCircle);

                    this.DrawingCircle(pictureBox, penColor, Brushes.White, trueCircle);
                    this.EraseTrueFalse(pictureBox,  new Rectangle(trueCircle.rect.X, trueCircle.rect.Y - 3 * circleRadius - 2, 2 * circleRadius, 3 * circleRadius));

                    this.DrawingCircle(pictureBox, penColor, Brushes.White, falseCircle);
                    this.EraseTrueFalse(pictureBox,  new Rectangle(falseCircle.rect.X, falseCircle.rect.Y - 3 * circleRadius - 2, 2 * circleRadius, 3 * circleRadius));

                }
                pictureBox.Invalidate();
            }
        }

        public override void Draw(PictureBox pictureBox, DashStyle dashStyle = DashStyle.Custom)
        {
            this.Drawing(pictureBox, dashStyle, Color.Black, Color.Black, Brushes.White);
        }

        public override void Erase(PictureBox pictureBox)
        {
            this.Drawing(pictureBox, DashStyle.Custom, Color.White, Color.White, Brushes.White);
        }

        public override bool IsVisible(Point point)
        {
            bool isVisible;
            using (GraphicsPath rhombusPath = new GraphicsPath())
            {
                rhombusPath.AddPolygon(rhombusPoints);
                isVisible = rhombusPath.IsVisible(point);

            }
            return isVisible;

            
        }
    }

}
