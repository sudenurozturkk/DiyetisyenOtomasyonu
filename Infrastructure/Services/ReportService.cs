using System;
using System.Collections.Generic;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        public List<WeightEntry> GetWeightHistory(int patientId, DateTime startDate, DateTime endDate)
        {
            return _reportRepository.GetWeightHistory(patientId, startDate, endDate);
        }
    }
}
