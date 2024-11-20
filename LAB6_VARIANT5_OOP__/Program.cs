using System;
using System.Collections.Generic;
using System.Linq;

namespace AirportMonitoring
{
    public enum FlightStatus
    {
        Очікується,
        Посадка,
        Відправлено,
        Затримано,
        Скасовано
    }

    public record Flight(string Destination, bool IsVIP)
    {
        public string Id { get; } = Guid.NewGuid().ToString(); // Унікальний ідентифікатор рейсу
        public FlightStatus Status { get; private set; } = FlightStatus.Очікується;

        public event Action<Flight, FlightStatus>? StatusChanged;

        public void ChangeStatus(FlightStatus newStatus)
        {
            if (Status != newStatus)
            {
                Status = newStatus;
                StatusChanged?.Invoke(this, newStatus);
            }
        }
    }

    public record Passenger(string Name)
    {
        public void Notify(Flight flight, FlightStatus status)
        {
            string message = status switch
            {
                FlightStatus.Очікується => $"Пасажир {Name}: Ваш рейс до {flight.Destination} очікується.",
                FlightStatus.Посадка => $"Пасажир {Name}: Почалася посадка на рейс до {flight.Destination}.",
                FlightStatus.Відправлено => $"Пасажир {Name}: Ваш рейс до {flight.Destination} відправлено.",
                FlightStatus.Затримано => $"Пасажир {Name}: Ваш рейс до {flight.Destination} затримано.",
                FlightStatus.Скасовано => $"Пасажир {Name}: Ваш рейс до {flight.Destination} скасовано.",
                _ => $"Пасажир {Name}: Статус рейсу до {flight.Destination} оновлено."
            };
            Console.WriteLine(message);
        }
    }

    public class Airport
    {
        private List<Flight> Flights { get; } = new();
        private Dictionary<string, List<Passenger>> FlightPassengers { get; } = new(); // Словник за Id рейсу

        public void AddFlight(string destination, bool isVIP)
        {
            var flight = new Flight(destination, isVIP);

            Flights.Add(flight);
            FlightPassengers[flight.Id] = new List<Passenger>();

            Console.WriteLine($"Рейс до {destination} додано.");
        }

        public void RegisterPassenger(string passengerName, int flightIndex)
        {
            if (flightIndex < 0 || flightIndex >= Flights.Count)
            {
                Console.WriteLine("Невірний вибір рейсу.");
                return;
            }

            var flight = Flights[flightIndex];

            if (flight.Status == FlightStatus.Відправлено || flight.Status == FlightStatus.Скасовано)
            {
                Console.WriteLine($"Реєстрація неможлива: рейс до {flight.Destination} вже {flight.Status.ToString().ToLower()}.");
                return;
            }

            var passenger = new Passenger(passengerName);
            FlightPassengers[flight.Id].Add(passenger);
            flight.StatusChanged += passenger.Notify;

            Console.WriteLine($"Пасажир {passenger.Name} зареєстрований на рейс до {flight.Destination}.");
        }

        public void ChangeFlightStatus(int flightIndex, FlightStatus newStatus)
        {
            if (flightIndex < 0 || flightIndex >= Flights.Count)
            {
                Console.WriteLine("Невірний вибір рейсу.");
                return;
            }

            var flight = Flights[flightIndex];

            // Логіка перевірки допустимості переходу
            bool isValidTransition = flight.Status switch
            {
                FlightStatus.Очікується => newStatus == FlightStatus.Посадка || newStatus == FlightStatus.Скасовано,
                FlightStatus.Посадка => newStatus == FlightStatus.Відправлено || newStatus == FlightStatus.Скасовано,
                FlightStatus.Затримано => newStatus == FlightStatus.Посадка || newStatus == FlightStatus.Скасовано,
                _ => false
            };

            if (!isValidTransition)
            {
                Console.WriteLine($"Недопустимий перехід: неможливо змінити статус рейсу з \"{flight.Status}\" на \"{newStatus}\".");
                return;
            }

            flight.ChangeStatus(newStatus);
        }

