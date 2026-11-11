namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// The kind of reference/master data an item represents. Kept as a single
/// extensible set so new reference lists can be added without new tables.
/// </summary>
public enum MasterDataCategory
{
    Department,
    ProductCategory,
    MaterialType,
    TestingMethod
}
