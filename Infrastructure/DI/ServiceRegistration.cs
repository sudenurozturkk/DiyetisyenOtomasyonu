using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Services;

namespace DiyetisyenOtomasyonu.Infrastructure.DI
{
    /// <summary>
    /// Servis kayıtları - Bootstrap
    /// Tüm servisler ve repository'ler burada kaydedilir
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Tüm servisleri kaydet
        /// </summary>
        public static void RegisterServices(ServiceContainer container)
        {
            // Repositories - Transient (her istekte yeni instance)
            container.Register<PatientRepository, PatientRepository>(ServiceLifetime.Transient);
            container.Register<UserRepository, UserRepository>(ServiceLifetime.Transient);
            container.Register<DoctorRepository, DoctorRepository>(ServiceLifetime.Transient);
            container.Register<MessageRepository, MessageRepository>(ServiceLifetime.Transient);
            container.Register<GoalRepository, GoalRepository>(ServiceLifetime.Transient);
            container.Register<AppointmentRepository, AppointmentRepository>(ServiceLifetime.Transient);
            container.Register<WeightEntryRepository, WeightEntryRepository>(ServiceLifetime.Transient);
            container.Register<BodyMeasurementRepository, BodyMeasurementRepository>(ServiceLifetime.Transient);
            container.Register<NoteRepository, NoteRepository>(ServiceLifetime.Transient);
            container.Register<ExerciseTaskRepository, ExerciseTaskRepository>(ServiceLifetime.Transient);
            container.Register<MealRepository, MealRepository>(ServiceLifetime.Transient);
            container.Register<DietRepository, DietRepository>(ServiceLifetime.Transient);
            container.Register<BadgeRepository, BadgeRepository>(ServiceLifetime.Transient);

            // Services - Transient
            container.Register<PatientService, PatientService>(ServiceLifetime.Transient);
            container.Register<MessageService, MessageService>(ServiceLifetime.Transient);
            container.Register<GoalService, GoalService>(ServiceLifetime.Transient);
            container.Register<AppointmentService, AppointmentService>(ServiceLifetime.Transient);
            container.Register<MealService, MealService>(ServiceLifetime.Transient);
            container.Register<DietService, DietService>(ServiceLifetime.Transient);
            container.Register<NoteService, NoteService>(ServiceLifetime.Transient);
            container.Register<ExerciseService, ExerciseService>(ServiceLifetime.Transient);
            container.Register<ReportService, ReportService>(ServiceLifetime.Transient);
            container.Register<NotificationService, NotificationService>(ServiceLifetime.Transient);
            container.Register<SearchService, SearchService>(ServiceLifetime.Transient);
            container.Register<BadgeService, BadgeService>(ServiceLifetime.Transient);
            container.Register<DashboardService, DashboardService>(ServiceLifetime.Transient);
            container.Register<PhotoService, PhotoService>(ServiceLifetime.Transient);
            container.Register<QRCodeService, QRCodeService>(ServiceLifetime.Transient);

            // Singleton Services
            container.Register<CacheService, CacheService>(ServiceLifetime.Singleton);
        }
    }
}