        public void DisplayPassengersForFlight(int flightIndex)
        {
            if (flightIndex < 0 || flightIndex >= Flights.Count)
            {
                Console.WriteLine("Невірний вибір рейсу.");
                return;
            }

            var flight = Flights[flightIndex];

            Console.WriteLine($"\nСписок пасажирів рейсу до {flight.Destination}:");
            if (FlightPassengers.TryGetValue(flight.Id, out var passengers) && passengers.Any())
            {
                foreach (var passenger in passengers)
                {
                    Console.WriteLine($"- {passenger.Name}");
                }
            }
            else
            {
                Console.WriteLine("Немає зареєстрованих пасажирів.");
            }
        }

        public void DisplayFlights()
        {
            Console.WriteLine("\nСписок рейсів:");
            for (int i = 0; i < Flights.Count; i++)
            {
                var flight = Flights[i];
                Console.WriteLine($"{i + 1}. {flight.Destination} - {flight.Status} {(flight.IsVIP ? "(VIP)" : "")}");
            }
        }

        public void DisplayStatistics()
        {
            Console.WriteLine("\nСтатистика рейсів:");
            Console.WriteLine($"Завершених рейсів: {Flights.Count(f => f.Status == FlightStatus.Відправлено)}");
            Console.WriteLine($"Затриманих рейсів: {Flights.Count(f => f.Status == FlightStatus.Затримано)}");
            Console.WriteLine($"Скасованих рейсів: {Flights.Count(f => f.Status == FlightStatus.Скасовано)}");
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var airport = new Airport();

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Додати рейс");
                Console.WriteLine("2. Зареєструвати пасажира");
                Console.WriteLine("3. Змінити статус рейсу");
                Console.WriteLine("4. Показати рейси");
                Console.WriteLine("5. Показати статистику");
                Console.WriteLine("6. Показати список пасажирів для рейсу");
                Console.WriteLine("7. Завершити симуляцію");

                Console.Write("Ваш вибір: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Введіть напрямок рейсу: ");
                        var destination = Console.ReadLine();
                        Console.Write("Це VIP-рейс? (y/n): ");
                        var isVIP = Console.ReadLine()?.ToLower() == "y";
                        airport.AddFlight(destination!, isVIP);
                        break;

                    case "2":
                        airport.DisplayFlights();
                        Console.Write("Введіть ім'я пасажира: ");
                        var passengerName = Console.ReadLine();
                        Console.Write("Виберіть номер рейсу: ");
                        if (int.TryParse(Console.ReadLine(), out var flightIndex1))
                        {
                            airport.RegisterPassenger(passengerName!, flightIndex1 - 1);
                        }
                        else
                        {
                            Console.WriteLine("Невірний номер рейсу.");
                        }
                        break;

                    case "3":
                        airport.DisplayFlights();
                        Console.Write("Виберіть номер рейсу: ");
                        if (int.TryParse(Console.ReadLine(), out var flightIndex2))
                        {
                            Console.WriteLine("Доступні статуси:");
                            foreach (var status in Enum.GetValues<FlightStatus>())
                            {
                                Console.WriteLine($"{(int)status}. {status}");
                            }

                            Console.Write("Виберіть новий статус: ");
                            if (int.TryParse(Console.ReadLine(), out var statusIndex) && Enum.IsDefined(typeof(FlightStatus), statusIndex))
                            {
                                airport.ChangeFlightStatus(flightIndex2 - 1, (FlightStatus)statusIndex);
                            }
                            else
                            {
                                Console.WriteLine("Невірний вибір статусу.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Невірний номер рейсу.");
                        }
                        break;

                    case "4":
                        airport.DisplayFlights();
                        break;

                    case "5":
                        airport.DisplayStatistics();
                        break;

                    case "6":
                        airport.DisplayFlights();
                        Console.Write("Виберіть номер рейсу: ");
                        if (int.TryParse(Console.ReadLine(), out var flightIndex3))
                        {
                            airport.DisplayPassengersForFlight(flightIndex3 - 1);
                        }
                        else
                        {
                            Console.WriteLine("Невірний номер рейсу.");
                        }
                        break;

                    case "7":
                        Console.WriteLine("Симуляцію завершено.");
                        return;

                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }
    }
}
