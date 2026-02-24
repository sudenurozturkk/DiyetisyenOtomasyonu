using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Modern Bildirim Servisi
    /// Windows Toast, E-posta, SMS bildirimleri
    /// Zamanlanmış bildirimler ve hatırlatmalar
    /// </summary>
    public class NotificationService
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly GoalRepository _goalRepository;
        private readonly MessageRepository _messageRepository;
        private readonly UserRepository _userRepository;
        private static List<ScheduledNotification> _scheduledNotifications = new List<ScheduledNotification>();

        public NotificationService()
        {
            _appointmentRepository = new AppointmentRepository();
            _goalRepository = new GoalRepository();
            _messageRepository = new MessageRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// Randevu hatırlatması gönder
        /// </summary>
        public void SendAppointmentReminder(int appointmentId, TimeSpan reminderTime)
        {
            var appointment = _appointmentRepository.GetById(appointmentId);
            if (appointment == null) return;

            var reminderDateTime = appointment.DateTime.Subtract(reminderTime);
            ScheduleNotification(reminderDateTime, $"Randevu Hatırlatması", 
                $"{appointment.DateTime:dd.MM.yyyy HH:mm} tarihinde randevunuz var.", 
                NotificationType.Appointment, appointment.PatientId);
        }

        /// <summary>
        /// Hedef hatırlatması gönder
        /// </summary>
        public void SendGoalReminder(int goalId, int userId)
        {
            var goal = _goalRepository.GetById(goalId);
            if (goal == null) return;

            var message = GetGoalReminderMessage(goal);
            ShowToastNotification(message, ToastNotification.ToastType.Info);
        }

        /// <summary>
        /// Yeni mesaj bildirimi
        /// </summary>
        public void NotifyNewMessage(int userId, string senderName, string messagePreview)
        {
            var notification = $"Yeni mesaj: {senderName}\n{messagePreview}";
            ShowToastNotification(notification, ToastNotification.ToastType.Info);
        }

        /// <summary>
        /// Zamanlanmış bildirim oluştur
        /// </summary>
        public void ScheduleNotification(DateTime scheduledTime, string title, string message, 
            NotificationType type, int? userId = null)
        {
            var notification = new ScheduledNotification
            {
                Id = _scheduledNotifications.Count + 1,
                ScheduledTime = scheduledTime,
                Title = title,
                Message = message,
                Type = type,
                UserId = userId,
                IsSent = false
            };

            _scheduledNotifications.Add(notification);
        }

        /// <summary>
        /// Zamanlanmış bildirimleri kontrol et ve gönder
        /// </summary>
        public void CheckScheduledNotifications()
        {
            var now = DateTime.Now;
            var dueNotifications = _scheduledNotifications
                .Where(n => !n.IsSent && n.ScheduledTime <= now)
                .ToList();

            foreach (var notification in dueNotifications)
            {
                ShowToastNotification($"{notification.Title}\n{notification.Message}", 
                    GetToastType(notification.Type));
                notification.IsSent = true;
            }
        }

        /// <summary>
        /// Günlük hatırlatmaları ayarla
        /// </summary>
        public void SetupDailyReminders(int userId)
        {
            var tomorrow = DateTime.Today.AddDays(1);
            
            // Su içme hatırlatması (her 2 saatte bir)
            for (int hour = 8; hour < 22; hour += 2)
            {
                ScheduleNotification(tomorrow.AddHours(hour), "Su İçme Hatırlatması", 
                    "Bir bardak su içmeyi unutmayın! 💧", NotificationType.Goal, userId);
            }

            // Öğün hatırlatması
            ScheduleNotification(tomorrow.AddHours(8), "Kahvaltı Zamanı", 
                "Kahvaltı yapmayı unutmayın! 🍳", NotificationType.Meal, userId);
            ScheduleNotification(tomorrow.AddHours(13), "Öğle Yemeği", 
                "Öğle yemeği zamanı! 🥗", NotificationType.Meal, userId);
            ScheduleNotification(tomorrow.AddHours(19), "Akşam Yemeği", 
                "Akşam yemeği zamanı! 🍽️", NotificationType.Meal, userId);
        }

        private void ShowToastNotification(string message, ToastNotification.ToastType type)
        {
            ToastNotification.Show(message, type, 5000);
        }

        private ToastNotification.ToastType GetToastType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Appointment => ToastNotification.ToastType.Info,
                NotificationType.Goal => ToastNotification.ToastType.Info,
                NotificationType.Meal => ToastNotification.ToastType.Warning,
                NotificationType.Message => ToastNotification.ToastType.Info,
                NotificationType.System => ToastNotification.ToastType.Info,
                _ => ToastNotification.ToastType.Info
            };
        }

        private string GetGoalReminderMessage(Goal goal)
        {
            var goalType = goal.GoalType switch
            {
                GoalType.Water => "Su",
                GoalType.Weight => "Kilo",
                GoalType.Steps => "Adım",
                GoalType.Sleep => "Uyku",
                GoalType.Protein => "Protein",
                _ => "Hedef"
            };

            var progress = goal.TargetValue > 0 
                ? (goal.CurrentValue / goal.TargetValue * 100).ToString("F0") 
                : "0";

            return $"{goalType} Hedefi: {progress}% tamamlandı. Devam edin! 💪";
        }
    }

    /// <summary>
    /// Zamanlanmış bildirim modeli
    /// </summary>
    public class ScheduledNotification
    {
        public int Id { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public int? UserId { get; set; }
        public bool IsSent { get; set; }
    }

    /// <summary>
    /// Bildirim tipleri
    /// </summary>
    public enum NotificationType
    {
        Appointment,
        Goal,
        Meal,
        Message,
        System
    }
}
