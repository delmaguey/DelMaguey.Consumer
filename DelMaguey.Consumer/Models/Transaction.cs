using System;
using System.Collections.Generic;

namespace DelMaguey.Consumer.Models;

public partial class Transaction
{
    public int TransId { get; set; }

    public DateTime TransDateTransTime { get; set; }

    public long CcNum { get; set; }

    public string Merchant { get; set; } = null!;

    public string Category { get; set; } = null!;

    public double Amt { get; set; }

    public string First { get; set; } = null!;

    public string Last { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public int Zip { get; set; }

    public double Lat { get; set; }

    public double Long { get; set; }

    public int CityPop { get; set; }

    public string Job { get; set; } = null!;

    public DateOnly Dob { get; set; }

    public string TransNum { get; set; } = null!;

    public int UnixTime { get; set; }

    public double MerchLat { get; set; }

    public double MerchLong { get; set; }

    public byte IsFraud { get; set; }

    public string Email { get; set; } = null!;
}
