using Memory.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
namespace Memory
{
    public partial class Form1 : Form
    {
        int gameSize;
        List<Image> images = new List<Image>
        {
            Resources.arsenal,
            Resources.aston,
            Resources.chelsea,
            Resources.tottenham,
            Resources.manchester,
            Resources.manchesterU,
            Resources.liverpool,
            Resources.nottingham

        };
        Image background = Resources.pytajnik;
        Tuple<int, int> card1;
        Tuple<int, int> card2;
        string[] gracze = new string[2];
    
        List<int[]> gameBoard;
        List<PictureBox[]> gameImages;
        int curTurn = 0;
        int[] score = new int[2] { 0, 0 };
        bool waiting = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void nowaGraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gamePanel.Controls.Clear();
            Form2 ilosc = new Form2();
          
            if (ilosc.ShowDialog(this) == DialogResult.OK)
            {
                gracze[0] = ilosc.textBox2.Text;
                gracze[1] = ilosc.textBox3.Text;

                gameSize =  int.Parse(ilosc.textBox1.Text);
                if (gameSize > images.Count)
                {
                    gameSize = 7;
                    MessageBox.Show("Ilość par przekroczyła maksymalną. Ustawiono na " + images.Count);
                }
                if (gameSize <= 1)
                {
                    gameSize = 5;
                    MessageBox.Show("Ilość nie może być mniejsza od 2. Ustawiono 5");
                }
            }
            startGame();
        }
         public static void ShuffleGameBoard(List<int[]> gameBoard, Tuple<int, int> gameDimensions)
        {
            List<int> flattened = new List<int>(); 
            for (int i = 0; i < gameDimensions.Item1; i++) { for (int j = 0; j < gameDimensions.Item2; j++)
                {
                    flattened.Add(gameBoard[i][j]);
                } }
            Random rand = new Random();
            for (int i = flattened.Count - 1; i > 0; i--)
            { int j = rand.Next(0, i + 1); 
                int temp = flattened[i]; 
                flattened[i] = flattened[j]; 
                flattened[j] = temp; }
        
            int index = 0;
            for (int i = 0; i < gameDimensions.Item1; i++)
            {
                for (int j = 0; j < gameDimensions.Item2; j++) 
                { gameBoard[i][j] = flattened[index++]; 
                }
            }

        }
            private void startGame()
            {

            score[0] = 0;
            score[1] = 0;
                Tuple<int, int> gameDimensions = FindClosestDivisors(gameSize*2);
            gameBoard = new List<int[]>();
            gameImages = new List<PictureBox[]>();
            int n = 0;
            int k = 0;
            for(int i=0; i<gameDimensions.Item1; i++)
            {
                gameBoard.Add(new int[gameDimensions.Item2]);
                gameImages.Add(new PictureBox[gameDimensions.Item2]);
                for(int j = 0; j < gameDimensions.Item2; j++)
                {
                    gameBoard[i][j] = n;
                    n += (k% 2);
                    k++;
                }
            }
      
            ShuffleGameBoard(gameBoard,gameDimensions);
         
            gamePanel.RowCount = gameDimensions.Item1;
            gamePanel.ColumnCount = gameDimensions.Item2;
            for (int i = 0; i < gamePanel.ColumnCount; i++) 
            {
                gamePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / gamePanel.RowCount)); 
            }
            for (int j = 0; j < gamePanel.RowCount; j++) 
            {
                gamePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100/gamePanel.RowCount)); 
            }
            gamePanel.Height = gamePanel.Width / gamePanel.ColumnCount * gamePanel.RowCount;
         
            for (int i = 0; i < gameDimensions.Item1; i++)
            {
                for (int j = 0; j < gameDimensions.Item2; j++)
                {
                    int row = i;
                    int col = j;
                    var img =  new PictureBox { Image = background , Parent = gamePanel, SizeMode = PictureBoxSizeMode.StretchImage,Width = gamePanel.Width/gamePanel.ColumnCount, Height = gamePanel.Width/gamePanel.ColumnCount, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill};
                    img.Click += (sender, e) =>
                    {
                        flip(row,col);
                    };
                    gameImages[i][j] = img;
                    gamePanel.Controls.Add(img, j,i);
              

                }
            }
        }
        void flip(int x, int y)
        {
            if(waiting) return;
            gameImages[x][y].Image = images[gameBoard[x][y]];
       
            if (card1 != null)
            {
               
                card2 = Tuple.Create(x, y);
                
                if (gameBoard[card1.Item1][card1.Item2] == gameBoard[card2.Item1][card2.Item2])
                {
                 
                    score[curTurn]++;
                    if (score[0] + score[1] == gameSize)
                    {
                        if (score[0] == score[1])
                        {
                            MessageBox.Show("Remis","Wynik",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        }
                        else
                        {


                            string winner = score[0] > score[1] ? gracze[0] : gracze[1];
                            MessageBox.Show("Wygrał gracz " + winner, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    label2.Text = "Wynik: " + score[0].ToString() + ":" + score[1].ToString();
                    waiting = true;
                    Task.Delay(500).ContinueWith(_ => {
                        gameImages[x][y].Invoke((Action)(() => gameImages[x][y].Visible = false));
                        gameImages[card1.Item1][card1.Item2].Invoke((Action)(() => gameImages[card1.Item1][card1.Item2].Visible = false));
                        card1 = null;
                        card2 = null;
                        waiting = false;
                    });
                }
                else
                {


                    if (curTurn == 0)
                    {
                        curTurn = 1;
                    }
                    else
                    {
                        curTurn = 0;
                    }
                    label1.Text = "Tura: " + gracze[curTurn];
                    waiting = true;
                    Task.Delay(1000).ContinueWith(_ => {
                        gameImages[x][y].Invoke((Action)(() => gameImages[x][y].Image = background));
                        gameImages[card1.Item1][card1.Item2].Invoke((Action)(() => gameImages[card1.Item1][card1.Item2].Image = background));
                        card1 = null;
                        card2 = null;
                        waiting = false;
                    });
                }

                
                
            }
            else
            {
                card1 = Tuple.Create(x, y);
                
            }
            
        }
        static Tuple<int, int> FindClosestDivisors(int n)
        {
            int sqrtN = (int)Math.Sqrt(n); 
            for (int i = sqrtN; i >= 1; i--) {
                if (n % i == 0) {
                    int divisor1 = i;
                    int divisor2 = n / i;
                    return Tuple.Create(divisor1, divisor2); } 
            }
            return Tuple.Create(1, n); 
        }
    }
}
