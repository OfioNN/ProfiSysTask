using Microsoft.EntityFrameworkCore;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Infrastructure.DataAccess {
    public class DocumentRepository : IDocumentRepository {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync() {
            return await _context.Documents
                .Include(d => d.Items)
                .ToListAsync();
        }

        public async Task SaveDocumentsAsync(IEnumerable<Document> documents) {
            await _context.Documents.AddRangeAsync(documents);
            await _context.SaveChangesAsync();
        }

        public async Task ClearDatabaseAsync() {
            _context.DocumentItems.RemoveRange(_context.DocumentItems);
            _context.Documents.RemoveRange(_context.Documents);
            await _context.SaveChangesAsync();
        }

    }
}
