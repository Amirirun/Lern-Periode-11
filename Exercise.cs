using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gymmmm.Models;

public class Exercise
{
    public string Name { get; set; } = "";

    public double Weight { get; set; }

    public int Sets { get; set; }

    public int Repetitions { get; set; }
}