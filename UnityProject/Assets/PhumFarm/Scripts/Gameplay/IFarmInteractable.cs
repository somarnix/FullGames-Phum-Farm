using PhumFarm;

namespace PhumFarm.Gameplay
{
    public interface IFarmInteractable
    {
        string Prompt { get; }
        void Interact(PlayerController player);
    }
}
