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
using System.Globalization;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace WindowsFormsApp1
{
    [Serializable]
    public enum BlockType
    {
        Start = 0,
        Stop = 1,
        Operation = 2,
        Decision = 3,
        Link = 4,
        Delete = 5
    }

    
    public partial class Form1 : Form
    {
        
        private BlockType blockType = BlockType.Operation;
        private List<Block> blocks = new List<Block>();

        private bool isEnabled = false;
        private bool hasStart = false;
        private bool hasStop = false;
        private Block blockEnabled;

        protected int operationBlockWidth = 100;
        protected int operationBlockHeight = 70;
        protected int decisionBlockSize = 100;
        protected int StartStopBlockRadius = 40;




        private bool IsMouseDown = false;
        private int startX;
        private int startY;
        private int CurX;
        private int CurY;
        private Block fromBlock;
        private Circle fromCircle;

        private List<Connection> establishedConnection = new List<Connection>();


        private bool isScrollDown = false;



        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

        }

        private void decisionButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Decision;
            startButton.BackColor = Color.LightGray;
            stopButton.BackColor = Color.LightGray;
            linkButton.BackColor = Color.LightGray;
            deleteButton.BackColor = Color.LightGray;
            operationButton.BackColor = Color.LightGray;
            decisionButton.BackColor = Color.DarkGray;
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();


            DialogResult res = form2.ShowDialog();

            if (res == DialogResult.OK)
            {

                this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.pictureBox1.Location = new System.Drawing.Point(-6, 3);
                this.pictureBox1.Name = "pictureBox1";
                this.pictureBox1.Size = new System.Drawing.Size((int)form2.x, (int)form2.y);
                this.pictureBox1.TabIndex = 0;
                this.pictureBox1.TabStop = false;




                Bitmap flag = new Bitmap((int)form2.x, (int)form2.y);
                Graphics flagGraphics = Graphics.FromImage(flag);


                flagGraphics.FillRectangle(Brushes.White, pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);

                pictureBox1.Image = flag;

                hasStart = false;
                hasStop = false;
                isEnabled = false;
                blockEnabled = null;
                blocks.Clear();
                fromBlock = null;
                fromCircle = null;
                IsMouseDown = false;
                isScrollDown = false;
                establishedConnection.Clear();



                pictureBox1.Invalidate();

            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && isScrollDown != true) //zaznaczanie istniejącego
            {
                Block foundBlock = null;
                bool isFound = false;
                foreach (var block in blocks)
                    if (block.IsVisible(e.Location))
                    {
                        foundBlock = block;
                        isFound = true;
                        break;
                    }

                if (isFound)
                {
                    if (isEnabled && blockEnabled != foundBlock)
                    {
                        //disenabling previous one:

                        DisenableRect();

                    }

                    isEnabled = true;
                    blockEnabled = foundBlock;

                    //enabling:
                    if (blockEnabled.IsTextEnabled)
                        textBox.Enabled = true;
                    else
                        textBox_EnabledChanged(textBox, new EventArgs());



                    blockEnabled.Draw(pictureBox1, DashStyle.Dash);

                }
                else
                    DisenableRect();

            }
            else if (e.Button == MouseButtons.Left && isScrollDown!= true)
            {
                Block foundBlock;
                bool isFound;

                if (blockType == BlockType.Operation)
                {
                    CreateOperationBlock(e.X, e.Y, operationBlockWidth, operationBlockHeight);
                }
                else if (blockType == BlockType.Decision)
                {
                    CreateDecisionBlock(e, 100, 100);
                }

                else if (blockType == BlockType.Start)
                {
                    if (!hasStart)
                    {
                        CreateStartBlock(e, 40);
                        hasStart = true;
                    }
                    else
                        MessageBox.Show("Schemat posiada już blok startowy!");

                }
                else if (blockType == BlockType.Stop)
                {
                    if (!hasStop)
                    {
                        CreateStopBlock(e, 40);

                        hasStop = true;
                    }
                    else
                        MessageBox.Show("Schemat posiada już blok końcowy!");
                }
                else if (blockType == BlockType.Delete) // delete
                {
                    foundBlock = null;
                    isFound = false;
                    foreach (var block in blocks)
                        if (block.IsVisible(e.Location))
                        {
                            foundBlock = block;
                            isFound = true;
                            break;
                        }

                    if (isFound)
                    {
                        foundBlock.Erase(pictureBox1);

                        //deleting all connections:
                        foreach (var connection in foundBlock.childs)
                        {
                            Circle circle = connection.toCircle;
                            circle.isLinked = false;
                            circle.isVisible = true;

                            connection.toBlock.inCircles.Add(circle);
                            connection.toBlock.parents.Remove(connection);
                            establishedConnection.Remove(connection);

                            //erasing the line connecting  the block
                            using (Pen LinePen = new Pen(Color.White, 3))
                            {
                                AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                                LinePen.CustomEndCap = bigArrow;
                                pictureBox1.CreateGraphics().DrawLine(LinePen, connection.from, connection.to);
                            }


                            //redrawing the child:
                            connection.toBlock.Draw(pictureBox1);

                        }

                        foreach (var connection in foundBlock.parents)
                        {
                            Circle circle = connection.fromCircle;
                            circle.isLinked = false;
                            circle.isVisible = true;

                            connection.fromBlock.outCircles.Add(circle);
                            connection.fromBlock.childs.Remove(connection);
                            establishedConnection.Remove(connection);


                            //erasing the line connecting  the block
                            using (Pen LinePen = new Pen(Color.White, 3))
                            {
                                AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                                LinePen.CustomEndCap = bigArrow;
                                pictureBox1.CreateGraphics().DrawLine(LinePen, connection.from, connection.to);
                            }
                               

                            //redrawing the parent:
                            connection.fromBlock.Draw(pictureBox1);
                        }

                        pictureBox1.Invalidate();


                        if (blockEnabled == foundBlock)
                        {
                            isEnabled = false;
                            blockEnabled = null;
                            textBox.Text = "";
                            textBox.Enabled = false;


                        }

                        if (foundBlock is StartBlock)
                            hasStart = false;

                        if (foundBlock is StopBlock)
                            hasStop = false;

                        blocks.Remove(foundBlock);

                        foreach (var block in blocks)
                        {
                            if (block == blockEnabled)
                                block.Draw(pictureBox1, DashStyle.Dash);
                            else
                                block.Draw(pictureBox1, DashStyle.Custom);
                        }
                    }

                }
            }
        }

        private void CreateOperationBlock(int X, int Y, int width, int height)
        {
            Rectangle rect = new Rectangle(X - width / 2, Y - height / 2, width, height);
          
            Block block = new OperationBlock(rect, BlockType.Operation, new Point(X, Y));
            block.Draw(pictureBox1, DashStyle.Custom);
            blocks.Add(block);
        }


        private void CreateStopBlock(MouseEventArgs e, int radius)
        {
            Rectangle rect = new Rectangle(e.X - radius, e.Y - radius, 2 * radius, 2 * radius);
         
            Block block = new StopBlock(rect, BlockType.Stop, new Point(e.X, e.Y));
            block.Draw(pictureBox1);
            blocks.Add(block);

        }


        private void CreateStartBlock(MouseEventArgs e, int radius)
        {

            Rectangle rect = new Rectangle(e.X - radius, e.Y - radius, 2 * radius, 2 * radius);
            
            Block block = new StartBlock(rect, BlockType.Start, new Point(e.X, e.Y));
            block.Draw(pictureBox1);
            blocks.Add(block);
        }

        private void CreateDecisionBlock(MouseEventArgs e, int width, int height)
        {
            PointF[] rhombusPoints =
            {
                    new Point(e.X + width/2, e.Y),
                    new Point(e.X, e.Y + height/2),
                    new Point(e.X - width/2, e.Y),
                    new Point(e.X, e.Y - height/2)
            };

            Block block = new DecisionBlock(rhombusPoints, BlockType.Decision, new Point(e.X, e.Y));
            block.Draw(pictureBox1, DashStyle.Custom);

            blocks.Add(block);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Start;
            startButton.BackColor = Color.DarkGray;
            stopButton.BackColor = Color.LightGray;
            linkButton.BackColor = Color.LightGray;
            deleteButton.BackColor = Color.LightGray;
            operationButton.BackColor = Color.LightGray;
            decisionButton.BackColor = Color.LightGray;

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Stop;
            startButton.BackColor = Color.LightGray;
            stopButton.BackColor = Color.DarkGray;
            linkButton.BackColor = Color.LightGray;
            deleteButton.BackColor = Color.LightGray;
            operationButton.BackColor = Color.LightGray;
            decisionButton.BackColor = Color.LightGray;

        }

        private void operationButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Operation;
            startButton.BackColor = Color.LightGray;
            stopButton.BackColor = Color.LightGray;
            linkButton.BackColor = Color.LightGray;
            deleteButton.BackColor = Color.LightGray;
            operationButton.BackColor = Color.DarkGray;
            decisionButton.BackColor = Color.LightGray;

        }

        private void linkButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Link;
            startButton.BackColor = Color.LightGray;
            stopButton.BackColor = Color.LightGray;
            linkButton.BackColor = Color.DarkGray;
            deleteButton.BackColor = Color.LightGray;
            operationButton.BackColor = Color.LightGray;
            decisionButton.BackColor = Color.LightGray;

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            this.blockType = BlockType.Delete;
            startButton.BackColor = Color.LightGray;
            stopButton.BackColor = Color.LightGray;
            linkButton.BackColor = Color.LightGray;
            deleteButton.BackColor = Color.DarkGray;
            operationButton.BackColor = Color.LightGray;
            decisionButton.BackColor = Color.LightGray;

        }
        private void DisenableRect()
        {
            if (blockEnabled != null)
                blockEnabled.Draw(pictureBox1, DashStyle.Custom);
            blockEnabled = null;
            isEnabled = false;
            textBox.Enabled = false;
            textBox.Text = "";
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (isEnabled)
            {
                blockEnabled.text = ((TextBox)sender).Text;
                blockEnabled.Draw(pictureBox1, DashStyle.Dash);
            }
        }

        private void textBox_EnabledChanged(object sender, EventArgs e)
        {
            if (isEnabled)
                ((TextBox)sender).Text = blockEnabled.text;
            else
                ((TextBox)sender).Text = "";
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            if (IsMouseDown != false)
            {
                CurX = e.X;
                CurY = e.Y;

                pictureBox1.Invalidate();
            }



            if (isScrollDown)
            {
               

                blockEnabled.Erase(pictureBox1);

                int xChange = blockEnabled.center.X - e.X;
                int yChange = blockEnabled.center.Y - e.Y;

                using (Pen linePen = new Pen(Color.White, 3))
                    foreach (var line in blockEnabled.parents)
                    {
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                        linePen.CustomEndCap = bigArrow;
                        pictureBox1.CreateGraphics().DrawLine(linePen, line.from, line.to);
                        line.to = new Point(line.to.X - xChange, line.to.Y - yChange);
                    }

                using (Pen linePen = new Pen(Color.White, 3))
                    foreach (var line in blockEnabled.childs)
                    {
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                        linePen.CustomEndCap = bigArrow;
                        pictureBox1.CreateGraphics().DrawLine(linePen, line.from, line.to);
                        line.from = new Point(line.from.X - xChange, line.from.Y - yChange);
                    }




                blockEnabled.center = new Point(e.X, e.Y);
                blockEnabled.SetNewLocation(e.Location);

               

                foreach (var block in blocks)
                    if (isEnabled && block == blockEnabled)
                    {
                        block.Draw(pictureBox1, DashStyle.Dash);
                    }
                    else
                        block.Draw(pictureBox1);

                pictureBox1.Invalidate();
            }

            return;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                IsMouseDown = false;
                pictureBox1.Invalidate();

            }



            if (isEnabled && e.Button == MouseButtons.Middle)
                isScrollDown = false;



        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Pen linePen = new Pen(Color.Black, 3))
            {
                AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                linePen.CustomEndCap = bigArrow;
                foreach (var line in establishedConnection)
                    e.Graphics.DrawLine(linePen, line.from, line.to);
            }



            if (blockType == BlockType.Link)
                if (IsMouseDown == false)
                {
                    foreach (var block in blocks)
                        foreach (var circle in block.inCircles)
                            if (block != fromBlock && circle.rect.Contains(new Point(CurX, CurY)))
                            {

                                if (fromBlock == null) return;

                                //circles becomes invisible when linked
                                circle.isLinked = true;
                                circle.isVisible = false;

                                fromCircle.isVisible = false;
                                fromCircle.isLinked = true;

                                //remember to add when one of the connected block will be deleted
                                block.inCircles.Remove(circle);
                                fromBlock.outCircles.Remove(fromCircle);

                                //establishing new connection
                                Connection connection = new Connection(new Point(startX, startY), new Point(CurX, CurY), fromBlock, block, fromCircle, circle);
                                establishedConnection.Add(connection);

                                fromBlock.childs.Add(connection);
                                block.parents.Add(connection);


                                //repainting connected blocks:
                                block.Erase(pictureBox1);
                                if (block == blockEnabled)
                                    block.Draw(pictureBox1, DashStyle.Dash);
                                else
                                    block.Draw(pictureBox1);

                                fromBlock.Erase(pictureBox1);

                                if (fromBlock == blockEnabled)
                                    fromBlock.Draw(pictureBox1, DashStyle.Dash);
                                else
                                    fromBlock.Draw(pictureBox1);

                                //painting line connecting block
                                using (Pen LinePen = new Pen(Color.Black, 3))
                                {
                                    AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                                    LinePen.CustomEndCap = bigArrow;
                                    e.Graphics.DrawLine(LinePen, connection.from, connection.to);
                                }
                                    

                                break;
                            }
                }
                else
                {
                    using (Pen dashed_pen = new Pen(Color.Black, 1))
                    {
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                        dashed_pen.CustomEndCap = bigArrow;
                        e.Graphics.DrawLine(dashed_pen, startX, startY, CurX, CurY);
                    }
                       

                }




        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && blockType == BlockType.Link)
            {

                foreach (var block in blocks)
                    foreach (var circle in block.outCircles)
                        if (circle.rect.Contains(e.Location))
                        {
                            IsMouseDown = true;
                            fromBlock = block;
                            fromCircle = circle;
                            startX = e.X;
                            startY = e.Y;
                            CurX = e.X;
                            CurY = e.Y;
                            break;
                        }
            }
            else if (e.Button == MouseButtons.Middle && isEnabled)
                isScrollDown = true;


        }

        private void button6_Click(object sender, EventArgs e)
        {
            Size formSize = this.Size;
            Point loc = this.Location;
            
            Size tableSize = tableLayoutPanel1.Size;
            Image image = this.pictureBox1.Image;
            Size pictureSize = this.pictureBox1.Size;
            


            CultureInfo.CurrentUICulture = new CultureInfo("pl");
            Controls.Clear();
            InitializeComponent();

            this.pictureBox1.Image = image;
            this.Size = formSize;
            this.Location = loc;
            this.leftPanel.AutoScroll = true;
            this.tableLayoutPanel1.Size = tableSize;
            this.pictureBox1.Size = pictureSize;

            button6.BackColor = Color.DarkGray;
            button7.BackColor = Color.LightGray;
        }

      
        private void button7_Click(object sender, EventArgs e)
        {

            Size formSize = this.Size;
            Point loc = this.Location;
            bool state = this.Enabled;
            Size tableSize = tableLayoutPanel1.Size;
            Image image = this.pictureBox1.Image;
            Size pictureSize = this.pictureBox1.Size;
           

            CultureInfo.CurrentUICulture = new CultureInfo("en");
            Controls.Clear();
            InitializeComponent();


            this.leftPanel.AutoScroll = true;
            this.pictureBox1.Image = image;
            this.Size = formSize;
            this.Location = loc;
            this.Enabled = state;
            this.tableLayoutPanel1.Size = tableSize;
            this.pictureBox1.Size = pictureSize;


            button7.BackColor = Color.DarkGray;

            button6.BackColor = Color.LightGray;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            

            saveFileDialog1.Filter = "Bloq files (*.bq)|*.bq";
          
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
               

                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(myStream , this.blocks);
                        formatter.Serialize(myStream, this.pictureBox1.Size);
                    }
                    catch (SerializationException exception)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + exception.Message);
                        throw;
                    }
                    finally
                    {
                        myStream.Close();
                    }

                   
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stream myStream;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();


            openFileDialog1.Filter = "Bloq files (*.bq)|*.bq";

            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                List<Block> savedBlocks = null;
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        savedBlocks = (List<Block>)(formatter.Deserialize(myStream));
                        pictureBox1.Size = (Size)(formatter.Deserialize(myStream));
                    }
                    catch (SerializationException exception)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + exception.Message);
                        throw;
                    }
                    finally
                    {
                        myStream.Close();
                    }

                    if(savedBlocks != null)
                    {
                        blocks = savedBlocks;
                       


                        Bitmap flag = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        Graphics flagGraphics = Graphics.FromImage(flag);


                        flagGraphics.FillRectangle(Brushes.White, pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);

                        pictureBox1.Image = flag;


                        hasStart = false;
                        hasStop = false;
                        isEnabled = false;
                        blockEnabled = null;
                       
                        fromBlock = null;
                        fromCircle = null;
                        IsMouseDown = false;
                        isScrollDown = false;

                        List<Connection> newConnections = new List<Connection>();
                        foreach (var block in savedBlocks)
                            foreach(var connection in block.parents)
                            newConnections.Add(connection);

                        establishedConnection = newConnections;

                        foreach (var block in savedBlocks)
                        {
                            if (block is StopBlock)
                                hasStop = true;

                            if (block is StartBlock)
                                hasStart = true;

                            block.Draw(pictureBox1);
                        }   
                            

                        pictureBox1.Invalidate();
                    }
                        





                }
            }
        }
    }

}





