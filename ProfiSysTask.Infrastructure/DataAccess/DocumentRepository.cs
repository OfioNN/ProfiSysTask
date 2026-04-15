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
                .AsNoTracking()
                .Include(d => d.Items)
                .ToListAsync();
        }
        public async Task<int> GetTotalDocumentsCountAsync(string searchText, string searchColumn) {
            var query = BuildFilterQuery(searchText, searchColumn);
            return await query.CountAsync();
        }

        public async Task<IEnumerable<Document>> GetPagedDocumentsAsync(int pageNumber, int pageSize, string searchText, string searchColumn) {
            var query = BuildFilterQuery(searchText, searchColumn);

            return await query
                .AsNoTracking()
                .Include(d => d.Items)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

        public async Task AddDocumentAsync(Document document) {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDocumentAsync(Document document) {
            var existing = await _context.Documents.FindAsync(document.Id);

            if (existing != null) {
                _context.Entry(existing).CurrentValues.SetValues(document);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteDocumentAsync(Document document) {
            var existing = await _context.Documents
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == document.Id);

            if (existing != null) {
                _context.Documents.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddItemAsync(DocumentItem item) {
            var currentMax = await _context.DocumentItems
                .Where(i => i.DocumentId == item.DocumentId)
                .Select(i => (int?)i.Ordinal)
                .MaxAsync() ?? 0;

            item.Ordinal = currentMax + 1;

            await _context.DocumentItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(DocumentItem item) {
            var existing = await _context.DocumentItems.FindAsync(item.Id);

            if (existing != null) {
                _context.Entry(existing).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteItemAsync(DocumentItem item) {
            var existing = await _context.DocumentItems.FindAsync(item.Id);

            if (existing != null) {
                _context.DocumentItems.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }


        private IQueryable<Document> BuildFilterQuery(string searchText, string searchColumn) {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText)) {
                switch (searchColumn) {
                    case "Imię":
                        query = query.Where(d => d.FirstName.Contains(searchText));
                        break;
                    case "Nazwisko":
                        query = query.Where(d => d.LastName.Contains(searchText));
                        break;
                    case "Miasto":
                        query = query.Where(d => d.City.Contains(searchText));
                        break;
                    case "Typ":
                        query = query.Where(d => d.Type.Contains(searchText));
                        break;
                    default:
                        query = query.Where(d =>
                            d.FirstName.Contains(searchText) ||
                            d.LastName.Contains(searchText) ||
                            d.City.Contains(searchText) ||
                            d.Type.Contains(searchText));
                        break;
                }
            }
            return query;
        }

    }
}
