using Riok.Mapperly.Abstractions;
using Project.Core.DTOs;
using Project.Infrastructure.Entities;

namespace Project.Core.Mappers;

[Mapper]
public partial class WarehouseMapper
{
    // Case 1: Flat mapping — property names match exactly
    public partial WarehouseDto EntityToDto(WarehouseEntity entity);

    // Case 2: List mapping — always declare explicitly, Mapperly does not generate this automatically
    public partial IEnumerable<WarehouseDto> EntitiesToDtos(IEnumerable<WarehouseEntity> entities);

    // Case 3: Property name mismatch — use [MapProperty(source, target)]
    [MapProperty(nameof(WarehouseEntity.WarehouseCode), nameof(WarehouseDto.Code))]
    public partial WarehouseDetailDto EntityToDetailDto(WarehouseEntity entity);

    // Case 4: DTO to API Response model — map between internal DTO and API contract
    public partial WarehouseResponse DtoToResponse(WarehouseDto dto);

    // Case 5: API Request model to DTO — map incoming request to internal DTO
    public partial WarehouseDto RequestToDto(WarehouseRequest request);
}