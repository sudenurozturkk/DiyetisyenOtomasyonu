using System;
using System.Collections.Generic;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;

        public AppointmentService()
        {
            _appointmentRepository = new AppointmentRepository();
        }

        public List<Appointment> GetPatientAppointments(int patientId)
        {
            return _appointmentRepository.GetByPatient(patientId);
        }

        public void UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = _appointmentRepository.GetById(id);
            if (appointment != null)
            {
                appointment.Status = status;
                _appointmentRepository.Update(appointment);
            }
        }
    }
}
