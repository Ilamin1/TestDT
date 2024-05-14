namespace TestDT.Application.Constants;

public static class DataUploadConstants
{
    public static readonly string[] AllowedColumns =
    [
        "tpep_pickup_datetime",
        "tpep_dropoff_datetime",
        "passenger_count",
        "trip_distance",
        "store_and_fwd_flag",
        "PULocationID",
        "DOLocationID",
        "fare_amount",
        "tip_amount"
    ];

    public const string DuplicatesFileName = "duplicates.csv";

    public const string FlagColumnName = "store_and_fwd_flag";
}