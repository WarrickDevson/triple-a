using KPW.Domain.Entities;
using KPW.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KPW.Infrastructure.Data;

public static class DatabaseSeeder
{
    private const string SeedPasswordHash =
        "$argon2id$v=19$m=65536,t=3,p=1$pE6t/ab4vs6xAn1qMByDZQ$GtKjB012JDKkMeTUDEMOBROTGRMOp2ubJZIZkE06NfI";

    private const string SampleVideoUrl =
        "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4";

    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedAnchor = new(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly ProgramStart = new(2026, 1, 1);
    private static readonly DateOnly TrackingStart = new(2026, 7, 13);

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedClinics(modelBuilder);
        SeedUsers(modelBuilder);
        SeedPets(modelBuilder);
        SeedMedicalHistories(modelBuilder);
        SeedExercises(modelBuilder);
        SeedExerciseSteps(modelBuilder);
        SeedRehabPrograms(modelBuilder);
        SeedRehabProgramExercises(modelBuilder);
        SeedDailyTrackingLogs(modelBuilder);
        SeedAppointments(modelBuilder);
        SeedVideoSubmissions(modelBuilder);
        SeedMessageThreads(modelBuilder);
        SeedMessages(modelBuilder);
        SeedExerciseSessionLogs(modelBuilder);
    }

