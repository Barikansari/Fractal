using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace Fractal
{
    public partial class Fractal : Form
    {

        private int MAX = 256;      // max iterations
        private double SX = -2.025; // start value real
        private double SY = -1.125; // start value imaginary
        private double EX = 0.6;    // end value real
        private double EY = 1.125;  // end value imaginary


        private static int x1, y1, xs, ys, xe, ye;
        private static double xstart, ystart, xende, yende, xzoom, yzoom;
        private static float xy;
        private Image picture;
        private Graphics g1;
        private HSB HSBcol = new HSB();
        private Pen pen;

        private static bool action, rectangle, finished;

        public Fractal()
        {
            InitializeComponent();

            SX = readState()[0];
            SY = readState()[1];
            EX = readState()[2];
            EY = readState()[3];

            finished = false;
            x1 = pictureBox1.Width;
            y1 = pictureBox1.Height;
            xy = (float)x1 / (float)y1;
            picture = new Bitmap(x1, y1);
            g1 = Graphics.FromImage(picture);
            finished = true;

            start();
            pictureBox1.Cursor = Cursors.Cross;
            
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (action)
            {
                xe = e.X;
                ye = e.Y;
                update();
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog f = new SaveFileDialog();
            f.Filter = "JPG(*.JPG) | *.JPG";
            if (f.ShowDialog() == DialogResult.OK)
            {
                picture.Save(f.FileName);
            }
            

        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveState(-2.025, -1.125, 0.6, 1.125);
            using (StreamWriter sw = File.CreateText("colorstate.txt"))
            {
                sw.WriteLine(0);
            }
            Application.Restart();
            
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Invalidate();
            pictureBox1.Dispose();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(pictureBox1.Image, 0, 0);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument p = new PrintDocument();
            p.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            p.Print();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new Fractal().Show();
        }

        private void changeColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int temp = new Random().Next(1, 8);
            mandelbrot(temp);

            using (StreamWriter sw = File.CreateText("colorstate.txt"))
            {
                sw.WriteLine(temp);
            }

            update();
        }

        private void startAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Start();

        }

        private int timerInt = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerInt++;
            
            if (timerInt >= 8)
            {
                timerInt = 1;
            } else
            {
                mandelbrot(timerInt);
                update();
            }

        }
        
        private void stopAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int z, w;
            if (action)
            {
                xe = e.X;
                ye = e.Y;
                if (xs > xe)
                {
                    z = xs;
                    xs = xe;
                    xe = z;
                }
                if (ys > ye)
                {
                    z = ys;
                    ys = ye;
                    ye = z;
                }
                w = (xe - xs);
                z = (ye - ys);
                if ((w < 2) && (z < 2)) initvalues();
                else
                {
                    if (((float)w > (float)z * xy)) ye = (int)((float)ys + (float)w / xy);
                    else xe = (int)((float)xs + (float)z * xy);
                    xende = xstart + xzoom * (double)xe;
                    yende = ystart + yzoom * (double)ye;
                    xstart += xzoom * (double)xs;
                    ystart += yzoom * (double)ys;
                }
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;
                int num = 0;
                using (StreamReader sr = File.OpenText("colorstate.txt"))
                {
                    int s = 0;
                    while ((s = Convert.ToInt32(sr.ReadLine())) != 0)
                    {
                        num = s;
                    }
                }

                mandelbrot(num);
                rectangle = false;
                update();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            action = true;
            if (action)
            {
                xs = e.X;
                ys = e.Y;
                rectangle = true;
            }
            
        }
        
        
        public void start()
        {
            action = false;
            rectangle = false;
            initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;

            int num = 0;
            using (StreamReader sr = File.OpenText("colorstate.txt"))
            {
                int s = 0;
                while ((s = Convert.ToInt32(sr.ReadLine())) != 0)
                {
                    num = s;
                }
            }

            mandelbrot(num);

        }
        
        private void mandelbrot(int num = 0) // calculate all points
        {
            int x, y;
            float h, b, alt = 0.0f;

            action = false;
            for (x = 0; x < x1; x += 2)
            {
                for (y = 0; y < y1; y++)
                {

                    h = pointcolour(xstart + xzoom * (double)x, ystart + yzoom * (double)y); // color value

                    if (h != alt)
                    {
                        b = 1.0f - h * h; // brightnes
                                          ///djm added

                        HSBcol.fromHSB(h * 255, 0.8f * 255, b * 255, num); //convert hsb to rgb then make a Java Color
                                                                      //HSBcol.fromHSB(h * color, 0.8f * 255, b * 255);
                        Color col = Color.FromArgb((int)HSBcol.rChan, (int)HSBcol.gChan, (int)HSBcol.bChan);
                        pen = new Pen(col);
                        
                        alt = h;
                    }

                    g1.DrawLine(pen, x, y, x + 1, y);

                }
            }

            action = true;
        }

        private float pointcolour(double xwert, double ywert) // color value from 0.0 to 1.0 by iterations
        {
            double r = 0.0, i = 0.0, m = 0.0;
            int j = 0;

            while ((j < MAX) && (m < 4.0))
            {
                j++;
                m = r * r - i * i;
                i = 2.0 * r * i + ywert;
                r = m + xwert;
            }
            return (float)j / (float)MAX;
        }

        private void initvalues() // reset start values
        {
            xstart = SX;
            ystart = SY;
            xende = EX;
            yende = EY;
            if ((float)((xende - xstart) / (yende - ystart)) != xy)
                xstart = xende - (yende - ystart) * (double)xy;
        }
        public void destroy() // delete all instances 
        {
            if (finished)
            {
                picture = null;
                g1 = null;
            }
        }
        public void update()
        {

            saveState(xstart, ystart, xende, yende);
            
            Graphics g = pictureBox1.CreateGraphics();
            g.DrawImage(picture, 0, 0);
            if (rectangle)
            {

                Pen p = new Pen(Color.White, 1);
                if (xs < xe)
                {
                    if (ys < ye)
                    {
                        g.DrawRectangle(p, xs, ys, (xe - xs), (ye - ys));
                    }
                    else
                    {
                        g.DrawRectangle(p, xs, ye, (xe - xs), (ys - ye));
                    }
                }
                else
                {
                    if (ys < ye)
                    {
                        g.DrawRectangle(p, xe, ys, (xs - xe), (ye - ys));
                    }
                    else
                    {
                        g.DrawRectangle(p, xe, ye, (xs - xe), (ys - ye));
                    }
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics obj = e.Graphics;
            obj.DrawImage(picture, new Point(0, 0));

        }

        private void Fractal_Shown(object sender, EventArgs e)
        {
           
        }

        private void saveState(double xstart, double ystart, double xend, double yend)
        {
            using(StreamWriter sw = File.CreateText("state.txt")) {
                sw.WriteLine(xstart);
                sw.WriteLine(ystart);
                sw.WriteLine(xend);
                sw.WriteLine(yend);
            }
            
        }

        private List<double> readState()
        {
            List<double> l = new List<double>();

            using(StreamReader sr = File.OpenText("state.txt")) {
                double s = 0;
                while ((s = Convert.ToDouble(sr.ReadLine())) != 0)
                {
                    l.Add(s);
                }
            }

            return l;
        }

    }
}
