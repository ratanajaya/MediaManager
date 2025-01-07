namespace CoreAPI.AL.Models.Dashboard;

public class TierFractionModel
{
    public required string Query { get; set; }
    public required string Name { get; set; }
    public int Tn { get; set; }
    public int T0 { get; set; }
    public int T1 { get; set; }
    public int T2 { get; set; }
    public int T3 { get; set; }
    public int Ts { get; set; }
}