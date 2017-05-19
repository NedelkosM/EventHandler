using System;
using System.Collections.Generic;
using System.Text;

namespace BarberShop {
    public class Customer {
        public enum CustomerState {
            Entering = 0,
            Waiting = 1,
            Treated = 2,
            Ready = 3,
            Cutting = 4,
            Leaving = 5
        }
        private volatile static int idCount = 1;
        public int Id { get; }

        private CustomerState state;
        public CustomerState State { get { return state; } }
        private double hairAmount;
        private double HairAmount { get { return hairAmount; } }
        private double TargetHairAmount { get; }
        private bool treated;
        public bool Treated { get { return treated; } }

        public Customer(double hairAmount = 10.0, double targetHairAmount = 3.0) {
            Id = idCount++;
            this.hairAmount = Math.Max(hairAmount, 0.1); // At least some hair (sorry bald guys)
            TargetHairAmount = Math.Min(Math.Max(targetHairAmount, 0), hairAmount); // Between 0 and current hair amount (can't grow hair, sorry)
            state = CustomerState.Entering;
        }

        public bool Welcome() {
            if (state == CustomerState.Entering) {
                state = CustomerState.Waiting;
                return true;
            }
            return false;
        }

        public bool Treat() {
            if (state == CustomerState.Waiting && !Treated) {
                state = CustomerState.Treated;
                treated = true;
                return true;
            }
            return false;
        }

        public void Ready() {
            if (state == CustomerState.Treated) {
                state = CustomerState.Ready;
            }
        }

        public bool BarberGreetings() {
            if (state == CustomerState.Ready) {
                state = CustomerState.Cutting;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cut customer hair by a certain amount.
        /// </summary>
        /// <param name="amount">Amount of hair to cut</param>
        /// <returns>If customer wants more cutting</returns>
        public bool CutHair(double amount) {
            if (state != CustomerState.Cutting) {
                return false;
            }
            hairAmount = Math.Max(hairAmount - amount, 0);
            if (hairAmount <= TargetHairAmount) {
                state = CustomerState.Leaving;
                return true;
            }
            return false;
        }
    }
}
