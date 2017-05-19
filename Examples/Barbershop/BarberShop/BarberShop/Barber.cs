using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BarberShop {

    public class Barber {
        public enum BarberState {
            Waiting,
            Cutting,
            Resting
        }
        private volatile static int idCount = 1;
        public int Id { get; }

        private volatile BarberState state;
        public BarberState State { get { return state; } }

        private double CutSpeed { get; }

        Customer currentCustomer;
        EventHandler shopEvents;

        public Barber(Shop shop, EventHandler shopEvents, double speed = 3.0) {
            Id = idCount++;
            state = BarberState.Waiting;
            CutSpeed = speed;
            this.shopEvents = shopEvents;
            shopEvents.Subscribe<Shop.FinishedTreating>(FinishedTreating);
        }

        private void FinishedTreating(Customer customer) {
            AssignCustomer(customer);
        }

        public void AssignCustomer(Customer customer) {
            if (customer != null && state == BarberState.Waiting) {
                if (customer.BarberGreetings()) {
                    TreatCustomer(customer);
                }
            }
        }

        private void CutHair() {
            do {
                Thread.Sleep(1000);
            } while (currentCustomer.CutHair(CutSpeed));
            var customer = currentCustomer;
            state = BarberState.Resting;
            shopEvents.Trigger<Shop.FinishedCutting>(customer);
            currentCustomer = null;
            Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(1000, 5000));
            state = BarberState.Waiting;
            shopEvents.Trigger<Shop.BarberVacancy>(this);
        }

        private void TreatCustomer(Customer customer) {
            if (customer != null) {
                currentCustomer = customer;
                state = BarberState.Cutting;
                var thread = new Thread(CutHair);
                thread.Start();
            }
        }
    }
}
