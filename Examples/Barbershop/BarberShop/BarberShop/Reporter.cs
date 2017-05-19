using System;
using System.Collections.Generic;
using System.Text;

namespace BarberShop
{
    class Reporter
    {
        public Reporter() {

        }

        public void Visit(Market market) {
            foreach(var s in market.Shops) {
                s.Subscribe<Shop.CustomerEnter>(CustomerEnter);
                s.Subscribe<Shop.FinishedTreating>(FinishedTreating);
                s.Subscribe<Shop.FinishedCutting>(FinishedCutting);
                s.Subscribe<Shop.CustomerExit>(CustomerExit);
                s.Subscribe<Shop.BarberVacancy>(BarberVacancy);
                s.Subscribe<Shop.AssistantVacancy>(AssistantVacancy);
            }
            market.Subscribe<Market.NewCustomer>(NewCustomer);
        }

        private void NewCustomer(Customer customer) {
            Console.WriteLine($"Reporter: Customer #{customer.Id} enters market.");
        }

        private void CustomerEnter(Customer customer) {
            Console.WriteLine($"Reporter: Customer #{customer.Id} enters shop.");
        }

        private void FinishedTreating(Customer customer) {
            Console.WriteLine($"Reporter: Finished treating customer #{customer.Id}.");
        }

        private void FinishedCutting(Customer customer) {
            Console.WriteLine($"Reporter: Finished cutting customer #{customer.Id}.");
        }

        private void CustomerExit(Customer customer) {
            Console.WriteLine($"Reporter: Finished cutting customer #{customer.Id}.");

        }

        private void BarberVacancy(Barber barber) {
            Console.WriteLine($"Reporter: Barber #{barber.Id} is free.");
        }

        private void AssistantVacancy(Assistant assistant) {
            Console.WriteLine($"Reporter: Assistant #{assistant.Id} is free.");
        }
    }
}
