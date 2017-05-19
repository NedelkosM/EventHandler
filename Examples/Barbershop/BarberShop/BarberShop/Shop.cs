using System;
using System.Collections.Generic;
using System.Text;

namespace BarberShop {

    public class Shop {
        public delegate void CustomerEnter(Customer customer);
        public delegate void FinishedTreating(Customer customer);
        public delegate void FinishedCutting(Customer customer);
        public delegate void CustomerExit(Customer customer);
        public delegate void BarberVacancy(Barber barber);
        public delegate void AssistantVacancy(Assistant assistant);

        private EventHandler Events { get; }

        private object customerLock = new object();
        private readonly List<Customer> customers;
        private readonly Market market;

        private int capacity;
        public int Capacity { get { return capacity; } }
        public int MaxCapacity { get; }

        private volatile static int idCount = 1;
        public int Id { get; }

        public Shop(Market market, int maxCapacity = 5) {
            Id = idCount++;
            Events = new EventHandler();
            customers = new List<Customer>();
            MaxCapacity = Math.Max(maxCapacity, 0);
            capacity = MaxCapacity;
            this.market = market;
            market.Subscribe<Market.NewCustomer>(NewCustomer);
            Events.Subscribe<FinishedCutting>(FinishedCuttingCustomer);
            Events.Subscribe<AssistantVacancy>(AvailableAssistant);
            Events.Subscribe<BarberVacancy>(AvailableBarber);
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


        private void AvailableBarber(Barber barber) {
            barber.AssignCustomer(FetchCustomerForCutting());
        }

        private void AvailableAssistant(Assistant assistant) {
            assistant.AssignCustomer(FetchCustomerForTreating());
        }

        public void NewCustomer(Customer customer) {
            ReceiveCustomer(customer);
        }

        public bool ReceiveCustomer(Customer customer) {
            if (capacity > 0 && customer.Welcome()) {
                capacity--;
                Events.Trigger<CustomerEnter>(customer);
                if (customer.State == Customer.CustomerState.Waiting) { // All assistants are busy
                    lock (customerLock) {
                        customers.Add(customer);
                    }
                }
                return true;
            }
            return false;
        }

        private Customer FetchCustomerForCutting() {
            lock (customerLock) {
                return customers.Find(c => c.State == Customer.CustomerState.Ready);
            }
        }

        private Customer FetchCustomerForTreating() {
            lock (customerLock) {
                return customers.Find(c => c.State == Customer.CustomerState.Waiting);
            }
        }

        private void FinishedCuttingCustomer(Customer customer) {
            lock (customerLock) {
                customers.Remove(customer);
            }
            Events.Trigger<CustomerExit>(customer);
            capacity = Math.Min(capacity+1, MaxCapacity);
            var nextCustomer = market.FetchNextCustomer();
            if (nextCustomer != null) {
                ReceiveCustomer(nextCustomer);
            }
        }

        public void HireBarber(double speed = 2.0) {
            new Barber(this,Events, speed);
        }

        public void HireAssistant() {
            new Assistant(this, Events);
        }
    }
}
