using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exam
{
    public enum SecurityLevel
    {
        Confidential,
        Secret,
        TopSecret
    }
    public class Agent
    {
        public SecurityLevel SecurityLevel { get; private set; }
        public string Name { get; private set; }

        public Agent(SecurityLevel securityLevel, string name)
        {
            SecurityLevel = securityLevel;
            Name = name;
        }

        public void CallElevator(Elevator elevator, int currentFloor)
        {
            elevator.RequestElevator(currentFloor, this);
        }
    }
    public class Elevator
    {
        private int currentFloor = 0;
        private readonly object lockObj = new object();
        private bool[] floorRequests = new bool[4];
        private Agent currentAgent = null;

        public void RequestElevator(int floor, Agent agent)
        {
            lock (lockObj)
            {
                floorRequests[floor] = true;
                Monitor.PulseAll(lockObj);
            }
        }

        public void Move()
        {
            while (true)
            {
                lock (lockObj)
                {
                    // Wait for a request.
                    while (!floorRequests.Contains(true))
                    {
                        Monitor.Wait(lockObj);
                    }

                    // Move to the requested floor.
                    for (int i = 0; i < floorRequests.Length; i++)
                    {
                        if (floorRequests[i])
                        {
                            Console.WriteLine($"Elevator moving to floor {i}.");
                            Thread.Sleep(Math.Abs(i - currentFloor) * 1000); // Simulate travel time.
                            currentFloor = i;
                            floorRequests[i] = false;
                            CheckSecurityAndOpenDoor();
                            break;
                        }
                    }
                }
            }
        }

        private void CheckSecurityAndOpenDoor()
        {
            if (currentAgent == null)
            {
                Console.WriteLine("No agent in the elevator.");
                return;
            }

            if (CanAccessFloor(currentAgent, currentFloor))
            {
                Console.WriteLine($"Door opens at floor {currentFloor} for agent {currentAgent.Name}.");
            }
            else
            {
                Console.WriteLine($"Agent {currentAgent.Name} does not have access to floor {currentFloor}.");
            }
        }

        private bool CanAccessFloor(Agent agent, int floor)
        {
            switch (floor)
            {
                case 0:
                    return true;
                case 1:
                    return agent.SecurityLevel >= SecurityLevel.Secret;
                case 2:
                    return agent.SecurityLevel >= SecurityLevel.TopSecret;
                case 3:
                    return agent.SecurityLevel == SecurityLevel.TopSecret;
                default:
                    return false;
            }
        }

        public void EnterElevator(Agent agent)
        {
            lock (lockObj)
            {
                currentAgent = agent;
                Console.WriteLine($"Agent {agent.Name} entered the elevator.");
            }
        }

        public void ExitElevator()
        {
            lock (lockObj)
            {
                Console.WriteLine($"Agent {currentAgent.Name} exited the elevator.");
                currentAgent = null;
            }
        }
    }
    public class ElevatorSystem
    {
        private Elevator elevator;
        private List<Thread> agentThreads = new List<Thread>();

        public ElevatorSystem()
        {
            elevator = new Elevator();
            Thread elevatorThread = new Thread(elevator.Move);
            elevatorThread.Start();
        }

        public void AddAgent(Agent agent)
        {
            Thread agentThread = new Thread(() => SimulateAgent(agent));
            agentThreads.Add(agentThread);
            agentThread.Start();
        }

        private void SimulateAgent(Agent agent)
        {
            Random rand = new Random();
            while (true)
            {
                int currentFloor = rand.Next(0, 4);
                Console.WriteLine($"Agent {agent.Name} is on floor {currentFloor}.");
                agent.CallElevator(elevator, currentFloor);
                Thread.Sleep(rand.Next(1000, 5000)); // Simulate agent's actions.
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ElevatorSystem system = new ElevatorSystem();

            system.AddAgent(new Agent(SecurityLevel.Confidential, "Agent A"));
            system.AddAgent(new Agent(SecurityLevel.Secret, "Agent B"));
            system.AddAgent(new Agent(SecurityLevel.TopSecret, "Agent C"));

            // Keep the main thread alive infinitely.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
