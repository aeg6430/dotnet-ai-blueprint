using System.Data;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly IDapperContext _context;

    protected BaseRepository(IDapperContext context) => _context = context;

    // Repositories always use the current scoped UoW; they never create their own connection/transaction.
    protected IDbConnection Connection => _context.Connection;
    protected IDbTransaction? Transaction => _context.Transaction;
    protected int DefaultCommandTimeoutSeconds => _context.DefaultCommandTimeoutSeconds;
}