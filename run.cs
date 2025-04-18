using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        const string dateFormat = "yyyy-MM-dd";
        var events = new List<(DateTime date, int change)>();

        foreach (var g in guests)
        {
            var checkIn = DateTime.ParseExact(g.CheckIn, dateFormat, null);
            var checkOut = DateTime.ParseExact(g.CheckOut, dateFormat, null);

            events.Add((checkIn, +1));
            events.Add((checkOut, -1));
        }

        events.Sort();
        
        var currentOccupancy = 0;
        foreach (var e in events)
        {
            currentOccupancy += e.change;
            if (currentOccupancy > maxCapacity)
                return false;
        }

        return true;
    }
    
    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }
    
    static void Main()
    {
        var maxCapacity = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());
        var guests = new List<Guest>();

        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var guest = ParseGuest(line);
            guests.Add(guest);
        }

        var result = CheckCapacity(maxCapacity, guests);

        Console.WriteLine(result ? "True" : "False");
    }
    
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();

        var nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;

        var checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;

        var checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;

        return guest;
    }
}