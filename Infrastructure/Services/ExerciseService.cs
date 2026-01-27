using System;
using System.Collections.Generic;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    public class ExerciseService
    {
        private readonly ExerciseTaskRepository _exerciseRepository;

        public ExerciseService()
        {
            _exerciseRepository = new ExerciseTaskRepository();
        }

        public List<ExerciseTask> GetPatientTasks(int patientId)
        {
            return _exerciseRepository.GetByPatient(patientId);
        }

        public void CompleteTask(int taskId)
        {
            var task = _exerciseRepository.GetById(taskId);
            if (task != null)
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.Now;
                _exerciseRepository.Update(task);
            }
        }
    }
}
