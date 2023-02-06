using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HiveSimulator
{
    public partial class Form1 : Form
    {
        private World world;
        private Random random = new Random();
        private DateTime start = DateTime.Now;
        private DateTime end;
        private int framesRun = 0;
        public Form1()
        {
            InitializeComponent();
            world = new World(new Bee.BeeMessage(SendMessage));

            timer1.Interval = 50;
            timer1.Tick += new EventHandler(RunFrame);
            timer1.Enabled = false;
            UpdateStats(new TimeSpan());
        }

        private void UpdateStats(TimeSpan frameDuration)
        {
            Bees.Text = world.Bees.Count.ToString();
            Flowers.Text = world.Flowers.Count.ToString();
            HoneyInHive.Text = String.Format("{0:f3}", world.Hive.Honey);
            double nectar = 0;
            foreach (Flower flower in world.Flowers)
                nectar += flower.Nectar;
            NectarInFlowers.Text = String.Format("{0:f3}", nectar);
            FramesRun.Text = framesRun.ToString();
            double milliSeconds = frameDuration.TotalMilliseconds;
            if (milliSeconds != 0.0)
                FrameRate.Text = string.Format("{0:f0} ({1:f1}ms)",
                1000 / milliSeconds, milliSeconds);
            else
                FrameRate.Text = "brak";
        }

        public void RunFrame(object sender, EventArgs e) 
        {
            framesRun++;
            world.Go(random);
            end = DateTime.Now;
            TimeSpan frameDuration = end - start;
            start = end;
            UpdateStats(frameDuration);
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled) 
            {
                toolStrip1.Items[0].Text = "Wznów symulację";
                timer1.Stop();
            }
            else
            {
                toolStrip1.Items[0].Text = "Zatrzymaj symulację";
                timer1.Start();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            framesRun = 0;
            world = new World(new Bee.BeeMessage(SendMessage));
            if (!timer1.Enabled)
            {
                toolStrip1.Items[0].Text = "Rozpocznij symulację";
            }
        }
        private void SendMessage(int ID, string Message)
        {
            statusStrip1.Items[0].Text = "Pszczoła numer " + ID + ": " + Message;
            var beeGroups =
            from bee in world.Bees
            group bee by bee.CurrentState into beeGroup
            orderby beeGroup.Key
            select beeGroup;
            listBox1.Items.Clear();
            foreach (var group in beeGroups)
            {
                string s;
                if (group.Count() == 1)
                    s = "pszczoła";
                else if (group.Count() > 4)
                    s = "pszczół";
                else
                    s = "pszczoły";

                string stringState;
                switch (group.Key)
                {
                    case BeeState.FlyingToFlower:
                        stringState = "Lot w kierunku kwiatu";
                        break;
                    case BeeState.GatheringNectar:
                        stringState = "Zbieranie nektaru";
                        break;
                    case BeeState.MakingHoney:
                        stringState = "Wytwarzanie miodu";
                        break;
                    case BeeState.Retired:
                        stringState = "Na emeryturze";
                        break;
                    case BeeState.ReturningToHive:
                        stringState = "Powrót do ula";
                        break;
                    default:
                        stringState = "Bezrobocie";
                        break;
                }

                listBox1.Items.Add(stringState + ": "
                                 + group.Count() + " " + s);
                if (group.Key == BeeState.Idle && group.Count() == world.Bees.Count() && framesRun > 0)
                {
                    listBox1.Items.Add("Symulacja zakończona: wszystkie pszczoły są bezrobotne.");
                    toolStrip1.Items[0].Text = "Symulacja zakończona";
                    statusStrip1.Items[0].Text = "Symulacja zakończona";
                    timer1.Enabled = false;
                }
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            World currentWorld = world;
            int currentFramesRun = framesRun;

            bool enabled = timer1.Enabled;
            if (enabled) { timer1.Stop(); }

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Plik symulatora (*.bees)|*.bees";
            openDialog.CheckPathExists = true;
            openDialog.CheckFileExists = true;
            openDialog.Title = "Wybierz plik z symulacją do odczytu.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (Stream output = File.OpenRead(openDialog.FileName))
                    {
                        world = (World)bf.Deserialize(output);
                        framesRun = (int)bf.Deserialize(output);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nie można odczytać pliku symulatora.\r\n " + ex.Message, "Błąd symulatora ula.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    world = currentWorld;
                    framesRun = currentFramesRun;
                }
            }

            world.Hive.MessageSender = new Bee.BeeMessage(SendMessage);
            foreach (Bee bee in world.Bees)
            {
                bee.MessageSender = new Bee.BeeMessage(SendMessage);
            }
            if (enabled)
                timer1.Start();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            bool enabled = timer1.Enabled;
            if (enabled)
                timer1.Stop();
            world.Hive.MessageSender = null;
            foreach (Bee bee in world.Bees)
                bee.MessageSender = null;
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Plik symulatora (*.bees)|*.bees";
            saveDialog.CheckPathExists = true;
            saveDialog.Title = "Wybierz plik do zapisania bieżącej symulacji.";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (Stream output = File.OpenWrite(saveDialog.FileName))
                    {
                        bf.Serialize(output, world);
                        bf.Serialize(output, framesRun);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nie można zapisać pliku symulatora.\r\n" + ex.Message,
                    "Błąd symulatora ula.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            world.Hive.MessageSender = new Bee.BeeMessage(SendMessage);
            foreach (Bee bee in world.Bees)
                bee.MessageSender = new Bee.BeeMessage(SendMessage);
            if (enabled)
                timer1.Start();
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        BeeControl control = null;
        private void button1_Click(object sender, EventArgs e)
        {
            if (control == null)
            {
                control = new BeeControl() { Location = new Point(90, 240)};
                Controls.Add(control);
            } else
            {
                using (control)
                {
                    Controls.Remove(control);
                }
            }
        }
    }
}
