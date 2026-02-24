using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Not yönetim servisi
    /// </summary>
    public class NoteService
    {
        private readonly NoteRepository _noteRepository;

        public NoteService()
        {
            _noteRepository = new NoteRepository();
        }

        /// <summary>
        /// Hastanın tüm notlarını getir
        /// </summary>
        public List<Note> GetPatientNotes(int patientId)
        {
            return _noteRepository.GetByPatientId(patientId).ToList();
        }

        /// <summary>
        /// Not ekle
        /// </summary>
        public Note AddNote(int patientId, int doctorId, string doctorName, string content, NoteCategory category = NoteCategory.General)
        {
            var note = new Note
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DoctorName = doctorName,
                Content = content,
                Category = category,
                Date = DateTime.Now
            };

            int noteId = _noteRepository.Add(note);
            note.Id = noteId;

            return note;
        }

        /// <summary>
        /// Not sil
        /// </summary>
        public void DeleteNote(int noteId)
        {
            _noteRepository.Delete(noteId);
        }
    }
}

