using CommunityToolkit.Mvvm.Input;
using gymmmm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace gymmmm.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string trainingName = "";
    private string newExercise = "";
    private string newWeight = "";
    private string newSets = "";
    private string newRepetitions = "";
    private string statusMessage = "";

    // Suchfeld
    private string searchExercise = "";

    // Datum des Trainings
    private DateTimeOffset? trainingDate = DateTimeOffset.Now;

    private Training? selectedTraining;
    private Exercise? selectedExercise;

    private int editingTrainingIndex = -1;

    private readonly string filePath;


    // ------------------------------------------------
    // EINGABEFELDER
    // ------------------------------------------------

    public string TrainingName
    {
        get => trainingName;
        set => SetProperty(ref trainingName, value);
    }


    public DateTimeOffset? TrainingDate
    {
        get => trainingDate;
        set => SetProperty(ref trainingDate, value);
    }


    public string NewExercise
    {
        get => newExercise;
        set => SetProperty(ref newExercise, value);
    }


    public string NewWeight
    {
        get => newWeight;
        set => SetProperty(ref newWeight, value);
    }


    public string NewSets
    {
        get => newSets;
        set => SetProperty(ref newSets, value);
    }


    public string NewRepetitions
    {
        get => newRepetitions;
        set => SetProperty(ref newRepetitions, value);
    }


    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }


    // ------------------------------------------------
    // SUCHE
    // ------------------------------------------------

    public string SearchExercise
    {
        get => searchExercise;

        set
        {
            SetProperty(ref searchExercise, value);

            // Bei jeder Änderung im Suchfeld
            // werden die Ergebnisse aktualisiert
            UpdateSearchResults();
        }
    }


    // ------------------------------------------------
    // AUSWAHL
    // ------------------------------------------------

    public Training? SelectedTraining
    {
        get => selectedTraining;
        set => SetProperty(ref selectedTraining, value);
    }


    public Exercise? SelectedExercise
    {
        get => selectedExercise;
        set => SetProperty(ref selectedExercise, value);
    }


    // ------------------------------------------------
    // LISTEN
    // ------------------------------------------------

    public ObservableCollection<Exercise> Exercises { get; } = new();

    public ObservableCollection<Training> SavedTrainings { get; } = new();

    public ObservableCollection<ExerciseProgress> ExerciseProgresses { get; } = new();

    // Trainings, die zur gesuchten Übung passen
    public ObservableCollection<Training> SearchResults { get; } = new();


    // ------------------------------------------------
    // COMMANDS
    // ------------------------------------------------

    public IRelayCommand AddExerciseCommand { get; }

    public IRelayCommand DeleteExerciseCommand { get; }

    public IRelayCommand CreateTrainingCommand { get; }

    public IRelayCommand EditTrainingCommand { get; }

    public IRelayCommand DeleteTrainingCommand { get; }


    // ------------------------------------------------
    // KONSTRUKTOR
    // ------------------------------------------------

    public MainWindowViewModel()
    {
        string folderPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "GymTrack");

        Directory.CreateDirectory(folderPath);

        filePath = Path.Combine(
            folderPath,
            "trainings.json");


        AddExerciseCommand =
            new RelayCommand(AddExercise);

        DeleteExerciseCommand =
            new RelayCommand(DeleteExercise);

        CreateTrainingCommand =
            new RelayCommand(CreateTraining);

        EditTrainingCommand =
            new RelayCommand(EditTraining);

        DeleteTrainingCommand =
            new RelayCommand(DeleteTraining);


        LoadTrainings();

        // Trainings direkt nach dem Laden sortieren
        SortSavedTrainingsByDate();

        UpdateExerciseProgress();

        UpdateSearchResults();
    }


    // ------------------------------------------------
    // ÜBUNG HINZUFÜGEN
    // ------------------------------------------------

    private void AddExercise()
    {
        if (string.IsNullOrWhiteSpace(NewExercise))
        {
            StatusMessage =
                "Bitte gib eine Übung ein.";

            return;
        }


        if (!double.TryParse(NewWeight, out double weight))
        {
            StatusMessage =
                "Bitte gib ein gültiges Gewicht ein.";

            return;
        }


        if (!int.TryParse(NewSets, out int sets))
        {
            StatusMessage =
                "Bitte gib eine gültige Anzahl Sätze ein.";

            return;
        }


        if (!int.TryParse(NewRepetitions, out int repetitions))
        {
            StatusMessage =
                "Bitte gib gültige Wiederholungen ein.";

            return;
        }


        if (weight < 0 ||
            sets <= 0 ||
            repetitions <= 0)
        {
            StatusMessage =
                "Bitte gib sinnvolle Trainingswerte ein.";

            return;
        }


        Exercise exercise = new Exercise
        {
            Name = NewExercise,
            Weight = weight,
            Sets = sets,
            Repetitions = repetitions
        };


        Exercises.Add(exercise);


        NewExercise = "";
        NewWeight = "";
        NewSets = "";
        NewRepetitions = "";


        StatusMessage =
            "Übung wurde hinzugefügt.";
    }


    // ------------------------------------------------
    // ÜBUNG LÖSCHEN
    // ------------------------------------------------

    private void DeleteExercise()
    {
        if (SelectedExercise == null)
        {
            StatusMessage =
                "Bitte wähle zuerst eine Übung aus.";

            return;
        }


        string exerciseName =
            SelectedExercise.Name;


        Exercises.Remove(
            SelectedExercise);


        SelectedExercise = null;


        StatusMessage =
            $"Übung '{exerciseName}' wurde gelöscht.";
    }


    // ------------------------------------------------
    // TRAINING SPEICHERN
    // ------------------------------------------------

    private void CreateTraining()
    {
        if (string.IsNullOrWhiteSpace(TrainingName))
        {
            StatusMessage =
                "Bitte gib einen Namen für das Training ein.";

            return;
        }


        if (Exercises.Count == 0)
        {
            StatusMessage =
                "Bitte füge mindestens eine Übung hinzu.";

            return;
        }


        Training training = new Training
        {
            Name = TrainingName,

            Date = TrainingDate ?? DateTimeOffset.Now,

            Exercises = Exercises
                .Select(exercise => new Exercise
                {
                    Name = exercise.Name,
                    Weight = exercise.Weight,
                    Sets = exercise.Sets,
                    Repetitions = exercise.Repetitions
                })
                .ToList()
        };


        // Bestehendes Training bearbeiten
        if (editingTrainingIndex >= 0)
        {
            SavedTrainings[editingTrainingIndex] =
                training;

            StatusMessage =
                $"Training '{TrainingName}' wurde geändert.";

            editingTrainingIndex = -1;
        }

        // Neues Training
        else
        {
            SavedTrainings.Add(training);

            StatusMessage =
                $"Training '{TrainingName}' wurde gespeichert.";
        }


        // Nach Datum sortieren
        SortSavedTrainingsByDate();

        SaveTrainings();

        UpdateExerciseProgress();

        UpdateSearchResults();


        TrainingName = "";

        TrainingDate =
            DateTimeOffset.Now;

        Exercises.Clear();

        SelectedExercise = null;

        SelectedTraining = training;
    }


    // ------------------------------------------------
    // TRAINING BEARBEITEN
    // ------------------------------------------------

    private void EditTraining()
    {
        if (SelectedTraining == null)
        {
            StatusMessage =
                "Bitte wähle zuerst ein Training aus.";

            return;
        }


        editingTrainingIndex =
            SavedTrainings.IndexOf(
                SelectedTraining);


        TrainingName =
            SelectedTraining.Name;


        TrainingDate =
            SelectedTraining.Date;


        Exercises.Clear();


        foreach (Exercise exercise
                 in SelectedTraining.Exercises)
        {
            Exercises.Add(
                new Exercise
                {
                    Name = exercise.Name,
                    Weight = exercise.Weight,
                    Sets = exercise.Sets,
                    Repetitions = exercise.Repetitions
                });
        }


        SelectedExercise = null;


        StatusMessage =
            "Training wurde zum Bearbeiten geladen.";
    }


    // ------------------------------------------------
    // TRAINING LÖSCHEN
    // ------------------------------------------------

    private void DeleteTraining()
    {
        if (SelectedTraining == null)
        {
            StatusMessage =
                "Bitte wähle zuerst ein Training aus.";

            return;
        }


        string name =
            SelectedTraining.Name;


        int deletedIndex =
            SavedTrainings.IndexOf(
                SelectedTraining);


        SavedTrainings.Remove(
            SelectedTraining);


        SelectedTraining = null;


        if (editingTrainingIndex == deletedIndex)
        {
            editingTrainingIndex = -1;

            TrainingName = "";

            TrainingDate =
                DateTimeOffset.Now;

            Exercises.Clear();

            SelectedExercise = null;
        }


        SaveTrainings();

        UpdateExerciseProgress();

        UpdateSearchResults();


        StatusMessage =
            $"Training '{name}' wurde gelöscht.";
    }


    // ------------------------------------------------
    // NEU:
    // TRAININGS NACH DATUM SORTIEREN
    // ------------------------------------------------

    private void SortSavedTrainingsByDate()
    {
        // Neueste Trainings zuerst
        List<Training> sortedTrainings =
            SavedTrainings
                .OrderByDescending(
                    training => training.Date)
                .ToList();


        SavedTrainings.Clear();


        foreach (Training training
                 in sortedTrainings)
        {
            SavedTrainings.Add(training);
        }
    }


    // ------------------------------------------------
    // NEU:
    // NACH EINER ÜBUNG SUCHEN
    // ------------------------------------------------

    private void UpdateSearchResults()
    {
        SearchResults.Clear();


        // Wenn nichts eingegeben wurde,
        // soll auch nichts gesucht werden
        if (string.IsNullOrWhiteSpace(SearchExercise))
        {
            return;
        }


        foreach (Training training
                 in SavedTrainings)
        {
            // Prüfen, ob irgendeine Übung
            // den Suchbegriff enthält
            bool containsExercise =
                training.Exercises.Any(
                    exercise =>
                        exercise.Name.Contains(
                            SearchExercise,
                            StringComparison.OrdinalIgnoreCase));


            if (containsExercise)
            {
                SearchResults.Add(training);
            }
        }
    }


    // ------------------------------------------------
    // JSON SPEICHERN
    // ------------------------------------------------

    private void SaveTrainings()
    {
        try
        {
            JsonSerializerOptions options =
                new JsonSerializerOptions
                {
                    WriteIndented = true
                };


            string json =
                JsonSerializer.Serialize(
                    SavedTrainings,
                    options);


            File.WriteAllText(
                filePath,
                json);
        }
        catch (Exception ex)
        {
            StatusMessage =
                "Fehler beim Speichern: "
                + ex.Message;
        }
    }


    // ------------------------------------------------
    // JSON LADEN
    // ------------------------------------------------

    private void LoadTrainings()
    {
        if (!File.Exists(filePath))
        {
            return;
        }


        try
        {
            string json =
                File.ReadAllText(filePath);


            List<Training>? trainings =
                JsonSerializer.Deserialize<List<Training>>(
                    json);


            if (trainings == null)
            {
                return;
            }


            SavedTrainings.Clear();


            foreach (Training training
                     in trainings)
            {
                SavedTrainings.Add(training);
            }
        }
        catch (Exception ex)
        {
            StatusMessage =
                "Fehler beim Laden: "
                + ex.Message;
        }
    }


    // ------------------------------------------------
    // TRAININGSFORTSCHRITT
    // ------------------------------------------------

    private void UpdateExerciseProgress()
    {
        ExerciseProgresses.Clear();


        List<Exercise> allExercises =
            SavedTrainings
                .SelectMany(
                    training => training.Exercises)
                .ToList();


        var groups =
            allExercises.GroupBy(
                exercise => exercise.Name.Trim(),
                StringComparer.OrdinalIgnoreCase);


        foreach (var group in groups)
        {
            List<Exercise> exercises =
                group.ToList();


            ExerciseProgress progress =
                new ExerciseProgress
                {
                    Name = group.Key,

                    TimesPerformed =
                        exercises.Count,

                    LastWeight =
                        exercises.Last().Weight,

                    MaxWeight =
                        exercises.Max(
                            exercise =>
                                exercise.Weight)
                };


            ExerciseProgresses.Add(progress);
        }
    }
}