    private static void SeedClinics(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>().HasData(
            new Clinic
            {
                ClinicId = 1,
                ClinicName = "Triple A Veterinary Physiotherapy - Demo Clinic",
                VatNumber = "4123456789",
                PhysicalAddress = "123 Wellness Street, Pretoria, Gauteng",
                ContactNumber = "+27110000000",
                InviteCode = "TRIPLEA1",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Clinic
            {
                ClinicId = 2,
                ClinicName = "Triple A North Branch",
                VatNumber = "4987654321",
                PhysicalAddress = "45 Rehabilitation Road, Centurion, Gauteng",
                ContactNumber = "+27110000099",
                InviteCode = "KPWNORTH2",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                ClinicId = 1,
                Email = "sysadmin@kpw.local",
                PasswordHash = SeedPasswordHash,
                FirstName = "System",
                LastName = "Administrator",
                PhoneNumber = "+27110000001",
                UserRole = UserRole.SysAdmin,
                SubscriptionTier = SubscriptionTier.Pro,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsEmailVerified = true,
                IsActive = true
            },
            new User
            {
                UserId = 2,
                ClinicId = 1,
                Email = "physio@kpw.local",
                PasswordHash = SeedPasswordHash,
                FirstName = "Demo",
                LastName = "Physiotherapist",
                PhoneNumber = "+27110000002",
                UserRole = UserRole.Physio,
                SubscriptionTier = SubscriptionTier.Pro,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsEmailVerified = true,
                IsActive = true
            },
            new User
            {
                UserId = 3,
                ClinicId = 1,
                Email = "owner@kpw.local",
                PasswordHash = SeedPasswordHash,
                FirstName = "Demo",
                LastName = "Owner",
                PhoneNumber = "+27110000003",
                UserRole = UserRole.Owner,
                SubscriptionTier = SubscriptionTier.Free,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsEmailVerified = true,
                IsActive = true
            });
    }

    private static void SeedPets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>().HasData(
            new Pet
            {
                PetId = 1,
                OwnerId = 3,
                PetName = "Buddy",
                Species = "Canine",
                Breed = "Labrador Retriever",
                BirthDate = new DateOnly(2019, 5, 12),
                WeightKg = 28.5m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Pet
            {
                PetId = 2,
                OwnerId = 3,
                PetName = "Luna",
                Species = "Canine",
                Breed = "Border Collie",
                BirthDate = new DateOnly(2020, 3, 8),
                WeightKg = 18.2m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Pet
            {
                PetId = 3,
                OwnerId = 3,
                PetName = "Max",
                Species = "Canine",
                Breed = "German Shepherd",
                BirthDate = new DateOnly(2018, 11, 22),
                WeightKg = 34.0m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Pet
            {
                PetId = 4,
                OwnerId = 3,
                PetName = "Bella",
                Species = "Canine",
                Breed = "Beagle",
                BirthDate = new DateOnly(2017, 7, 4),
                WeightKg = 14.8m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Pet
            {
                PetId = 5,
                OwnerId = 3,
                PetName = "Whiskers",
                Species = "Feline",
                Breed = "Domestic Shorthair",
                BirthDate = new DateOnly(2016, 2, 14),
                WeightKg = 4.6m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Pet
            {
                PetId = 6,
                OwnerId = 3,
                PetName = "Milo",
                Species = "Feline",
                Breed = "Maine Coon",
                BirthDate = new DateOnly(2021, 9, 30),
                WeightKg = 5.9m,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedMedicalHistories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalHistory>().HasData(
            new MedicalHistory
            {
                MedicalHistoryId = 1,
                PetId = 1,
                Diagnosis = "Hip Dysplasia",
                InjuryOrCondition = "Mild bilateral hip dysplasia with reduced hind-limb mobility.",
                ClinicianNotes = "Begin low-impact strengthening and proprioception work.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 2,
                PetId = 2,
                Diagnosis = "Cranial Cruciate Ligament Rupture",
                InjuryOrCondition = "Right stifle CCL rupture, post-surgical repair 3 weeks ago.",
                ClinicianNotes = "Progress to controlled loading; monitor for limb favouring.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 3,
                PetId = 3,
                Diagnosis = "Chronic Lameness",
                InjuryOrCondition = "Intermittent forelimb lameness, suspected soft tissue strain.",
                ClinicianNotes = "Focus on proprioception and gradual return to activity.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 4,
                PetId = 4,
                Diagnosis = "Obesity-Related Mobility Decline",
                InjuryOrCondition = "Overweight with reduced exercise tolerance and stiff gait.",
                ClinicianNotes = "Combine weight management with low-impact mobility exercises.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 5,
                PetId = 5,
                Diagnosis = "Feline Osteoarthritis",
                InjuryOrCondition = "Bilateral elbow osteoarthritis with reduced jumping ability.",
                ClinicianNotes = "Gentle range-of-motion and environmental modification advised.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 6,
                PetId = 6,
                Diagnosis = "Post-Operative Recovery",
                InjuryOrCondition = "Abdominal surgery 10 days ago; restricted activity period.",
                ClinicianNotes = "Gradual return to movement; avoid jumping for 4 weeks.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new MedicalHistory
            {
                MedicalHistoryId = 7,
                PetId = 1,
                Diagnosis = "Prior Elbow Dysplasia",
                InjuryOrCondition = "Historical mild elbow dysplasia, managed conservatively since 2024.",
                ClinicianNotes = "Monitor during hind-limb loading exercises.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedExercises(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>().HasData(
            new Exercise
            {
                ExerciseId = 1,
                Title = "Sit-to-Stand",
                ShortDescription = "Build hind-limb strength through controlled transitions.",
                TargetedMuscles = "Gluteals, quadriceps, hamstrings",
                ClinicalPurpose = "Improve weight-bearing tolerance after hip dysplasia diagnosis.",
                SafetyNotes = "Stop if the dog shows pain, vocalises, or refuses to stand.",
                CommonMistakes = "Allowing the dog to collapse backward instead of pushing through the hind limbs.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "HipDysplasia",
                DifficultyLevel = 2,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 2,
                Title = "Passive Range of Motion",
                ShortDescription = "Gentle hip flexion and extension to maintain joint mobility.",
                TargetedMuscles = "Hip flexors, hip extensors",
                ClinicalPurpose = "Maintain joint range before active strengthening.",
                SafetyNotes = "Move slowly and stay within a pain-free range.",
                CommonMistakes = "Forcing the limb beyond comfortable flexion.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "HipDysplasia",
                DifficultyLevel = 1,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 3,
                Title = "Weight Shifting",
                ShortDescription = "Encourage controlled lateral weight transfer over the hind limbs.",
                TargetedMuscles = "Core stabilisers, gluteals",
                ClinicalPurpose = "Improve balance and proprioception during recovery.",
                SafetyNotes = "Use a non-slip surface and support the dog if needed.",
                CommonMistakes = "Moving too quickly between sides.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "HipDysplasia",
                DifficultyLevel = 2,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 4,
                Title = "Cavaletti Poles",
                ShortDescription = "Low obstacle walking to restore stifle stability and coordination.",
                TargetedMuscles = "Quadriceps, hamstrings, core",
                ClinicalPurpose = "Rebuild neuromuscular control after CCL surgery.",
                SafetyNotes = "Keep pole height low; stop if limping worsens.",
                CommonMistakes = "Poles set too high or spaced inconsistently.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "PostOperative",
                DifficultyLevel = 2,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 5,
                Title = "Slow Lead Walk",
                ShortDescription = "Controlled leash walking on varied surfaces.",
                TargetedMuscles = "Forelimb stabilisers, shoulder girdle",
                ClinicalPurpose = "Gradual return to weight-bearing after lameness.",
                SafetyNotes = "Avoid slippery floors; keep sessions short.",
                CommonMistakes = "Allowing the dog to pull or trot before ready.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "Lameness",
                DifficultyLevel = 1,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 6,
                Title = "Incline Walk",
                ShortDescription = "Gentle uphill walking to build endurance without high impact.",
                TargetedMuscles = "Hind-limb extensors, cardiovascular system",
                ClinicalPurpose = "Support weight management and mobility improvement.",
                SafetyNotes = "Use a mild incline only; monitor breathing rate.",
                CommonMistakes = "Choosing slopes that are too steep.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Canine",
                ConditionCategory = "WeightManagement",
                DifficultyLevel = 2,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 7,
                Title = "Gentle Stretch",
                ShortDescription = "Slow elbow and shoulder flexion for feline arthritis.",
                TargetedMuscles = "Shoulder flexors, elbow extensors",
                ClinicalPurpose = "Maintain joint mobility in arthritic cats.",
                SafetyNotes = "Keep sessions under 5 minutes; reward calm behaviour.",
                CommonMistakes = "Restraining too firmly causing stress.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Feline",
                ConditionCategory = "Arthritis",
                DifficultyLevel = 1,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Exercise
            {
                ExerciseId = 8,
                Title = "Crate Rest Transitions",
                ShortDescription = "Controlled in-and-out crate movements post-surgery.",
                TargetedMuscles = "Core, hind-limb flexors",
                ClinicalPurpose = "Safe reintroduction to movement after abdominal surgery.",
                SafetyNotes = "No jumping onto or off furniture.",
                CommonMistakes = "Rushing the transition before the incision has healed.",
                VideoUrl = SampleVideoUrl,
                TargetSpecies = "Feline",
                ConditionCategory = "PostOperative",
                DifficultyLevel = 1,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedExerciseSteps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseStep>().HasData(
            new ExerciseStep { ExerciseStepId = 1, ExerciseId = 1, StepNumber = 1, StepInstruction = "Position your dog on a non-slip mat with hind limbs square.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 2, ExerciseId = 1, StepNumber = 2, StepInstruction = "Lure the dog into a controlled sit, keeping the spine neutral.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 3, ExerciseId = 1, StepNumber = 3, StepInstruction = "Cue a slow stand using a treat, pausing briefly at the top before repeating.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 4, ExerciseId = 2, StepNumber = 1, StepInstruction = "Support the limb gently and flex the hip slowly for 3 seconds.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 5, ExerciseId = 2, StepNumber = 2, StepInstruction = "Return to neutral, then extend the hip within a comfortable range.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 6, ExerciseId = 3, StepNumber = 1, StepInstruction = "Stand beside your dog and gently shift weight toward one hind limb.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 7, ExerciseId = 3, StepNumber = 2, StepInstruction = "Hold for 2 seconds, then shift to the opposite side and repeat.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 8, ExerciseId = 4, StepNumber = 1, StepInstruction = "Set 3–4 poles at low height, spaced to match your dog's stride.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 9, ExerciseId = 4, StepNumber = 2, StepInstruction = "Walk slowly through the poles on a loose leash, rewarding calm steps.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 10, ExerciseId = 4, StepNumber = 3, StepInstruction = "Complete 3 passes, rest 30 seconds, then repeat for 2 sets.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 11, ExerciseId = 5, StepNumber = 1, StepInstruction = "Begin on a flat, non-slip surface with a short leash.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 12, ExerciseId = 5, StepNumber = 2, StepInstruction = "Walk at a slow pace for 5 minutes, encouraging even weight distribution.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 13, ExerciseId = 6, StepNumber = 1, StepInstruction = "Find a gentle incline (5–10 degrees) with good footing.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 14, ExerciseId = 6, StepNumber = 2, StepInstruction = "Walk uphill for 2 minutes, rest, then walk down slowly.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 15, ExerciseId = 7, StepNumber = 1, StepInstruction = "Place your cat on a comfortable surface and allow them to settle.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 16, ExerciseId = 7, StepNumber = 2, StepInstruction = "Gently flex each front limb, holding for 3 seconds within a pain-free range.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 17, ExerciseId = 8, StepNumber = 1, StepInstruction = "Open the crate door and lure your cat out with a treat at ground level.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 18, ExerciseId = 8, StepNumber = 2, StepInstruction = "Guide them back in slowly; repeat 5 times with rest between.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseStep { ExerciseStepId = 19, ExerciseId = 8, StepNumber = 3, StepInstruction = "End the session before your cat shows signs of fatigue or stress.", CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true });
    }

    private static void SeedRehabPrograms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RehabProgram>().HasData(
            new RehabProgram
            {
                RehabProgramId = 1,
                PhysioId = 2,
                PetId = 1,
                ProgramTitle = "Buddy Hip Recovery - Week 4",
                StartDate = ProgramStart,
                Notes = "Low-impact introductory routine for hip dysplasia recovery.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new RehabProgram
            {
                RehabProgramId = 2,
                PhysioId = 2,
                PetId = 2,
                ProgramTitle = "Luna ACL Recovery - Week 3",
                StartDate = new DateOnly(2026, 7, 6),
                Notes = "Post-surgical stifle rehabilitation with controlled loading.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new RehabProgram
            {
                RehabProgramId = 3,
                PhysioId = 2,
                PetId = 3,
                ProgramTitle = "Max Lameness Rehab",
                StartDate = new DateOnly(2026, 7, 1),
                Notes = "Proprioception and gradual return to activity.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new RehabProgram
            {
                RehabProgramId = 4,
                PhysioId = 2,
                PetId = 4,
                ProgramTitle = "Bella Weight & Mobility Plan",
                StartDate = new DateOnly(2026, 6, 15),
                Notes = "Combined weight management and low-impact exercise.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new RehabProgram
            {
                RehabProgramId = 5,
                PhysioId = 2,
                PetId = 5,
                ProgramTitle = "Whiskers Arthritis Care",
                StartDate = new DateOnly(2026, 6, 1),
                Notes = "Gentle feline mobility programme.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedRehabProgramExercises(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RehabProgramExercise>().HasData(
            new RehabProgramExercise { RehabProgramExerciseId = 1, RehabProgramId = 1, ExerciseId = 1, Repetitions = 8, Sets = 3, FrequencyPerDay = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 2, RehabProgramId = 1, ExerciseId = 2, Repetitions = 10, Sets = 2, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 3, RehabProgramId = 1, ExerciseId = 3, Repetitions = 6, Sets = 2, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 4, RehabProgramId = 2, ExerciseId = 4, Repetitions = 3, Sets = 2, FrequencyPerDay = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 5, RehabProgramId = 2, ExerciseId = 5, Repetitions = 1, Sets = 1, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 6, RehabProgramId = 3, ExerciseId = 5, Repetitions = 1, Sets = 1, FrequencyPerDay = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 7, RehabProgramId = 3, ExerciseId = 3, Repetitions = 6, Sets = 2, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 8, RehabProgramId = 4, ExerciseId = 6, Repetitions = 1, Sets = 1, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 9, RehabProgramId = 4, ExerciseId = 5, Repetitions = 1, Sets = 1, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 10, RehabProgramId = 5, ExerciseId = 7, Repetitions = 5, Sets = 2, FrequencyPerDay = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 11, RehabProgramId = 5, ExerciseId = 8, Repetitions = 5, Sets = 1, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new RehabProgramExercise { RehabProgramExerciseId = 12, RehabProgramId = 2, ExerciseId = 3, Repetitions = 6, Sets = 2, FrequencyPerDay = 1, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true });
    }

    private static void SeedDailyTrackingLogs(ModelBuilder modelBuilder)
    {
        var logs = new List<DailyTrackingLog>();
        var logId = 1;

        // Pet 1 (Buddy): pain 7→4, mobility 4→7 over 14 days
        var buddyPain = new[] { 7, 7, 6, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4 };
        var buddyMobility = new[] { 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7 };
        var buddyEnergy = new[] { 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8 };

        for (var day = 0; day < 14; day++)
        {
            logs.Add(new DailyTrackingLog
            {
                LogId = logId++,
                PetId = 1,
                LogDate = TrackingStart.AddDays(day),
                PainScore = buddyPain[day],
                MobilityScore = buddyMobility[day],
                EnergyScore = buddyEnergy[day],
                IsCompleted = true,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
        }

        // Pet 2 (Luna): improving post-ACL
        var lunaPain = new[] { 8, 7, 7, 6, 6, 6, 5, 5, 5, 4, 4, 4, 3, 3 };
        var lunaMobility = new[] { 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8 };
        var lunaEnergy = new[] { 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8 };

        for (var day = 0; day < 14; day++)
        {
            logs.Add(new DailyTrackingLog
            {
                LogId = logId++,
                PetId = 2,
                LogDate = TrackingStart.AddDays(day),
                PainScore = lunaPain[day],
                MobilityScore = lunaMobility[day],
                EnergyScore = lunaEnergy[day],
                IsCompleted = true,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
        }

        // Pet 3 (Max): gradual lameness improvement
        var maxPain = new[] { 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3 };
        var maxMobility = new[] { 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8 };
        var maxEnergy = new[] { 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 9 };

        for (var day = 0; day < 14; day++)
        {
            logs.Add(new DailyTrackingLog
            {
                LogId = logId++,
                PetId = 3,
                LogDate = TrackingStart.AddDays(day),
                PainScore = maxPain[day],
                MobilityScore = maxMobility[day],
                EnergyScore = maxEnergy[day],
                IsCompleted = true,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
        }

        modelBuilder.Entity<DailyTrackingLog>().HasData(logs);
    }

    private static void SeedAppointments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment
            {
                AppointmentId = 1,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 1,
                ScheduledDateTime = SeedAnchor.AddHours(10),
                AppointmentStatus = AppointmentStatus.Scheduled,
                ClientNotes = "Follow-up on hip mobility progress.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 2,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 2,
                ScheduledDateTime = SeedAnchor.AddHours(14),
                AppointmentStatus = AppointmentStatus.Scheduled,
                ClientNotes = "ACL recovery check — stifle stability assessment.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 3,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 3,
                ScheduledDateTime = SeedAnchor.AddHours(18),
                AppointmentStatus = AppointmentStatus.Scheduled,
                ClientNotes = "Review lameness improvement this week.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 4,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 4,
                ScheduledDateTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
                AppointmentStatus = AppointmentStatus.Completed,
                ClientNotes = "Weight check and mobility review.",
                ClinicianNotes = "Weight stable; continue incline walks.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 5,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 5,
                ScheduledDateTime = new DateTime(2026, 7, 15, 11, 0, 0, DateTimeKind.Utc),
                AppointmentStatus = AppointmentStatus.Completed,
                ClientNotes = "Arthritis management review.",
                ClinicianNotes = "Good response to gentle stretches.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 6,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 6,
                ScheduledDateTime = new DateTime(2026, 7, 10, 10, 0, 0, DateTimeKind.Utc),
                AppointmentStatus = AppointmentStatus.Cancelled,
                ClientNotes = "Post-op check — rescheduled due to travel.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 7,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 1,
                ScheduledDateTime = new DateTime(2026, 7, 5, 9, 30, 0, DateTimeKind.Utc),
                AppointmentStatus = AppointmentStatus.Completed,
                ClientNotes = "Initial hip dysplasia assessment.",
                ClinicianNotes = "Started sit-to-stand programme.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new Appointment
            {
                AppointmentId = 8,
                PhysioId = 2,
                OwnerId = 3,
                PetId = 2,
                ScheduledDateTime = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc),
                AppointmentStatus = AppointmentStatus.Scheduled,
                ClientNotes = "4-week post-surgery milestone review.",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedVideoSubmissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoSubmission>().HasData(
            new VideoSubmission
            {
                VideoSubmissionId = 1,
                PetId = 1,
                ExerciseId = 1,
                RawVideoStorageUrl = "videos/demo-buddy-sit-to-stand-raw.mp4",
                ProcessingStatus = VideoProcessingStatus.Pending,
                IsReviewed = false,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 2,
                PetId = 1,
                ExerciseId = 3,
                RawVideoStorageUrl = "videos/demo-buddy-weight-shift-raw.mp4",
                ProcessedVideoStreamingUrl = SampleVideoUrl,
                ProcessingStatus = VideoProcessingStatus.Ready,
                PhysioFeedbackNotes = "Good weight transfer — try holding each side for 3 seconds instead of 2.",
                IsReviewed = true,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 3,
                PetId = 2,
                ExerciseId = 4,
                RawVideoStorageUrl = "videos/demo-luna-cavaletti-raw.mp4",
                ProcessingStatus = VideoProcessingStatus.Pending,
                IsReviewed = false,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 4,
                PetId = 2,
                ExerciseId = 4,
                RawVideoStorageUrl = "videos/demo-luna-cavaletti-week2-raw.mp4",
                ProcessedVideoStreamingUrl = SampleVideoUrl,
                ProcessingStatus = VideoProcessingStatus.Ready,
                PhysioFeedbackNotes = "Excellent pole clearance. Increase to 4 passes next week.",
                IsReviewed = true,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 5,
                PetId = 3,
                ExerciseId = 5,
                RawVideoStorageUrl = "videos/demo-max-lead-walk-raw.mp4",
                ProcessingStatus = VideoProcessingStatus.Pending,
                IsReviewed = false,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 6,
                PetId = 4,
                ExerciseId = 6,
                RawVideoStorageUrl = "videos/demo-bella-incline-raw.mp4",
                ProcessingStatus = VideoProcessingStatus.Processing,
                IsReviewed = false,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            },
            new VideoSubmission
            {
                VideoSubmissionId = 7,
                PetId = 5,
                ExerciseId = 7,
                RawVideoStorageUrl = "videos/demo-whiskers-stretch-raw.mp4",
                ProcessingStatus = VideoProcessingStatus.Failed,
                IsReviewed = false,
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                IsActive = true
            });
    }

    private static void SeedMessageThreads(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageThread>().HasData(
            new MessageThread { MessageThreadId = 1, PetId = 1, OwnerId = 3, PhysioId = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new MessageThread { MessageThreadId = 2, PetId = 2, OwnerId = 3, PhysioId = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new MessageThread { MessageThreadId = 3, PetId = 3, OwnerId = 3, PhysioId = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new MessageThread { MessageThreadId = 4, PetId = 4, OwnerId = 3, PhysioId = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new MessageThread { MessageThreadId = 5, PetId = 5, OwnerId = 3, PhysioId = 2, CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true });
    }

    private static void SeedMessages(ModelBuilder modelBuilder)
    {
        var msgDate = SeedAnchor.AddDays(-3);

        modelBuilder.Entity<Message>().HasData(
            new Message { MessageId = 1, MessageThreadId = 1, SenderUserId = 3, Body = "Hi, Buddy seems a bit stiff after yesterday's sit-to-stand session. Is that normal?", CreatedDate = msgDate, ModifiedDate = msgDate, IsActive = true },
            new Message { MessageId = 2, MessageThreadId = 1, SenderUserId = 2, Body = "Some mild stiffness is expected in week 4. If it persists beyond 24 hours or he won't bear weight, let me know.", ReadAt = msgDate.AddHours(2), CreatedDate = msgDate.AddHours(1), ModifiedDate = msgDate.AddHours(1), IsActive = true },
            new Message { MessageId = 3, MessageThreadId = 1, SenderUserId = 3, Body = "Thanks — he's much better this morning. I'll upload today's video shortly.", ReadAt = msgDate.AddHours(4), CreatedDate = msgDate.AddHours(3), ModifiedDate = msgDate.AddHours(3), IsActive = true },
            new Message { MessageId = 4, MessageThreadId = 1, SenderUserId = 3, Body = "Uploaded the weight-shifting video — let me know what you think!", VideoSubmissionId = 2, CreatedDate = SeedAnchor.AddDays(-1), ModifiedDate = SeedAnchor.AddDays(-1), IsActive = true },
            new Message { MessageId = 5, MessageThreadId = 2, SenderUserId = 2, Body = "Luna's recovery is tracking well. Ready to add cavaletti poles this week.", ReadAt = msgDate, CreatedDate = msgDate.AddDays(-2), ModifiedDate = msgDate.AddDays(-2), IsActive = true },
            new Message { MessageId = 6, MessageThreadId = 2, SenderUserId = 3, Body = "Great! She's eager to work — I'll start with the lowest pole height.", ReadAt = msgDate.AddHours(1), CreatedDate = msgDate.AddDays(-2).AddHours(3), ModifiedDate = msgDate.AddDays(-2).AddHours(3), IsActive = true },
            new Message { MessageId = 7, MessageThreadId = 2, SenderUserId = 3, Body = "Cavaletti video uploaded. She knocked one pole on the third pass.", VideoSubmissionId = 3, CreatedDate = SeedAnchor.AddDays(-1), ModifiedDate = SeedAnchor.AddDays(-1), IsActive = true },
            new Message { MessageId = 8, MessageThreadId = 2, SenderUserId = 2, Body = "That's fine for week 3 — spacing looks good. I'll review the video today.", CreatedDate = SeedAnchor, ModifiedDate = SeedAnchor, IsActive = true },
            new Message { MessageId = 9, MessageThreadId = 3, SenderUserId = 3, Body = "Max is still favouring his left front leg on walks. Should I reduce the duration?", CreatedDate = msgDate.AddDays(-1), ModifiedDate = msgDate.AddDays(-1), IsActive = true },
            new Message { MessageId = 10, MessageThreadId = 3, SenderUserId = 2, Body = "Yes, drop to 3 minutes for now and keep surfaces flat. Upload a walk video if you can.", ReadAt = msgDate, CreatedDate = msgDate, ModifiedDate = msgDate, IsActive = true },
            new Message { MessageId = 11, MessageThreadId = 3, SenderUserId = 3, Body = "Will do — thanks for the quick reply.", ReadAt = msgDate.AddHours(1), CreatedDate = msgDate.AddHours(1), ModifiedDate = msgDate.AddHours(1), IsActive = true },
            new Message { MessageId = 12, MessageThreadId = 4, SenderUserId = 2, Body = "Bella's weight has dropped 0.3 kg since last visit. Keep up the incline walks.", ReadAt = msgDate.AddDays(-3), CreatedDate = msgDate.AddDays(-4), ModifiedDate = msgDate.AddDays(-4), IsActive = true },
            new Message { MessageId = 13, MessageThreadId = 4, SenderUserId = 3, Body = "She's enjoying the walks! Energy seems higher too.", ReadAt = msgDate.AddDays(-2), CreatedDate = msgDate.AddDays(-3), ModifiedDate = msgDate.AddDays(-3), IsActive = true },
            new Message { MessageId = 14, MessageThreadId = 5, SenderUserId = 3, Body = "Whiskers won't stay still for the stretches. Any tips?", CreatedDate = msgDate.AddDays(-2), ModifiedDate = msgDate.AddDays(-2), IsActive = true },
            new Message { MessageId = 15, MessageThreadId = 5, SenderUserId = 2, Body = "Try shorter sessions with treats after each limb. Feliway spray can help too.", ReadAt = msgDate.AddDays(-1), CreatedDate = msgDate.AddDays(-2).AddHours(4), ModifiedDate = msgDate.AddDays(-2).AddHours(4), IsActive = true },
            new Message { MessageId = 16, MessageThreadId = 5, SenderUserId = 3, Body = "That worked much better — she completed all 5 stretches today!", ReadAt = SeedAnchor.AddDays(-1), CreatedDate = SeedAnchor.AddDays(-1), ModifiedDate = SeedAnchor.AddDays(-1), IsActive = true },
            new Message { MessageId = 17, MessageThreadId = 1, SenderUserId = 2, Body = "Reviewed Buddy's weight-shifting video — nice improvement from last week.", CreatedDate = SeedAnchor, ModifiedDate = SeedAnchor, IsActive = true },
            new Message { MessageId = 18, MessageThreadId = 4, SenderUserId = 3, Body = "Uploaded an incline walk video for Bella.", VideoSubmissionId = 6, CreatedDate = SeedAnchor, ModifiedDate = SeedAnchor, IsActive = true });
    }

    private static void SeedExerciseSessionLogs(ModelBuilder modelBuilder)
    {
        var anchorDay = SeedAnchor.Date;

        modelBuilder.Entity<ExerciseSessionLog>().HasData(
            // Buddy: 1 of 2 sit-to-stand done today → reminder for 2nd session
            new ExerciseSessionLog { ExerciseSessionLogId = 1, PetId = 1, ExerciseId = 1, RehabProgramId = 1, CompletedAt = anchorDay.AddHours(8), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 2, PetId = 1, ExerciseId = 2, RehabProgramId = 1, CompletedAt = anchorDay.AddHours(8).AddMinutes(30), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 3, PetId = 1, ExerciseId = 3, RehabProgramId = 1, CompletedAt = anchorDay.AddDays(-1).AddHours(9), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            // Luna: cavaletti 1 of 2 done
            new ExerciseSessionLog { ExerciseSessionLogId = 4, PetId = 2, ExerciseId = 4, RehabProgramId = 2, CompletedAt = anchorDay.AddHours(7), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 5, PetId = 2, ExerciseId = 5, RehabProgramId = 2, CompletedAt = anchorDay.AddDays(-1).AddHours(10), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            // Max: slow walk 1 of 2 done
            new ExerciseSessionLog { ExerciseSessionLogId = 6, PetId = 3, ExerciseId = 5, RehabProgramId = 3, CompletedAt = anchorDay.AddHours(6), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 7, PetId = 3, ExerciseId = 3, RehabProgramId = 3, CompletedAt = anchorDay.AddDays(-1).AddHours(11), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            // Bella: incline walk done (1/day complete)
            new ExerciseSessionLog { ExerciseSessionLogId = 8, PetId = 4, ExerciseId = 6, RehabProgramId = 4, CompletedAt = anchorDay.AddHours(7).AddMinutes(30), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 9, PetId = 4, ExerciseId = 5, RehabProgramId = 4, CompletedAt = anchorDay.AddDays(-2).AddHours(9), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            // Whiskers: 1 of 2 stretches done
            new ExerciseSessionLog { ExerciseSessionLogId = 10, PetId = 5, ExerciseId = 7, RehabProgramId = 5, CompletedAt = anchorDay.AddHours(9), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 11, PetId = 5, ExerciseId = 8, RehabProgramId = 5, CompletedAt = anchorDay.AddDays(-1).AddHours(8), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            // Historical completions
            new ExerciseSessionLog { ExerciseSessionLogId = 12, PetId = 1, ExerciseId = 1, RehabProgramId = 1, CompletedAt = anchorDay.AddDays(-1).AddHours(8), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 13, PetId = 1, ExerciseId = 1, RehabProgramId = 1, CompletedAt = anchorDay.AddDays(-1).AddHours(17), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 14, PetId = 2, ExerciseId = 4, RehabProgramId = 2, CompletedAt = anchorDay.AddDays(-1).AddHours(7), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true },
            new ExerciseSessionLog { ExerciseSessionLogId = 15, PetId = 2, ExerciseId = 4, RehabProgramId = 2, CompletedAt = anchorDay.AddDays(-1).AddHours(16), CreatedDate = SeedDate, ModifiedDate = SeedDate, IsActive = true });
    }
}
