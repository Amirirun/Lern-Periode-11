using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gymmmm.Models;

public class ExerciseProgress
{
    public string Name { get; set; } = "";

    public int TimesPerformed { get; set; }

    public double LastWeight { get; set; }

    public double MaxWeight { get; set; }
}