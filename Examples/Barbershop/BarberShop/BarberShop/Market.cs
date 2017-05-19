using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace BarberShop {
    public class Market {
        public static readonly Random random = new Random(DateTime.Now.Millisecond * DateTime.Now.Minute);

        static void Main(string[] args) {
            var reporter = new Reporter();
            var markets = new List<Market>();
            for (int i = 0; i < 5; i++) {
                var market = new Market();
                market.OpenShops(i + random.Next(2,4));
                reporter.Visit(market);
                market.OpenDoors(300000); // 5 minutes
                market.BringCustomers(random.Next(1, 3));
                markets.Add(market);
            }
            var waitThread = new Thread(new ParameterizedThreadStart(WaitMarketClose));
            waitThread.Start(markets);
            waitThread.Join();
            Console.WriteLine("All markets are now closed.");
            Console.ReadKey();
        }

        static void WaitMarketClose(object markets) {
            List<Market> marketList;
            try {
                marketList = (List<Market>) markets;
            } catch {
                return;
            }
            while(marketList.Find(m => m.Open) != null){
                Thread.Sleep(10);
            }
        }

        public delegate void NewCustomer(Customer c);

        private EventHandler Events { get; }

        private readonly List<Shop> shops;
        public Shop[] Shops { get { return shops.ToArray(); } }
        private readonly List<Customer> customers;

        private bool open;
        public bool Open { get { return open; } }

        private volatile static int idCount = 1;
        public int Id { get; }

        public Market() {
            Id = idCount++;
            Events = new EventHandler();
            shops = new List<Shop>();
            customers = new List<Customer>();
        }


        /// <summary>
        /// Add listener to this Shop's event.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="listener">Listener</param>
        public void Subscribe<T>(T listener) { // This is to safely expose the events
            Events.Subscribe<T>(listener);
        }

        /// <summary>
        /// Remove listener from this Shop's event.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="listener">Listener</param>
        public void Unsubscribe<T>(T listener) { // This is to safely expose the events
            Events.Unsubscribe<T>(listener);
        }

        public void StreamMarket(object timeOpen) {
            var time = 0.0;
            try {
                time = Math.Max((double)timeOpen,1);
            } catch {
                time = 120000; // 2 minutes
            }
            while(time > 0) {
                BringCustomer();
                var timeWait = random.Next(10000, 20000);
                Thread.Sleep(timeWait);
                time -= timeWait;
            }
            CloseDoors();
        }

        public void OpenShops(int amount) {
            for (int i = 0; i < amount; i++) {
                var newShop = new Shop(this, random.Next(2, 6));
                for (int k = 0; k < random.Next(1, 4); k++)
                    newShop.HireBarber(random.Next(100, 300)/100.0);
                for (int k = 0; k < random.Next(1, 4); k++)
                    newShop.HireAssistant();
                shops.Add(newShop);
            }
        }

        public void BringCustomers(int amount) {
            for (int i = 0; i < 10; i++) {
                BringCustomer();
            }
        }

        public void BringCustomer() {
            var newCustomer = new Customer(random.Next(10, 20), random.Next(12, 30));
            Events.Trigger<NewCustomer>(newCustomer);
            if (!newCustomer.Treated) {
                customers.Add(newCustomer);
            }
        }

        public void OpenDoors(double duration) {
            Console.WriteLine($"Market {Id} is now open.");
            open = true;
            var thread = new Thread(new ParameterizedThreadStart(StreamMarket));
            thread.Start(duration);
        }

        public void CloseDoors() {
            open = false;
        }

        public Customer FetchNextCustomer() {
            if (customers.Count > 0) {
                var customer = customers[0];
                customers.RemoveAt(0);
                return customer;
            } else {
                return null;
            }
        }
    }
}