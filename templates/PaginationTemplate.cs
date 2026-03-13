// TEMPLATE — Pagination patterns. Two strategies — pick based on scenario.
// See docs/ARCHITECTURE.md Section 8 for when to use which.
//
// ─────────────────────────────────────────────────────────────────────────────
// STRATEGY 1: OFFSET-BASED — Admin / Back-office
// Use when: the user needs to jump to page N, see total count, or sort freely
// ─────────────────────────────────────────────────────────────────────────────

namespace Project.Core.DTOs;

// Request — always validated in Controller before reaching Service
public class OffsetPageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Response — always wrap list results in this before returning from Service
public class OffsetPageResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// ─────────────────────────────────────────────────────────────────────────────
// Repository — Offset-based SQL pattern
// ─────────────────────────────────────────────────────────────────────────────

/*
public async Task<(IEnumerable<WarehouseEntity> Items, int TotalCount)> GetPagedAsync(OffsetPageRequest request)
{
    const string sql = @"
        SELECT Id, Name, Location, Capacity
        FROM Warehouse
        ORDER BY Name
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM Warehouse;";

    var parameters = new
    {
        Offset   = (request.Page - 1) * request.PageSize,
        PageSize = request.PageSize
    };

    try
    {
        _logger.LogDebug("Executing GetPagedAsync — Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        using var multi = await Connection.QueryMultipleAsync(sql, parameters, Transaction);
        var items      = await multi.ReadAsync<WarehouseEntity>();
        var totalCount = await multi.ReadFirstAsync<int>();

        return (items, totalCount);
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error retrieving paged warehouses");
        throw;
    }
}
*/

// ─────────────────────────────────────────────────────────────────────────────
// STRATEGY 2: CURSOR-BASED — High-traffic / Mobile
// Use when: infinite scroll, large datasets, no need to jump to page N
// ─────────────────────────────────────────────────────────────────────────────

// Request
public class CursorPageRequest
{
    public int? Cursor { get; init; }       // ID of last seen record — null for first page
    public int Limit { get; init; } = 20;
}

// Response
public class CursorPageResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int? NextCursor { get; init; }   // null = no more pages
    public bool HasNextPage => NextCursor.HasValue;
}

// ─────────────────────────────────────────────────────────────────────────────
// Repository — Cursor-based SQL pattern
// ─────────────────────────────────────────────────────────────────────────────

/*
public async Task<(IEnumerable<WarehouseEntity> Items, int? NextCursor)> GetCursorPagedAsync(CursorPageRequest request)
{
    const string sql = @"
        SELECT TOP (@Limit) Id, Name, Location, Capacity
        FROM Warehouse
        WHERE (@Cursor IS NULL OR Id > @Cursor)
        ORDER BY Id ASC";

    var parameters = new
    {
        Cursor = request.Cursor,
        Limit  = request.Limit + 1      // fetch one extra to detect if next page exists
    };

    try
    {
        _logger.LogDebug("Executing GetCursorPagedAsync — Cursor: {Cursor}, Limit: {Limit}", request.Cursor, request.Limit);

        var items = (await Connection.QueryAsync<WarehouseEntity>(sql, parameters, Transaction)).ToList();

        int? nextCursor = null;
        if (items.Count > request.Limit)
        {
            items.RemoveAt(items.Count - 1);        // remove the extra record
            nextCursor = items[^1].Id;              // last real record's ID becomes the cursor
        }

        return (items, nextCursor);
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error retrieving cursor-paged warehouses");
        throw;
    }
}
*/

// ─────────────────────────────────────────────────────────────────────────────
// Service — same pattern for both strategies
// ─────────────────────────────────────────────────────────────────────────────

/*
// Offset example
public async Task<OffsetPageResponse<WarehouseDto>> GetWarehousesPagedAsync(OffsetPageRequest request)
{
    var (items, totalCount) = await _warehouseRepository.GetPagedAsync(request);

    return new OffsetPageResponse<WarehouseDto>
    {
        Items      = _mapper.EntitiesToDtos(items),
        Page       = request.Page,
        PageSize   = request.PageSize,
        TotalCount = totalCount
    };
}

// Cursor example
public async Task<CursorPageResponse<WarehouseDto>> GetWarehousesCursorPagedAsync(CursorPageRequest request)
{
    var (items, nextCursor) = await _warehouseRepository.GetCursorPagedAsync(request);

    return new CursorPageResponse<WarehouseDto>
    {
        Items      = _mapper.EntitiesToDtos(items),
        NextCursor = nextCursor
    };
}
*/
