using System.Data;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly IDapperContext _context;

    protected BaseRepository(IDapperContext context) => _context = context;

    // These properties link directly to the DapperContext managed by the Service
    protected IDbConnection Connection => _context.Connection;
    protected IDbTransaction? Transaction => _context.Transaction;
}