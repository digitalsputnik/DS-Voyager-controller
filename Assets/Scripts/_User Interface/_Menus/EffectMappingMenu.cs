using VoyagerController.Mapping;

namespace VoyagerController.UI
{
    public class EffectMappingMenu : Menu
    {
        public void ExitEffectMapping()
        {
            EffectMapper.LeaveEffectMapping();
        }
    }
}