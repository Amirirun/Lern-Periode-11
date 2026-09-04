using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gymmmm.Models;

public class Training
{
    public string Name { get; set; } = "";

    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public List<Exercise> Exercises { get; set; } = new();

    [JsonIgnore]
    public int ExerciseCount => Exercises.Count;

    [JsonIgnore]
    public string DateText => Date.ToString("dd.MM.yyyy");
}