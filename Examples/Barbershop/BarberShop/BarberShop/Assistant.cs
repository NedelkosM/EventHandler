using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BarberShop
{
    public class Assistant
    {
        public enum AssistantState {
            Waiting,
            TreatingCustomer
        }
        private volatile static int idCount = 1;
        public int Id { get; }

        private volatile AssistantState state;
        public AssistantState State { get { return state; } }

        private Shop shop;
        private EventHandler shopEvents;
        private object stateLock = new object();
        private volatile Customer currentCustomer;

        public Assistant(Shop shop, EventHandler shopEvents) {
            Id = idCount++;
            this.shop = shop;
            this.shopEvents = shopEvents;
            state = AssistantState.Waiting;
            shop.Subscribe<Shop.CustomerEnter>(CustomerWalkin);
        }

        public void AssignCustomer(Customer customer) {
            lock (stateLock) {
                if (customer != null && state == AssistantState.Waiting && customer.Treat()) {
                    currentCustomer = customer;
                    state = AssistantState.TreatingCustomer;
                    var thread = new Thread(TreatCustomer);
                    thread.Start();
                }
            }
        }

        private void CustomerWalkin(Customer customer) {
            AssignCustomer(customer);
        }

        private void TreatCustomer() {
            //Console.WriteLine($"Assistant {Id} treating customer {currentCustomer.Id}");
            Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(2000, 10000));
            FinishedTreating();
        }

        private void FinishedTreating() {
            lock (stateLock) {
                currentCustomer.Ready();
                state = AssistantState.Waiting;
                shopEvents.Trigger<Shop.FinishedTreating>(currentCustomer);
                currentCustomer = null;
                shopEvents.Trigger<Shop.AssistantVacancy>(this);
            }
        }
    }
}
