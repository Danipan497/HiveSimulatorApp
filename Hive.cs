using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HiveSimulator
{
    [Serializable]
    public class Hive
    {
        private World world;

        private const int InitialBees = 6;
        private const double InitialHoney = 3.2;
        private const double MaximumHoney = 15.0;
        private const double NectarHoneyRatio = .25;
        private const double MinimumHoneyForCreatingBees = 4.0;
        private const int MaximumBees = 8;

        private Dictionary<string, Point> locations;
        private int beeCount = 0;

        public double Honey { get; private set; }

        public Bee.BeeMessage MessageSender;


        private void InitializeLocations()
        {
            locations = new Dictionary<string, Point>();
            locations.Add("Wejście", new Point(1321, 297));
            locations.Add("Żłobek", new Point(95, 174));
            locations.Add("Fabryka miodu", new Point(157, 98));
            locations.Add("Wyjście", new Point(411, 398));
        }

        public Point GetLocation(string location)
        {
            if (locations.Keys.Contains(location))
                return locations[location];
            else
                throw new ArgumentException("Nieznana lokalizacja: " + location);
        }

        public Hive(World world, Bee.BeeMessage MessageSender)
        {
            this.MessageSender = MessageSender;
            this.world = world;

            Honey = InitialHoney;
            InitializeLocations();
            Random random = new Random();
            for (int i = 0; i < InitialBees; i++)
                AddBee(random);
        }

        public bool AddHoney(double nectar)
        {
            double honeyToAdd = nectar * NectarHoneyRatio;
            if (honeyToAdd + Honey > MaximumHoney)
                return false;
            Honey += honeyToAdd;
            return true;
        }

        public bool ConsumeHoney(double Amount)
        {
            if (Amount > Honey)
                return false;
            else
            {
                Honey -= Amount;
                return true;
            }
        }
        private void AddBee(Random random)
        {
            beeCount++;
            int r1 = random.Next(100) - 50;
            int r2 = random.Next(100) - 50;
            Point startPoint = new Point(locations["Żłobek"].X + r1,
                                         locations["Żłobek"].Y + r2);
            Bee newBee = new Bee(beeCount, startPoint, world, this);
            newBee.MessageSender += this.MessageSender;
            world.Bees.Add(newBee);
        }

        public void Go(Random random)
        {
            if (world.Bees.Count < MaximumBees
            && Honey > MinimumHoneyForCreatingBees
            && random.Next(10) == 1)
            {
                AddBee(random);
            }
        }

    }
}
