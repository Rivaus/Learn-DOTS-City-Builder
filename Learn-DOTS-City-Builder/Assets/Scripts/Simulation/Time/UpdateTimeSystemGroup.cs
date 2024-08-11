using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial class UpdateTimeSystemGroup : ComponentSystemGroup
{
    public UpdateTimeSystemGroup()
    {
        IRateManager rateManager = new RateUtils.VariableRateManager(66);
        this.RateManager = rateManager;
    }
}
