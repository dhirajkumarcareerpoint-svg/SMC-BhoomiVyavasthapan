namespace SMC.Application.DTOs;

public class AllocationProcessDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? NoticeNumber { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? LastDateToApply { get; set; }
    public DateTime? AuctionDate { get; set; }
    public decimal? ReserveAmount { get; set; }
    public decimal? HighestBidAmount { get; set; }
    public string? HighestBidderName { get; set; }
    public string? HighestBidderMobile { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Shera { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAllocationProcessDto
{
    public int PropertyId { get; set; }
    public string Method { get; set; } = "SarvajanikLilaw";
    public string? NoticeNumber { get; set; }
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastDateToApply { get; set; }
    public DateTime? AuctionDate { get; set; }
    public decimal? ReserveAmount { get; set; }
    public decimal? HighestBidAmount { get; set; }
    public string? HighestBidderName { get; set; }
    public string? HighestBidderMobile { get; set; }
    public string Status { get; set; } = "JahirNamaPrasiddh";
    public string? Shera { get; set; }
}

public class UpdateAllocationProcessDto : CreateAllocationProcessDto { }
